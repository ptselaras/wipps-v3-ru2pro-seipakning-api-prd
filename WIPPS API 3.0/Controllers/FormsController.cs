using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;
using EFCore.BulkExtensions;
using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using WIPPS_API_3._0.Helpers;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Services;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Request;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Route("api")]
    [ApiController]
    public class FormsController : ControllerTrait<Form>
    {
        private readonly SafetymanContext _context;
        private readonly IUriService uriService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IConverter _converter;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string plural = "forms";
        private static string table = "forms";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "slug";

        public FormsController(
            SafetymanContext context, 
            IUriService uriService, 
            IWebHostEnvironment hostingEnvironment,
            IEmailSender emailSender,
            IConverter converter
        ) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            this.uriService = uriService;
            _hostingEnvironment = hostingEnvironment;
            _emailSender = emailSender;
            _converter = converter;
        }

        // GET: api/Forms
        [HttpGet]
        public async Task<ActionResult<object>> GetForms(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var role = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == user.Id)
                                                .FirstOrDefaultAsync();

            string sQUeryTotal = getQueryTable(table, search, role.Role.Name, user.Id, user.RefineryId, order, from, to, null, null);

            var queryTotal = _context.Set<Form2>()
               .FromSqlRaw(sQUeryTotal)
               .ToList().Count();

            string sQuery = getQueryTable(table, search, role.Role.Name, user.Id, user.RefineryId, order, from, to, start, length);

            var query = _context.Set<Form2>()
                .FromSqlRaw(sQuery)
                .ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.slug = item.Slug;
                o.area = item.AreaName;
                o.company = item.CompanyName;
                o.job = item.JobName;
                o.form = item.FormName;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                var formPercentages = _context.ComponentChecklists
                    .Where(x => x.FormId == item.Id)
                    .GroupBy(x => x.FormId)
                    .Select(cc => new FormPercentage
                    {
                        FormId = cc.Key,
                        Inspection = "1",
                        Safe = Math.Round((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Unsafe = Math.Round((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Percentage = Decimal.Round((decimal)((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) / (((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) + (decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))))) * 100), 2).ToString()
                    }).ToList();

                if(formPercentages.Count() > 0)
                {
                    List<object> listPercentage = new List<object>();
                    foreach(var value in formPercentages)
                    {
                        dynamic formPercentagesObject = new ExpandoObject();
                        formPercentagesObject.form_id = value.FormId;
                        formPercentagesObject.inspection = value.Inspection;
                        formPercentagesObject.safe = value.Safe;
                        formPercentagesObject.Unsafe = value.Unsafe;
                        formPercentagesObject.percentage = value.Percentage;

                        listPercentage.Add(formPercentagesObject);
                    }

                    o.percentage = listPercentage;
                }

                listDatas.Add(o);
            }

            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(start, length);

            dynamic response = new ExpandoObject();
            response.total = queryTotal;
            var totalRecords = queryTotal;

            //var pageData = listDatas
            //    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            //    .Take(validFilter.PageSize)
            //    .ToList();

            response.data = listDatas;

            response.paginate = PaginationHelper.CreatePagedResponse<ItemInspection>(listDatas, validFilter, totalRecords, uriService, route);
            return Ok(response);
        }

        [HttpGet("forms-users")]
        public async Task<ActionResult<object>> GetHistoryUser(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to,
            [FromQuery(Name = "user")] string user,
            [FromQuery(Name = "form")] string form
        )
        {
            string sQUeryTotal = getHistoryUserQueryTable(table, search, user, form, order, from, to, null, null);

            var queryTotal = _context.Set<Form2>()
               .FromSqlRaw(sQUeryTotal)
               .ToList().Count();

            string sQuery = getHistoryUserQueryTable(table, search, user, form, order, from, to, start, length);

            var query = _context.Set<Form2>()
                .FromSqlRaw(sQuery)
                .ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.slug = item.Slug;
                o.area = item.AreaName;
                o.company = item.CompanyName;
                o.job = item.JobName;
                o.form = item.FormName;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                var formPercentages = _context.ComponentChecklists
                    .Where(x => x.FormId == item.Id)
                    .GroupBy(x => x.FormId)
                    .Select(cc => new FormPercentage
                    {
                        FormId = cc.Key,
                        Inspection = "1",
                        Safe = Math.Round((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Unsafe = Math.Round((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Percentage = Decimal.Round((decimal)((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) / (((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) + (decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))))) * 100), 2).ToString()
                        //Safe = Math.Round((decimal)cc.Sum(s => (s.SafeValue / (s.SafeValue + s.UnsafeValue))), 2).ToString(),
                        //Unsafe = Math.Round((decimal)cc.Sum(s => (s.UnsafeValue / (s.SafeValue + s.UnsafeValue))), 2).ToString(),
                        //Percentage = Decimal.Round((decimal)((decimal)cc.Sum(s => (s.SafeValue / (s.SafeValue + s.UnsafeValue))) / (((decimal)cc.Sum(s => (s.UnsafeValue / (s.SafeValue + s.UnsafeValue))) + (decimal)cc.Sum(s => (s.SafeValue / (s.SafeValue + s.UnsafeValue))))) * 100), 2).ToString()
                    }).ToList();

                if (formPercentages.Count() > 0)
                {
                    List<object> listPercentage = new List<object>();
                    foreach (var value in formPercentages)
                    {
                        dynamic formPercentagesObject = new ExpandoObject();
                        formPercentagesObject.form_id = value.FormId;
                        formPercentagesObject.inspection = value.Inspection;
                        formPercentagesObject.safe = value.Safe;
                        formPercentagesObject.Unsafe = value.Unsafe;
                        formPercentagesObject.percentage = value.Percentage;

                        listPercentage.Add(formPercentagesObject);
                    }

                    o.percentage = listPercentage;
                }

                listDatas.Add(o);
            }

            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(start, length);

            dynamic response = new ExpandoObject();
            response.total = queryTotal;
            var totalRecords = queryTotal;

            //var pageData = listDatas
            //    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            //    .Take(validFilter.PageSize)
            //    .ToList();

            response.data = listDatas;

            response.paginate = PaginationHelper.CreatePagedResponse<ItemInspection>(listDatas, validFilter, totalRecords, uriService, route);
            return Ok(response);
        }

        // GET: api/Forms/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetForm(string slug)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var query = await _context.Forms
                .Where(x => x.Slug.ToLower() == slug.ToLower())
                .Include(companies => companies.Company)
                .Include(areas => areas.Area)
                .Include(forms => forms.FormType)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(attachmants => attachmants.ComponentAttachments)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(formComponents => formComponents.FormComponent)
                .ThenInclude(components => components.Component)
                .FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            var user = await _context.Users.Where(x => x.Id == query.CreatedUserId).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.area_id = query.Area.Name;

            Job job = _context.Jobs.Where(x => x.Id == query.JobId).FirstOrDefault();
            data.job_id = job.Name;

            data.company_id = query.CompanyId;
            data.form_type_id = query.FormType.Name;
            data.name = query.Name;
            data.slug = query.Slug;
            data.created_user_id = query.CreatedUserId;
            data.updated_user_id = query.UpdatedUserId;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.equipment = query.Equipment;

            List<object> companies = new List<object>();
            dynamic dataCompanies = new ExpandoObject();
            dataCompanies.id = query.Company.Id;
            dataCompanies.name = query.Company.Name + "(" + query.Company.Type + ")";
            dataCompanies.slug = query.Company.Slug;
            dataCompanies.type = query.Company.Type;
            dataCompanies.refinery_id = query.Company.RefineryId;
            dataCompanies.created_at = query.Company.CreatedAt;
            dataCompanies.updated_at = query.Company.UpdatedAt;
            companies.Add(dataCompanies);

            data.companies = companies;
            data.creator = user.Name ?? "-";

            var percentages = _context.ComponentChecklists
                    .Where(x => x.FormId == query.Id)
                    .GroupBy(x => x.FormId)
                    .Select(cc => new FormPercentage
                    {
                        FormId = cc.Key,
                        Inspection = "1",
                        Safe = Math.Round((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Unsafe = Math.Round((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Percentage = Decimal.Round((decimal)((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) / (((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) + (decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))))) * 100), 2).ToString()
                    }).ToList();

            data.percent = percentages[0].Percentage;

            List<Components> componentsList = new List<Components>();
            foreach (var value in query.ComponentChecklists)
            {
                componentsList.Add(new Components
                {

                    note = value.Description,
                    id = value.Id,
                    safe_value = value.SafeValue,
                    unsafe_value = value.UnsafeValue,
                    status = value.Status,
                    text = value.FormComponent.Component.Name ?? "-",
                    images = value.ComponentAttachments.ToList()
                });
            }

            List<dynamic> components = new List<dynamic>();

            foreach (var value in componentsList)
            {
                List<object> images = new List<object>();

                foreach (var image in value.images)
                {
                    dynamic v = new ExpandoObject();

                    v.id = image.Id;
                    v.component_checklist_id = image.ComponentChecklistId;
                    if (image.File != "")
                    {
                        v.file = base_url + "/storage/app/public/" + image.File;
                    }
                    v.description = image.Description;
                    v.created_at = image.CreatedAt;
                    v.updated_at = image.UpdatedAt;

                    images.Add(v);
                }

                dynamic val = new ExpandoObject();
                val.note = value.note;
                val.id = value.id;
                val.safe_value = value.safe_value;
                val.unsafe_value = value.unsafe_value;
                val.status = value.status;
                val.text = value.text;
                val.images = images;

                string sub = "";
                string footnote = "";

                string[] splitter = value.text.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    val.sub = splitter[0];
                    val.text = splitter[1];
                    sub = splitter[0];
                }

                string[] splitter2 = value.text.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    val.footnote = splitter2[1];
                    val.text = splitter2[0];
                    footnote = splitter2[1];
                }

                if (!(sub != ""))
                {
                    val.sub = "Others";
                }

                if (!(footnote != ""))
                {
                    val.footnote = "Others";
                }

                components.Add(val);
            }

            data.components = components;

            dynamic dataUser = new ExpandoObject();
            dataUser.id = user.Id;
            dataUser.name = user.Name;
            dataUser.slug = user.Slug;
            dataUser.username = user.Username;
            dataUser.email = user.Email;
            dataUser.photo = user.Photo;
            dataUser.refinery_id = user.RefineryId;
            dataUser.email_verified_at = user.EmailVerifiedAt;
            dataUser.created_at = user.CreatedAt;
            dataUser.updated_at = user.UpdatedAt;

            data.user = dataUser;

            List<object> listPercentage = new List<object>();
            foreach (var value in percentages)
            {
                dynamic formPercentagesObject = new ExpandoObject();
                formPercentagesObject.form_id = value.FormId;
                formPercentagesObject.inspection = value.Inspection;
                formPercentagesObject.safe = value.Safe;
                formPercentagesObject.Unsafe = value.Unsafe;
                formPercentagesObject.percentage = value.Percentage;

                listPercentage.Add(formPercentagesObject);
            }

            data.percentage = listPercentage;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Forms/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<IActionResult> PutForm(string slug, Form request)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var query = _context.Forms
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            Form form = query;

            try
            {
                form.AreaId = request.AreaId;
                form.JobId = request.JobId;
                form.CompanyId = request.CompanyId;
                form.FormTypeId = request.FormTypeId;
                form.Name = request.Name;
                form.Slug = StringExtensions.Slugify(request.Name);
                form.UpdatedUserId = user.Id;
                form.UpdatedAt = DateTime.Now;

                _context.Forms.Update(form);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";
                error.success = false;
                return StatusCode(500, error);
            }

            dynamic data = new ExpandoObject();
            data.id = form.Id;
            data.area_id = form.AreaId;
            data.job_id = form.JobId;
            data.company_id = form.CompanyId;
            data.form_type_id = form.FormTypeId;
            data.name = form.Name;
            data.slug = form.Slug;
            data.created_user_id = form.CreatedUserId;
            data.updated_user_id = form.UpdatedUserId;
            data.created_at = form.CreatedAt;
            data.updated_at = form.UpdatedAt;
            data.equipment = form.Equipment;


            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Forms
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostForm(
            [FromBodyOrDefault]  RequestItem request,
            [FromForm] string components,
            [FromForm] string form
        )
        {
            List<DataComponent> componentss = new List<DataComponent>();
            DataForm forms = new DataForm();

            string type = "body";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                componentss = request.components;
                forms = request.form;
                type = "body";
            }
            else
            {
                componentss = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataComponent>>(components);
                forms = Newtonsoft.Json.JsonConvert.DeserializeObject<DataForm>(form);
                type = "other";
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            Form model = new Form();

            try
            {

                long refinery_id = (long)user.RefineryId;

                model.FormTypeId = forms.form_type_id;
                model.AreaId = forms.area_id;

                bool isNumeric = int.TryParse(forms.job_id, out _);

                if (isNumeric)
                {
                   model.JobId = int.Parse(forms.job_id);
                }
                else
                {
                   string jobName = string.Format(StringExtensions.UcWords(forms.job_id.Trim()).ToString());
                   Job jobModel = new Job();
                   jobModel.Name = jobName;
                   jobModel.Slug = StringExtensions.Slugify(jobName);
                   jobModel.RefineryId = refinery_id;
                   jobModel.CreatedAt = DateTime.Now;
                   jobModel.UpdatedAt = DateTime.Now;

                   _context.Jobs.Add(jobModel);
                   await _context.SaveChangesAsync();

                   model.JobId = jobModel.Id;
                }

                model.Name = forms.name;
                model.Slug = StringExtensions.Slugify(forms.name);
                model.Equipment = forms.equipment;
                model.CreatedUserId = user.Id;
                model.UpdatedUserId = user.Id;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                foreach (int val in forms.companies)
                {
                   model.CompanyId = val;
                }

                _context.Forms.Add(model);
                await _context.SaveChangesAsync();

                foreach (DataComponent value in componentss)
                {
                   //int? safe_value = 1;
                   //int? unsafe_value = 0;

                   //if (value.people != null && value.total != null)
                   //{
                   //    safe_value = value.people;
                   //    int? total = value.total;
                   //    unsafe_value = total - safe_value;
                   //}

                   int? safe_value = 0;
                   int? unsafe_value = 1;

                   if (value.people != null && value.total != null)
                   {
                       safe_value = value.people;
                       int? total = value.total;
                       unsafe_value = total - safe_value;

                       if(unsafe_value == 0)
                       {
                           unsafe_value = 1;
                       }
                   }

                   ComponentChecklist componentChecklistModel = new ComponentChecklist();
                   componentChecklistModel.FormId = model.Id;
                   componentChecklistModel.FormComponentId = value.id;
                   componentChecklistModel.Status = value.status;
                   componentChecklistModel.SafeValue = safe_value;
                   componentChecklistModel.UnsafeValue = unsafe_value;
                   componentChecklistModel.Description = value.note == null ? "" : value.note;
                   componentChecklistModel.CreatedAt = DateTime.Now;
                   componentChecklistModel.UpdatedAt = DateTime.Now;

                   _context.ComponentChecklists.Add(componentChecklistModel);
                }

                await _context.SaveChangesAsync();

                List<ComponentAttachment> componentAttachments = new List<ComponentAttachment>();
                foreach (ComponentChecklist value in model.ComponentChecklists)
                {
                   foreach (DataComponent val in componentss)
                   {
                       if (value.FormComponentId == val.id)
                       {
                           foreach (DataImage com in val.images)
                           {
                               string file = "";
                               if (type == "body")
                               {
                                   file = com.file;
                               }
                               else
                               {
                                   //var split = com.file.Split("" + user.Id + "");
                                   //file = split[0] + "\\" + user.Id + "\\" + split[1];
                                   file = com.file;
                               }
                               componentAttachments.Add(new ComponentAttachment
                               {
                                   ComponentChecklistId = value.Id,
                                   File = file,
                                   Description = "",
                                   CreatedAt = DateTime.Now,
                                   UpdatedAt = DateTime.Now
                               });
                           }
                       }
                   }
                }

                _context.BulkInsert(componentAttachments);

                await _context.SaveChangesAsync();

                var email = SendForm(model, user);

                await transaction.CommitAsync();
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return StatusCode(500, error);
            }

            dynamic data = new ExpandoObject();
            data.form_type_id = model.FormTypeId;
            data.area_id = model.AreaId;
            data.job_id = model.JobId;
            data.name = model.Name;
            data.equipment = model.Equipment;
            data.created_user_id = model.CreatedUserId;
            data.updated_user_id = model.UpdatedUserId;
            data.company_id = model.CompanyId;
            data.updated_user_id = model.UpdatedUserId;
            data.slug = model.Slug;
            data.created_at = model.CreatedAt;
            data.updated_at = model.UpdatedAt;
            data.id = model.Id;

            List<object> attachments = new List<object>();
            foreach (ComponentChecklist value in model.ComponentChecklists)
            {
                dynamic item = new ExpandoObject();
                item.id = value.Id;
                item.form_id = value.FormId;
                item.form_component_id = value.FormComponentId;
                item.status = value.Status;
                item.safe_value = value.SafeValue;
                item.unsafe_value = value.UnsafeValue;
                item.description = value.Description;
                item.created_at = value.CreatedAt;
                item.updated_at = value.UpdatedAt;

                attachments.Add(item);
            }

            data.attachments = attachments;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        // DELETE: api/Forms/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteForm(string slug)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var forms = await _context.Forms
                    .Include(x => x.ComponentChecklists)
                    .ThenInclude(x => x.ComponentAttachments)
                    .FirstOrDefaultAsync(x => x.Slug.ToLower() == slug.ToLower());

                if (forms == null)
                {
                    return NotFound("Not Found");
                }

                foreach (ComponentChecklist value in forms.ComponentChecklists)
                {
                    List<ComponentAttachment> componentAttachments = _context.ComponentAttachments.Where(x => x.ComponentChecklistId == value.Id).ToList();

                    foreach (ComponentAttachment value2 in componentAttachments)
                    {
                        _context.ComponentAttachments.Remove(value2);
                        await _context.SaveChangesAsync();
                    }

                }
                _context.ComponentChecklists.RemoveRange(forms.ComponentChecklists);
                await _context.SaveChangesAsync();

                _context.Forms.Remove(forms);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.title = "Error";
                error.Content = "Error on delete your data";
                error.data = "Error on delete your data";

                return StatusCode(500, error);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        [HttpDelete("overall-slug-user/{slug}")]
        public async Task<ActionResult<object>> OverallSlugUser(string slug)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var forms = await _context.Forms
                    .Include(x => x.ComponentChecklists)
                    .ThenInclude(x => x.ComponentAttachments)
                    .FirstOrDefaultAsync(x => x.Slug.ToLower() == slug.ToLower());

                if (forms == null)
                {
                    return NotFound("Not Found");
                }

                foreach (ComponentChecklist value in forms.ComponentChecklists)
                {
                    List<ComponentAttachment> componentAttachments = _context.ComponentAttachments.Where(x => x.ComponentChecklistId == value.Id).ToList();

                    foreach (ComponentAttachment value2 in componentAttachments)
                    {
                        _context.ComponentAttachments.Remove(value2);
                        await _context.SaveChangesAsync();
                    }

                }
                _context.ComponentChecklists.RemoveRange(forms.ComponentChecklists);
                await _context.SaveChangesAsync();

                _context.Forms.Remove(forms);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.title = "Error";
                error.Content = "Error on delete your data";
                error.data = "Error on delete your data";

                return StatusCode(500, error);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        [HttpDelete("history/{slug}")]
        public async Task<ActionResult<object>> DeleteHistory(string slug)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var forms = await _context.Forms
                    .Include(x => x.ComponentChecklists)
                    .ThenInclude(x => x.ComponentAttachments)
                    .FirstOrDefaultAsync(x => x.Slug.ToLower() == slug.ToLower());

                if (forms == null)
                {
                    return NotFound("Not Found");
                }

                foreach (ComponentChecklist value in forms.ComponentChecklists)
                {
                    List<ComponentAttachment> componentAttachments = _context.ComponentAttachments.Where(x => x.ComponentChecklistId == value.Id).ToList();

                    foreach (ComponentAttachment value2 in componentAttachments)
                    {
                        _context.ComponentAttachments.Remove(value2);
                        await _context.SaveChangesAsync();
                    }

                }
                _context.ComponentChecklists.RemoveRange(forms.ComponentChecklists);
                await _context.SaveChangesAsync();

                _context.Forms.Remove(forms);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.title = "Error";
                error.Content = "Error on delete your data";
                error.data = "Error on delete your data";

                return StatusCode(500, error);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        [HttpPost("list")]
        public async Task<ActionResult<object>> GetList()
        {
            var query = await _context.Forms.OrderBy(x => x.Id).ToListAsync();

            List<object> listDatas = new List<object>();
            foreach (var value in query.ToList())
            {
                dynamic o = new ExpandoObject();

                o.id = value.Id;
                o.text = value.Name;
                listDatas.Add(o);

            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas.ToList();

            return response;
        }

        [HttpPost("export/single")]
        public async Task<ActionResult<object>> ExportSingle(RequestExport request)
        {
            var model = await _context.Forms.Where(x => x.Slug.ToLower() == request.slug.ToLower())
                .Include(companies => companies.Company)
                .Include(areas => areas.Area)
                .ThenInclude(areasEmail => areasEmail.AreaEmails)
                .Include(forms => forms.FormType)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(attachmants => attachmants.ComponentAttachments)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(formComponents => formComponents.FormComponent)
                .ThenInclude(components => components.Component)
                .FirstOrDefaultAsync();

            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            DataReport dataReport = new DataReport();
            dataReport.area_id = model.Area.Name;

            Job job = _context.Jobs.Where(x => x.Id == model.JobId).FirstOrDefault();
            dataReport.job_id = job.Name;

            dataReport.form_type_id = model.FormType.Name;
            dataReport.companies = model.Company;

            List<Components> componentsList = new List<Components>();
            foreach (var value in model.ComponentChecklists)
            {
                componentsList.Add(new Components
                {

                    note = value.Description,
                    id = value.Id,
                    status = value.Status,
                    text = value.FormComponent.Component.Name ?? "-",
                    images = value.ComponentAttachments.ToList()
                });
            }

            List<Components> components = new List<Components>();

            foreach (var value in componentsList)
            {
                List<ComponentAttachment> images = new List<ComponentAttachment>();

                foreach (var image in value.images)
                {
                    ComponentAttachment v = new ComponentAttachment();

                    if (image.File != "")
                    {
                        v.File = base_url + "/storage/app/public/" + image.File;
                    }

                    images.Add(v);
                }

                Components val = new Components();
                val.note = value.note;
                val.id = value.id;
                val.status = value.status;
                val.text = value.text;
                val.images = images;

                string[] splitter = value.text.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    val.sub = splitter[0];
                    val.text = splitter[1];
                }

                string[] splitter2 = value.text.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    val.footnote = splitter2[1];
                    val.text = splitter2[0];
                }

                components.Add(val);
            }

            dataReport.components = components;

            var user = _context.Users.Where(x => x.Id == model.CreatedUserId).FirstOrDefault();
            dataReport.user = user.Name;

            var template = TemplateGenerator.GetMailReport(_hostingEnvironment, base_url, dataReport, user);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "WIPPS REPORT"
            };


            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = template,
                WebSettings = {
                    DefaultEncoding = "utf-8"
                },
                HeaderSettings =
                {
                    FontName = "Arial",
                    FontSize = 9,
                    Right = "Page [page] of [toPage]",
                    Line = true
                },
                FooterSettings =
                {
                    FontName = "Arial",
                    FontSize = 9,
                    Line = true,
                    Center = "WIPPS REPORT"
                }
            };

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            byte[] fileBytes;

            var result = _converter.Convert(pdf);

            using (var ms = new MemoryStream(result))
            {
                fileBytes = ms.ToArray();
            }

            var dataCreated = model.CreatedAt.Value.ToString("dd MMMM yyy");

            string path = _hostingEnvironment.WebRootPath + "/temp/document";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string file = Path.Combine(path, model.JobId + "" + dataCreated + ".pdf");

            if (fileBytes.Length > 0)
            {
                using (var stream = new FileStream(file, FileMode.Create))
                {
                    stream.Write(fileBytes, 0, fileBytes.Length);
                    stream.Flush();
                }
            }

            return Ok(JsonConvert.SerializeObject(base_url + "/storage/app/public/temp/document/" + model.JobId + "" + dataCreated + ".pdf"));
        }
        
        private bool FormExists(long id)
        {

            return _context.Forms.Any(e => e.Id == id);
        }

        private string SendForm(Form form, User user)
        {
            int statusCode = 1;

            var model = _context.Forms.Where(x => x.Id == form.Id)
                .Include(companies => companies.Company)
                .Include(areas => areas.Area)
                .ThenInclude(areasEmail => areasEmail.AreaEmails)
                .Include(forms => forms.FormType)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(attachmants => attachmants.ComponentAttachments)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(formComponents => formComponents.FormComponent)
                .ThenInclude(components => components.Component)
                .FirstOrDefault();

            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            if(model.Area.AreaEmails.Count() > 0)
            {
                DataReport dataReport = new DataReport();
                dataReport.area_id = model.Area.Name;

                Job job = _context.Jobs.Where(x => x.Id == model.JobId).FirstOrDefault();
                dataReport.job_id = job.Name;

                dataReport.form_type_id = model.FormType.Name;
                dataReport.companies = model.Company;

                List<Components> componentsList = new List<Components>();
                foreach (var value in model.ComponentChecklists)
                {
                    componentsList.Add(new Components
                    {

                        note = value.Description,
                        id = value.Id,
                        status = value.Status,
                        text = value.FormComponent.Component.Name ?? "-",
                        images = value.ComponentAttachments.ToList()
                    });
                }

                List<Components> components = new List<Components>();

                foreach (var value in componentsList)
                {
                    List<ComponentAttachment> images = new List<ComponentAttachment>();

                    foreach (var image in value.images)
                    {
                        ComponentAttachment v = new ComponentAttachment();

                        if (image.File != "")
                        {
                            v.File = base_url + "/storage/app/public/" + image.File;
                        }

                        images.Add(v);
                    }

                    Components val = new Components();
                    val.note = value.note;
                    val.id = value.id;
                    val.status = value.status;
                    val.text = value.text;
                    val.images = images;

                    string[] splitter = value.text.Split("<sub>");
                    if (splitter.Count() == 2)
                    {
                        val.sub = splitter[0];
                        val.text = splitter[1];
                    }

                    string[] splitter2 = value.text.Split("<footnote>");
                    if (splitter2.Count() == 2)
                    {
                        val.footnote = splitter2[1];
                        val.text = splitter2[0];
                    }

                    components.Add(val);
                }

                dataReport.components = components;
                dataReport.user = user.Name;

                var template = TemplateGenerator.GetMailReport(_hostingEnvironment, base_url, dataReport, user);

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "WIPPS REPORT"
                };


                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = template,
                    WebSettings = {
                    DefaultEncoding = "utf-8"
                },
                    HeaderSettings =
                {
                    FontName = "Arial",
                    FontSize = 9,
                    Right = "Page [page] of [toPage]",
                    Line = true
                },
                    FooterSettings =
                {
                    FontName = "Arial",
                    FontSize = 9,
                    Line = true,
                    Center = "WIPPS REPORT"
                }
                };

                var pdf = new HtmlToPdfDocument
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                byte[] fileBytes;

                var result = _converter.Convert(pdf);

                using (var ms = new MemoryStream(result))
                {
                    fileBytes = ms.ToArray();
                }

                Attachment attachment = new Attachment
                {
                    FileBytes = fileBytes,
                    FileName = string.Format("Report {0}.pdf", DateTime.Now.ToString("dd MMM yyy")),
                    ContentType = @"application/pdf"
                };

                List<string> receivers = new List<string>();

                foreach(var value in model.Area.AreaEmails)
                {
                    receivers.Add(value.Email);
                }

                //TemplateGenerator.GetMailMessage(base_url)
                Config config = _context.Configs.FirstOrDefault();

                try
                {
                    var message = new Message(receivers, "LAPORAN dari " + user.Name, config.EmailTemplate, _hostingEnvironment.WebRootPath + "/assets/image/logo.png", attachment);
                    _emailSender.SendEmail(message);
                }
                catch(Exception e)
                {
                    statusCode = 0;
                }
                
            }

            return statusCode == 1 ? "berhasil" : "gagal";
        }

        public string getQueryTable(string table, string search, string role, long? user_id, long? refinery_id, IEnumerable<Dictionary<string, string>> order, string from, string to, int? start, int? length)
        {
            string[] columns = { "forms.*", "areas.name as AreaName", "jobs.name as JobName", "companies.name as CompanyName", "form_types.name as FormName" };
            string[] columnsSearch = { "forms.id", "areas.name", "companies.name", "jobs.name", "areas.name", "form_types.name", "forms.created_at" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN companies ON companies.id = forms.company_id ");
            SqlStr.Append("INNER JOIN areas ON areas.id = forms.area_id ");
            SqlStr.Append("INNER JOIN jobs ON jobs.id = forms.job_id ");
            SqlStr.Append("INNER JOIN form_types ON form_types.id = forms.form_type_id ");

            SqlStr.Append(" WHERE ");

            if (search != null)
            {
                var index = 0;
                foreach (var column in columnsSearch)
                {
                    if ((columnsSearch.Length - 1) == index)
                    {
                        SqlStr.Append(column + " like '%" + search + "%' ");
                    }
                    else
                    {
                        SqlStr.Append(column + " like '%" + search + "%' OR ");
                    }

                    index++;
                }

                if ((role != "administrator") && (role != "super-admin"))
                {
                    SqlStr.Append(" forms.created_user_id = " + user_id + "");
                }
                else
                {
                    List<string> ids = getUserIdByRefinery(refinery_id);

                    SqlStr.Append(" forms.created_user_id IN (" + string.Join(", ", ids) + ")");
                }
            }
            else
            {
                if (!(role == "administrator") && !(role == "super-admin"))
                {
                    SqlStr.Append(" forms.created_user_id = " + user_id + "");
                }
                else
                {
                    List<string> ids = getUserIdByRefinery(refinery_id);

                    SqlStr.Append(" forms.created_user_id IN (" + string.Join(", ", ids) + ")");
                }

            }

            if(from != "" && to != "")
            {
                SqlStr.Append(" AND forms.created_at >= '" + from + " 00:00:00' AND forms.created_at <= '" + to + " 23:59:59' ");
            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY forms.created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY forms." + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            if (start != null && length != null)
            {
                SqlStr.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public string getHistoryUserQueryTable(string table, string search, string userSlug, string formSlug, IEnumerable<Dictionary<string, string>> order, string from, string to, int? start, int? length)
        {
            string[] columns = { "forms.*", "areas.name as AreaName", "jobs.name as JobName", "companies.name as CompanyName", "form_types.name as FormName" };
            string[] columnsSearch = { "forms.id", "areas.name", "companies.name", "jobs.name", "areas.name", "form_types.name", "forms.created_at" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN companies ON companies.id = forms.company_id ");
            SqlStr.Append("INNER JOIN areas ON areas.id = forms.area_id ");
            SqlStr.Append("INNER JOIN jobs ON jobs.id = forms.job_id ");
            SqlStr.Append("INNER JOIN form_types ON form_types.id = forms.form_type_id ");

            SqlStr.Append(" WHERE ");

            if (search != null)
            {
                var index = 0;
                foreach (var column in columnsSearch)
                {
                    if ((columnsSearch.Length - 1) == index)
                    {
                        SqlStr.Append(column + " like '%" + search + "%' ");
                    }
                    else
                    {
                        SqlStr.Append(column + " like '%" + search + "%' OR ");
                    }

                    index++;
                }
            }

            var user = _context.Users.Where(x => x.Slug.ToLower() == userSlug.ToLower()).FirstOrDefault();
            SqlStr.Append(" forms.created_user_id = " + user.Id + " ");
            var form = _context.FormTypes.Where(x => x.Slug.ToLower() == formSlug.ToLower()).FirstOrDefault();
            SqlStr.Append(" AND form_types.id = " + form.Id + " ");

            if ((from != "" && from != null) && (to != "" && to != null))
            {
                SqlStr.Append(" AND forms.created_at BETWEEN '" + from + "' AND '" + to + "' ");
            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY forms.created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY forms." + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            if(start != null && length != null)
            {
                SqlStr.AppendFormat("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);
            }

            

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        private List<string> getUserIdByRefinery(long? refinery_id)
        {
            var user = _context.Users.Where(x => x.RefineryId == refinery_id).ToList();
            List<string> list = new List<string>();
            foreach (var item in user)
            {
                list.Add(item.Id.ToString());
            }

            return list;
        }

        public class Components
        {
            public string note { get; set; }
            public long id { get; set; }
            public long? status { get; set; }
            public int? safe_value { get; set; }
            public int? unsafe_value { get; set; }
            public string text { get; set; }
            public List<ComponentAttachment> images { get; set; }
            public string sub { get; set; }
            public string footnote { get; set; }
        }

        public class DataComponent
        {
            public long id { get; set; }
            public string note { get; set; }
            public string text { get; set; }
            public List<DataImage> images { get; set; }
            public long status { get; set; }
            public int? people { get; set; }
            public int? total { get; set; }
        }

        public class DataImage
        {
            public int index { get; set; }
            public string file { get; set; }
        }

        public class DataForm
        {
            public int form_type_id { get; set; }
            public int area_id { get; set; }
            public string job_id { get; set; }
            public string name { get; set; }
            public string equipment { get; set; }
            public List<int> companies { get; set; }
        }

        public class DataReport
        {
            public string area_id { get; set; }
            public string job_id { get; set; }
            public string form_type_id { get; set; }
            public string user { get; set; }
            public Company companies { get; set; }
            public List<Components> components { get; set; }
        }

        public class RequestItem
        {
            public List<DataComponent> components { get; set; }
            public DataForm form { get; set; }
        }

        public class RequestExport
        {
            public string slug { get; set; }
        }

        private class Validator : AbstractValidator<RequestItem>
        {
            public Validator()
            {
                RuleFor(m => m.form).NotEmpty().WithMessage("required");
                RuleFor(m => m.components).NotEmpty().WithMessage("required");

            }
        }
    }
}
