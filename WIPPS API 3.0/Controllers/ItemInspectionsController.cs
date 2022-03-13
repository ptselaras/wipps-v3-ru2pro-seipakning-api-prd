using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using WIPPS_API_3._0.Controllers.Response;
using WIPPS_API_3._0.Helpers;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Services;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Request;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/item-inspections")]
    [Route("api")]
    [ApiController]
    public class ItemInspectionsController : ControllerTrait<ItemInspection>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUriService uriService;
        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "item_inspections";
        private static string plural = "item-inspections";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "slug";

        public ItemInspectionsController(SafetymanContext context, IUriService uriService, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            this.uriService = uriService;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/ItemInspections
        [HttpGet]
        public async Task<ActionResult<object>> GetItemInspections(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length
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

            string sQuery = getQueryTable(table, search, role.Role.Name, user.Id, user.RefineryId, order);

            var query = (_context.Set<ItemInspection2>()
                .FromSqlRaw(sQuery).ToList())
                .GroupBy(x => x.Barcode)
                .Select(g => new { g.Key, Data = g.ToList()})
                .ToDictionary(x => x.Key, x => x.Data);

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query.Values)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.item_id = item[0].ItemId;
                o.model = item[0].Model;
                o.brand = item[0].Brand;
                o.barcode = item[0].Barcode;
                o.area = item[0].AreaName;
                o.company = item[0].CompanyName;
                o.row_number = initNumber;

                listDatas.Add(o);
            }

            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(start, length);

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            var totalRecords = listDatas.Count();

            var pageData = listDatas
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            response.data = pageData;

            response.paginate = PaginationHelper.CreatePagedResponse<ItemInspection>(pageData, validFilter, totalRecords, uriService, route);
            return Ok(response);
        }

        [HttpGet("barcode-item-inspections")]
        public async Task<ActionResult<object>> GetBarcodeItemInspections(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length,
            [FromQuery(Name = "barcode")] string barcode
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

            string sQuery = getBarcodeQueryTable(table, search, barcode,  order);

            var query = _context.Set<ItemInspection2>()
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
                o.slug_item = item.SlugItem;
                o.item_id = item.ItemId;
                o.model = item.Model;
                o.brand = item.Brand;
                o.barcode = item.Barcode;
                o.area = item.AreaName;
                o.company = item.CompanyName;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                listDatas.Add(o);
            }

            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(start, length);

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            var totalRecords = listDatas.Count();

            List<object> listDatas2 = new List<object>();
            foreach(var item in query)
            {
                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.slug_item = item.SlugItem;
                o.item_id = item.ItemId;
                o.model = item.Model;
                o.brand = item.Brand;
                o.barcode = item.Barcode;
                o.area = item.AreaName;
                o.company = item.CompanyName;
                o.created_at = item.CreatedAt;

                listDatas2.Add(o);
            }

            var data = listDatas
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            var pageData = listDatas2
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            response.data = data;
            response.paginate = PaginationHelper.CreatePagedResponse<ItemInspection>(pageData, validFilter, totalRecords, uriService, route);
            return Ok(response);
        }

        // GET: api/ItemInspections/5
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetItemInspection(string slug)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var query = await _context.ItemInspections
                .Where(x => x.Slug.ToLower() == slug.ToLower())
                .Include(areas => areas.Area)
                .Include(companies => companies.Company)
                .Include(items => items.Item)
                .Include(x => x.ItemInspectionChecklists)
                .ThenInclude(x => x.ItemRequirement)
                .ThenInclude(x => x.Requirement)
                .Include(x => x.ItemInspectionChecklists)
                .ThenInclude(x => x.ItemInspectionChecklistAttachments)
                .Select(x => new ItemInspection()
                {
                    Area = x.Area,
                    AreaId = x.AreaId,
                    Barcode = x.Barcode,
                    Brand = x.Brand,
                    Company = x.Company,
                    CompanyId = x.CompanyId,
                    CreatedAt = x.CreatedAt,
                    CreatedUserId = x.CreatedUserId,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    Id = x.Id,
                    Inspector = x.Inspector,
                    Item = x.Item,
                    ItemId = x.ItemId,
                    ItemInspectionChecklists = x.ItemInspectionChecklists,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    Model = x.Model,
                    RefineryId = x.RefineryId,
                    Safetyman = x.Safetyman,
                    Slug = x.Slug,
                    StartDate = x.StartDate,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedUserId = x.UpdatedUserId,
                    AreaName = x.Area.Name,
                    CompanyName = x.Company.Name,
                    SlugItem = x.Item.Slug
                })
                .FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            var response = await GetData(query, base_url);

            return Ok(response);
        }

        [HttpGet("barcode/{slug}")]
        public async Task<IActionResult> GetLastInspectionBarcode(string slug)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var query = await _context.ItemInspections
                .Where(x => x.Barcode.ToLower() == slug.ToLower())
                .Include(areas => areas.Area)
                .Include(companies => companies.Company)
                .Include(items => items.Item)
                .Include(x => x.ItemInspectionChecklists)
                .ThenInclude(x => x.ItemRequirement)
                .ThenInclude(x => x.Requirement)
                .Include(x => x.ItemInspectionChecklists)
                .ThenInclude(x => x.ItemInspectionChecklistAttachments)
                .Select(x => new ItemInspection()
                {
                    Area = x.Area,
                    AreaId = x.AreaId,
                    Barcode = x.Barcode,
                    Brand = x.Brand,
                    Company = x.Company,
                    CompanyId = x.CompanyId,
                    CreatedAt = x.CreatedAt,
                    CreatedUserId = x.CreatedUserId,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    Id = x.Id,
                    Inspector = x.Inspector,
                    Item = x.Item,
                    ItemId = x.ItemId,
                    ItemInspectionChecklists = x.ItemInspectionChecklists,
                    Lat = x.Lat,
                    Lng = x.Lng,
                    Model = x.Model,
                    RefineryId = x.RefineryId,
                    Safetyman = x.Safetyman,
                    Slug = x.Slug,
                    StartDate = x.StartDate,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedUserId = x.UpdatedUserId,
                    AreaName = x.Area.Name,
                    CompanyName = x.Company.Name,
                    SlugItem = x.Item.Slug
                })
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            var response = await GetData(query, base_url);

            return Ok(response);
        }

        // PUT: api/ItemInspections/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutItemInspection(string slug, RequestAddItem request)
        {
            var validator = new ValidatorAdd();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                var errors = new ExpandoObject() as IDictionary<string, Object>;
                foreach (var error in result.Errors)
                {
                    errors.Add(error.PropertyName, new List<string> { error.ErrorMessage });
                }
                return UnprocessableEntity(new ResponseError
                {
                    errors = errors,
                    message = "The given data was invalid"
                });
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            ItemInspection model = new ItemInspection();

            try
            {
                DataItem item = request.item;
                DataForm form = request.form;
                List<DataComponent> components = request.components;

                long refinery_id = (long)user.RefineryId;

                model.AreaId = item.area_id;
                model.ItemId = item.item_id;
                model.CompanyId = item.company_id;
                model.Brand = item.brand;
                model.Model = item.model;
                model.Slug = StringExtensions.Slugify(item.brand + " " + item.model);
                model.Inspector = form.inspector;
                model.Safetyman = form.safetyman;
                model.StartDate = form.start_date;
                model.DueDate = form.due_date;
                model.Description = form.description;

                model.Barcode = slug;

                model.CreatedUserId = user.Id;
                model.UpdatedUserId = user.Id;
                model.RefineryId = refinery_id;
                model.Lat = null;
                model.Lng = null;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                model.AreaName = "";
                model.CompanyName = "";
                model.SlugItem = "";

                _context.ItemInspections.Add(model);
                await _context.SaveChangesAsync();

                List<ItemInspectionChecklist> itemInspectionChecklists = new List<ItemInspectionChecklist>();
                foreach (DataComponent value in components)
                {
                    ItemInspectionChecklist modelItemInspectionChecklist = new ItemInspectionChecklist();
                    modelItemInspectionChecklist.ItemInspectionId = model.Id;
                    modelItemInspectionChecklist.ItemRequirementId = value.id;
                    modelItemInspectionChecklist.Status = value.status;
                    modelItemInspectionChecklist.Description = value.note == null ? "" : value.note;
                    modelItemInspectionChecklist.CreatedAt = DateTime.Now;
                    modelItemInspectionChecklist.UpdatedAt = DateTime.Now;

                    _context.ItemInspectionChecklists.Add(modelItemInspectionChecklist);
                }

                await _context.SaveChangesAsync();

                List<ItemInspectionChecklistAttachment> itemInspectionChecklistAttachments = new List<ItemInspectionChecklistAttachment>();
                foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
                {
                    foreach (DataComponent val in components)
                    {
                        if (value.ItemRequirementId == val.id)
                        {
                            foreach (DataImage com in val.images)
                            {
                                itemInspectionChecklistAttachments.Add(new ItemInspectionChecklistAttachment
                                {
                                    ItemInspectionChecklistId = value.Id,
                                    File = com.file,
                                    Description = "",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }
                    }
                }

                _context.BulkInsert(itemInspectionChecklistAttachments);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return error;
            }

            dynamic data = new ExpandoObject();
            data.area_id = request.item.area_id;
            data.item_id = model.ItemId;
            data.company_id = request.item.company_id;
            data.brand = model.Brand;
            data.model = model.Model;
            data.slug = model.Slug;
            data.barcode = model.Barcode;
            data.created_user_id = model.CreatedUserId;
            data.updated_user_id = model.UpdatedUserId;
            data.description = model.Description;
            data.start_date = model.StartDate;
            data.due_date = model.DueDate;
            data.safetyman = model.Safetyman;
            data.inspector = model.Inspector;
            data.created_at = model.CreatedAt;
            data.updated_at = model.UpdatedAt;
            data.id = model.Id;

            List<object> attachments = new List<object>();
            foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
            {
                dynamic item = new ExpandoObject();
                item.id = value.Id;
                item.item_inspection_id = value.ItemInspectionId;
                item.item_requirement_id = value.ItemRequirementId;
                item.status = value.Status;
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

        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PostItemInspectionSlug(
            string slug,
            [FromBodyOrDefault] RequestAddItem request,
            [FromForm] string components,
            [FromForm] string form,
            [FromForm] string item,
            [FromForm] string _method
        )
        {
            List<DataComponent> componentss = new List<DataComponent>();
            DataForm forms = new DataForm();
            DataItem items = new DataItem();

            string type = "body";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                componentss = request.components;
                forms = request.form;
                items = request.item;
                type = "body";
            }
            else
            {
                componentss = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataComponent>>(components);
                forms = Newtonsoft.Json.JsonConvert.DeserializeObject<DataForm>(form);
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<DataItem>(item);
                type = "other";
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            ItemInspection model = new ItemInspection();

            try
            {

                long refinery_id = (long)user.RefineryId;

                model.AreaId = items.area_id;
                model.ItemId = items.item_id;
                model.CompanyId = items.company_id;
                model.Brand = items.brand;
                model.Model = items.model;
                model.Slug = StringExtensions.Slugify(items.brand + " " + items.model);
                model.Inspector = forms.inspector;
                model.Safetyman = forms.safetyman;
                model.StartDate = forms.start_date;
                model.DueDate = forms.due_date;
                model.Description = forms.description;

                model.Barcode = slug;

                model.CreatedUserId = user.Id;
                model.UpdatedUserId = user.Id;
                model.RefineryId = refinery_id;
                model.Lat = null;
                model.Lng = null;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                model.AreaName = "";
                model.CompanyName = "";
                model.SlugItem = "";

                _context.ItemInspections.Add(model);
                await _context.SaveChangesAsync();

                List<ItemInspectionChecklist> itemInspectionChecklists = new List<ItemInspectionChecklist>();
                foreach (DataComponent value in componentss)
                {
                    ItemInspectionChecklist modelItemInspectionChecklist = new ItemInspectionChecklist();
                    modelItemInspectionChecklist.ItemInspectionId = model.Id;
                    modelItemInspectionChecklist.ItemRequirementId = value.id;
                    modelItemInspectionChecklist.Status = value.status;
                    modelItemInspectionChecklist.Description = value.note == null ? "" : value.note;
                    modelItemInspectionChecklist.CreatedAt = DateTime.Now;
                    modelItemInspectionChecklist.UpdatedAt = DateTime.Now;

                    _context.ItemInspectionChecklists.Add(modelItemInspectionChecklist);
                }

                await _context.SaveChangesAsync();

                List<ItemInspectionChecklistAttachment> itemInspectionChecklistAttachments = new List<ItemInspectionChecklistAttachment>();
                foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
                {
                    foreach (DataComponent val in componentss)
                    {
                        if (value.ItemRequirementId == val.id)
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
                                    var split = com.file.Split("" + user.Id + "");
                                    file = split[0] + "\\" + user.Id + "\\" + split[1];
                                }
                                itemInspectionChecklistAttachments.Add(new ItemInspectionChecklistAttachment
                                {
                                    ItemInspectionChecklistId = value.Id,
                                    File = file,
                                    Description = "",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }
                    }
                }

                _context.BulkInsert(itemInspectionChecklistAttachments);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return error;
            }

            dynamic data = new ExpandoObject();
            data.area_id = items.area_id;
            data.item_id = model.ItemId;
            data.company_id = items.company_id;
            data.brand = model.Brand;
            data.model = model.Model;
            data.slug = model.Slug;
            data.barcode = model.Barcode;
            data.created_user_id = model.CreatedUserId;
            data.updated_user_id = model.UpdatedUserId;
            data.description = model.Description;
            data.start_date = model.StartDate;
            data.due_date = model.DueDate;
            data.safetyman = model.Safetyman;
            data.inspector = model.Inspector;
            data.created_at = model.CreatedAt;
            data.updated_at = model.UpdatedAt;
            data.id = model.Id;

            List<object> attachments = new List<object>();
            foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
            {
                dynamic o = new ExpandoObject();
                o.id = value.Id;
                o.item_inspection_id = value.ItemInspectionId;
                o.item_requirement_id = value.ItemRequirementId;
                o.status = value.Status;
                o.description = value.Description;
                o.created_at = value.CreatedAt;
                o.updated_at = value.UpdatedAt;

                attachments.Add(o);
            }

            data.attachments = attachments;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        // POST: api/ItemInspections
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostItemInspection(
            [FromBodyOrDefault] RequestAddItem request, 
            [FromForm] string components,
            [FromForm] string item,
            [FromForm] string form
        )
        {

            DataItem items = new DataItem();
            DataForm forms = new DataForm();
            List<DataComponent> componentss = new List<DataComponent>();

            string type = "body";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                items = request.item;
                forms = request.form;
                componentss = request.components;
                type = "body";
            }
            else
            {
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<DataItem>(item);
                forms = Newtonsoft.Json.JsonConvert.DeserializeObject<DataForm>(form);
                componentss = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataComponent>>(components);
                type = "other";
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            ItemInspection model = new ItemInspection();

            try
            {

                long refinery_id = (long)user.RefineryId;

                model.AreaId = items.area_id;
                model.ItemId = items.item_id;
                model.CompanyId = items.company_id;
                model.Brand = items.brand;
                model.Model = items.model;
                model.Slug = StringExtensions.Slugify(items.brand + " " + items.model);
                model.Inspector = forms.inspector;
                model.Safetyman = forms.safetyman;
                model.StartDate = forms.start_date;
                model.DueDate = forms.due_date;
                model.Description = forms.description;

                Guid barcode = Guid.NewGuid();
                model.Barcode = barcode.ToString();

                model.CreatedUserId = user.Id;
                model.UpdatedUserId = user.Id;
                model.RefineryId = refinery_id;
                model.Lat = null;
                model.Lng = null;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                model.AreaName = "";
                model.CompanyName = "";
                model.SlugItem = "";

                _context.ItemInspections.Add(model);
                await _context.SaveChangesAsync();

                List<ItemInspectionChecklist> itemInspectionChecklists = new List<ItemInspectionChecklist>();
                foreach (DataComponent value in componentss)
                {
                    ItemInspectionChecklist modelItemInspectionChecklist = new ItemInspectionChecklist();
                    modelItemInspectionChecklist.ItemInspectionId = model.Id;
                    modelItemInspectionChecklist.ItemRequirementId = value.id;
                    modelItemInspectionChecklist.Status = value.status;
                    modelItemInspectionChecklist.Description = value.note == null ? "" : value.note;
                    modelItemInspectionChecklist.CreatedAt = DateTime.Now;
                    modelItemInspectionChecklist.UpdatedAt = DateTime.Now;

                    _context.ItemInspectionChecklists.Add(modelItemInspectionChecklist);
                }

                await _context.SaveChangesAsync();

                List<ItemInspectionChecklistAttachment> itemInspectionChecklistAttachments = new List<ItemInspectionChecklistAttachment>();
                foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
                {
                    foreach (DataComponent val in componentss)
                    {
                        if (value.ItemRequirementId == val.id)
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
                                    var split = com.file.Split("" + user.Id + "");
                                    file = split[0] + "\\" + user.Id + "\\" + split[1];
                                }

                                itemInspectionChecklistAttachments.Add(new ItemInspectionChecklistAttachment
                                {
                                    ItemInspectionChecklistId = value.Id,
                                    File = file,
                                    Description = "",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }
                    }
                }

                _context.BulkInsert(itemInspectionChecklistAttachments);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return StatusCode(500, error);
            }

            dynamic data = new ExpandoObject();
            data.area_id = items.area_id;
            data.item_id = model.ItemId;
            data.company_id = items.company_id;
            data.brand = model.Brand;
            data.model = model.Model;
            data.slug = model.Slug;
            data.barcode = model.Barcode;
            data.created_user_id = model.CreatedUserId;
            data.updated_user_id = model.UpdatedUserId;
            data.description = model.Description;
            data.start_date = model.StartDate;
            data.due_date = model.DueDate;
            data.safetyman = model.Safetyman;
            data.inspector = model.Inspector;
            data.created_at = model.CreatedAt;
            data.updated_at = model.UpdatedAt;
            data.id = model.Id;

            List<object> attachments = new List<object>();
            foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
            {
                dynamic o = new ExpandoObject();
                o.id = value.Id;
                o.item_inspection_id = value.ItemInspectionId;
                o.item_requirement_id = value.ItemRequirementId;
                o.status = value.Status;
                o.description = value.Description;
                o.created_at = value.CreatedAt;
                o.updated_at = value.UpdatedAt;

                attachments.Add(o);
            }

            data.attachments = attachments;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        [HttpPost("update_old_inspection/{id}")]
        public async Task<ActionResult<object>> UpdateOldInspection(
            long id, 
            [FromBodyOrDefault] RequestUpdateItem request,
            [FromForm] string components,
            [FromForm] string description,
            [FromForm] string start_date,
            [FromForm] string due_date
        )
        {
            List<DataComponent> componentss = new List<DataComponent>();
            string desc = "";
            DateTime? s_date;
            DateTime? d_date;

            string type = "body";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                desc = request.description;
                s_date = request.start_date;
                d_date = request.due_date;
                componentss = request.components;
                type = "body";
            }
            else
            {
                desc = description;
                s_date = DateTime.Parse(start_date);
                d_date = DateTime.Parse(due_date);
                componentss = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataComponent>>(components);
                type = "other";
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var query = _context.ItemInspections
                            .Where(x => x.Id == id)
                            .Include(areas => areas.Area)
                            .Include(companies => companies.Company)
                            .Include(items => items.Item)
                            .Include(x => x.ItemInspectionChecklists)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            ItemInspection model = query;

            try
            {
                model.StartDate = s_date;
                model.DueDate = d_date;
                model.Description = desc;
                model.UpdatedAt = DateTime.Now;

                _context.ItemInspections.Update(model);
                await _context.SaveChangesAsync();

                foreach (DataComponent value in componentss)
                {
                    ItemInspectionChecklist modelItemInspectionChecklist = _context.ItemInspectionChecklists
                        .Where(x => x.ItemInspectionId == id && x.ItemRequirementId == value.id)
                        .FirstOrDefault();

                    if (modelItemInspectionChecklist != null)
                    {
                        modelItemInspectionChecklist.Status = value.status;
                        modelItemInspectionChecklist.Description = value.note == null ? "" : value.note;
                        modelItemInspectionChecklist.UpdatedAt = DateTime.Now;

                        _context.ItemInspectionChecklists.Update(modelItemInspectionChecklist);
                    }
                }

                await _context.SaveChangesAsync();

                List<ItemInspectionChecklistAttachment> itemInspectionChecklistAttachments = new List<ItemInspectionChecklistAttachment>();
                foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
                {
                    foreach (DataComponent val in componentss)
                    {
                        if (value.ItemRequirementId == val.id)
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
                                    var split = com.file.Split("" + user.Id + "");
                                    file = split[0] + "\\" + user.Id + "\\" + split[1];
                                }

                                itemInspectionChecklistAttachments.Add(new ItemInspectionChecklistAttachment
                                {
                                    ItemInspectionChecklistId = value.Id,
                                    File = file,
                                    Description = "",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }
                    }
                }

                _context.BulkInsert(itemInspectionChecklistAttachments);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return error;
            }

            dynamic data = new ExpandoObject();
            data.area_id = model.Area.Id;
            data.item_id = model.Item.Id;
            data.company_id = model.Company.Id;
            data.brand = model.Brand;
            data.model = model.Model;
            data.slug = model.Slug;
            data.barcode = model.Barcode;
            data.refinery_id = model.RefineryId;
            data.created_user_id = model.CreatedUserId;
            data.updated_user_id = user.Id;
            data.description = desc;
            data.start_date = s_date;
            data.due_date = d_date;
            data.safetyman = model.Safetyman;
            data.inspector = model.Inspector;
            data.created_at = model.CreatedAt;
            data.updated_at = model.UpdatedAt;
            data.id = model.Id;

            List<object> attachments = new List<object>();
            foreach (ItemInspectionChecklist value in model.ItemInspectionChecklists)
            {
                dynamic item = new ExpandoObject();
                item.id = value.Id;
                item.item_inspection_id = value.ItemInspectionId;
                item.item_requirement_id = value.ItemRequirementId;
                item.status = value.Status;
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

        [HttpGet("reinspection/{slug}")]
        public async Task<ActionResult<object>> GetReInspection(string slug)
        {
            var query = _context.ItemInspections.Where(x => x.Barcode.ToLower() == slug.ToLower()).OrderByDescending(x => x.Id).FirstOrDefault();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.area_id = query.AreaId;
            data.item_id = query.ItemId;
            data.company_id = query.CompanyId;
            data.brand = query.Brand;
            data.model = query.Model;
            data.slug = query.Slug;
            data.barcode = query.Barcode;
            data.refinery_id = query.RefineryId;
            data.created_user_id = query.CreatedUserId;
            data.updated_user_id = query.UpdatedUserId;
            data.lat = query.Lat;
            data.lng = query.Lng;
            data.description = query.Description;
            data.start_date = query.StartDate.Value.ToString("yyyy-MM-dd");
            data.due_date = query.DueDate.Value.ToString("yyyy-MM-dd");
            data.safetyman = query.Safetyman;
            data.inspector = query.Inspector;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            return data;
        }

        // DELETE: api/ItemInspections/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteItemInspection(string slug)
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
                var itemInspections = await _context.ItemInspections
                    .Include(x => x.ItemInspectionChecklists)
                    .ThenInclude(x => x.ItemInspectionChecklistAttachments)
                    .FirstOrDefaultAsync(x => x.Slug.ToLower() == slug.ToLower());

                if (itemInspections == null)
                {
                    return NotFound("Not Found");
                }

                foreach(ItemInspectionChecklist value in itemInspections.ItemInspectionChecklists)
                {
                    List<ItemInspectionChecklistAttachment> checklistAttachment = _context.ItemInspectionChecklistAttachments.Where(x => x.ItemInspectionChecklistId == value.Id).ToList();
                    
                    foreach(ItemInspectionChecklistAttachment value2 in checklistAttachment)
                    {
                        _context.ItemInspectionChecklistAttachments.Remove(value2);
                        await _context.SaveChangesAsync();
                    }
                    
                }
                _context.ItemInspectionChecklists.RemoveRange(itemInspections.ItemInspectionChecklists);
                await _context.SaveChangesAsync();

                _context.ItemInspections.Remove(itemInspections);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return error;
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        private bool ItemInspectionExists(long id)
        {
            return _context.ItemInspections.Any(e => e.Id == id);
        }

        public string getQueryTable(string table, string search, string role, long? user_id, long? refinery_id, IEnumerable<Dictionary<string, string>> order)
        {
            string[] columns = { "item_inspections.*", "areas.name as AreaName", "areas.name as SlugItem", "companies.name as CompanyName" };
            string[] columnsSearch = { "item_inspections.item_id", "item_inspections.model", "item_inspections.brand", "item_inspections.barcode", "areas.name", "companies.name" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT "+string.Join(", ", columns)+" FROM " + table + " ");

            SqlStr.Append("INNER JOIN companies ON companies.id = item_inspections.company_id ");
            SqlStr.Append("INNER JOIN areas ON areas.id = item_inspections.area_id ");

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

                if (!(role == "administrator") && !(role == "super-admin"))
                {
                    SqlStr.Append(" item_inspections.created_user_id = " + user_id + "");
                }
                else
                {
                    List<string> ids = getUserIdByRefinery(refinery_id);

                    SqlStr.Append(" item_inspections.created_user_id IN (" + string.Join(", ", ids) + ")");
                }
            }
            else
            {
                if (!(role == "administrator") && !(role == "super-admin"))
                {
                    SqlStr.Append(" item_inspections.created_user_id = " + user_id + "");
                }
                else
                {
                    List<string> ids = getUserIdByRefinery(refinery_id);

                    SqlStr.Append(" item_inspections.created_user_id IN (" + string.Join(", ", ids) + ")");
                }

            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY item_inspections.created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY item_inspections." + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public string getBarcodeQueryTable(string table, string search, string barcode, IEnumerable<Dictionary<string, string>> order)
        {
            string[] columns = { "item_inspections.*", "items.slug as SlugItem", "areas.name as AreaName", "companies.name as CompanyName" };
            string[] columnsSearch = { "item_inspections.id", "items.slug", "item_inspections.item_id", "item_inspections.slug", "areas.name", "companies.name", "item_inspections.brand", "item_inspections.model", "item_inspections.barcode", "item_inspections.created_at" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN companies ON companies.id = item_inspections.company_id ");
            SqlStr.Append("INNER JOIN areas ON areas.id = item_inspections.area_id ");
            SqlStr.Append("INNER JOIN items ON items.id = item_inspections.item_id ");

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

                SqlStr.Append(" item_inspections.barcode = '" + barcode + "'");
            }
            else
            {
                SqlStr.Append(" item_inspections.barcode = '" + barcode + "'");

            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY item_inspections.created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY item_inspections." + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        private List<string> getUserIdByRefinery(long? refinery_id)
        {
            var user =  _context.Users.Where(x => x.RefineryId == refinery_id).ToList();
            List<string> list = new List<string>();
            foreach(var item in user)
            {
                list.Add(item.Id.ToString());
            }

            return list;
        }

        private async Task<object> GetData(ItemInspection query, string base_url)
        {
            var user = await _context.Users.Where(x => x.Id == query.CreatedUserId).FirstOrDefaultAsync();

            List<Components> componentsList = new List<Components>();
            foreach (var value in query.ItemInspectionChecklists)
            {
                componentsList.Add(new Components
                {

                    note = value.Description,
                    id = value.Id,
                    item_requirement_id = value.ItemRequirementId,
                    status = value.Status,
                    text = value.ItemRequirement.Requirement.Name ?? "-",
                    images = value.ItemInspectionChecklistAttachments.ToList()
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
                    v.item_inspection_checklist_id = image.ItemInspectionChecklistId;
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
                val.item_requirement_id = value.item_requirement_id;
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

            dynamic dataCompanies = new ExpandoObject();
            dataCompanies.id = query.Company.Id;
            dataCompanies.name = query.Company.Name;
            dataCompanies.slug = query.Company.Slug;
            dataCompanies.type = query.Company.Type;
            dataCompanies.refinery_id = query.Company.RefineryId;
            dataCompanies.created_at = query.Company.CreatedAt;
            dataCompanies.updated_at = query.Company.UpdatedAt;

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

            dynamic dataItem = new ExpandoObject();
            dataItem.id = query.Item.Id;
            dataItem.name = query.Item.Name;
            dataItem.slug = query.Item.Slug;
            dataItem.file = query.Item.File;
            dataItem.refinery_id = query.Item.RefineryId;
            dataItem.created_at = query.Item.CreatedAt;
            dataItem.updated_at = query.Item.UpdatedAt;
            dataItem.percentage = "0.00";
            dataItem.percentagen = "0.00";
            dataItem.total = "0.00";
            dataItem.pos = "0";
            dataItem.neg = "0";
            dataItem.elipsis = Attributes.getElipsisAttribue(query.Item.Name);

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.area_id = query.AreaName;
            data.item_id = query.ItemId;
            data.company_id = query.CompanyName;
            data.brand = query.Brand;
            data.model = query.Model;
            data.slug = query.Slug;
            data.barcode = query.Barcode;
            data.refinery_id = query.RefineryId;
            data.created_user_id = query.CreatedUserId;
            data.updated_user_id = query.UpdatedUserId;
            data.lat = query.Lat;
            data.lng = query.Lng;
            data.description = query.Description;
            data.start_date = query.StartDate;
            data.due_date = query.DueDate;
            data.safetyman = query.Safetyman;
            data.inspector = query.Inspector;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.creator = user.Name ?? "-";
            data.components = components;
            data.companies = dataCompanies;
            data.user = dataUser;
            data.item = dataItem;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        public class DataItem
        {
            public int area_id { get; set; }
            public string brand { get; set; }
            public int company_id { get; set; }
            public int item_id { get; set; }
            public string model { get; set; }
        }

        public class DataComponent
        {
            public long id { get; set; }
            public string note { get; set; }
            public List<DataImage> images { get; set; }
            public long status { get; set; }
        }

        public class DataImage
        {
            public int index { get; set; }
            public string file { get; set; }
        }

        public class DataForm
        {
            public string description { get; set; }
            public DateTime? start_date { get; set; }
            public DateTime? due_date { get; set; }
            public string safetyman { get; set; }
            public string inspector { get; set; }
        }

        public class RequestAddItem
        {
            public DataItem item { get; set; }
            public List<DataComponent> components { get; set; }
            public DataForm form { get; set; }
            //public string _method { get; set; }
        }
        public class RequestUpdateItem
        {
            public string description { get; set; }
            public DateTime? start_date { get; set; }
            public DateTime? due_date { get; set; }
            public List<DataComponent> components { get; set; }
        }
        private class Components
        {
            public string note { get; set; }
            public long id { get; set; }
            public long item_requirement_id { get; set; }
            public long status { get; set; }
            public string text { get; set; }
            public List<ItemInspectionChecklistAttachment> images { get; set; }
            public string sub { get; set; }
            public string footnote { get; set; }
        }

        private class ValidatorAdd : AbstractValidator<RequestAddItem>
        {
            public ValidatorAdd()
            {
                RuleFor(m => m.item).NotEmpty().WithMessage("required");
                RuleFor(m => m.components).NotEmpty().WithMessage("required");
           
            }
        }

        private class ValidatorUpdate : AbstractValidator<RequestUpdateItem>
        {
            public ValidatorUpdate()
            {
                RuleFor(m => m.components).NotEmpty().WithMessage("required");
            }
        }
    }
}
