using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPPS_API_3._0.Helpers;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/form-components")]
    [ApiController]
    public class FormComponentsController : ControllerTrait<FormComponent>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "form_components";
        private static string plural = "form-components";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "id";
        public FormComponentsController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/FormComponents
        [HttpGet]
        public async Task<ActionResult<object>> GetFormComponents(
            [FromQuery(Name = "form")] string form,
            [FromQuery(Name = "search")] string search
        )
        {
            string sQuery = getQueryTable(table, search, form);

            if (sQuery == "")
            {
                return NotFound();
            }

            var query = _context.Set<FormComponent2>()
                .FromSqlRaw(sQuery)
                .ToList();

            List<object> listDatas = new List<object>();

            int num = 1;

            foreach (var value in query)
            {
                num = num + 1;

                dynamic o = new ExpandoObject();
                o.id = value.Id;
                o.form = value.FormName;
                o.slug = value.ComponentSlug;
                o.form_id = value.FormTypeId;
                o.component_id = value.ComponentId;
                o.component = value.ComponentName;
                o.created_at = value.CreatedAt;
                o.num = num;
                o.created = value.CreatedAt.Value.ToString("ddd. yyyy, dd MMMM HH:mm:ss");

                string sub = "";
                string footnote = "";

                string[] splitter = value.ComponentName.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    o.sub = splitter[0];
                    o.text = splitter[1];
                    sub = splitter[0];
                    o.component = splitter[1];
                }

                string[] splitter2 = value.ComponentName.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    o.footnote = splitter2[1];
                    o.text = splitter2[0];
                    footnote = splitter2[1];
                }

                if (value.Order == null)
                {
                    o.order = value.Id;
                }
                else
                {
                    o.order = value.Order;
                }

                if (!(sub != ""))
                {
                    o.sub = "Others";
                }

                if (!(footnote != ""))
                {
                    o.footnote = "Others";
                }

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas;
            response.success = true;

            return response;
        }

        // GET: api/FormComponents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetFormComponent(long id)
        {
            var query = _context.FormComponents
                             .Where(x => x.Id == id)
                             .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.form_type_id = query.FormTypeId;
            data.component_id = query.ComponentId;
            data.order = query.Order;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/FormComponents/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PutFormComponent(string slug, RequestItem request)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var validator = new Validator();
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

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var query = _context.FormTypes
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            List<int> attached = new List<int>();
            List<long> updated = new List<long>();
            var detached = new ExpandoObject() as IDictionary<string, Object>;

            try
            {
                List<object> componentsIds = new List<object>();
                List<FormComponent> formComponents = _context.FormComponents.Where(x => x.FormTypeId == query.Id).ToList();

                int index = 0;

                foreach(DataComponent value in request.components)
                {
                    bool isNumeric = int.TryParse(value.component, out _);

                    if (isNumeric)
                    {
                        int findIndex = formComponents.FindIndex(o => o.Id == int.Parse(value.order) || o.Order == int.Parse(value.order));
                        if (findIndex != -1)
                        {
                            updated.Add(formComponents[findIndex].ComponentId);
                        }
                        else
                        {
                            attached.Add(int.Parse(value.component));

                            FormComponent formComponentModel = new FormComponent();
                            formComponentModel.FormTypeId = query.Id;
                            formComponentModel.ComponentId = long.Parse(value.component);
                            formComponentModel.Order = long.Parse(value.order);
                            formComponentModel.CreatedAt = DateTime.Now;
                            formComponentModel.UpdatedAt = DateTime.Now;

                            _context.FormComponents.Add(formComponentModel);

                            await _context.SaveChangesAsync();

                        }
                    }
                    else
                    {
                        Component componentModel = new Component();
                        componentModel.Name = value.component;
                        componentModel.Slug = StringExtensions.Slugify(value.component);
                        componentModel.RefineryId = (long)user.RefineryId;
                        componentModel.CreatedAt = DateTime.Now;
                        componentModel.UpdatedAt = DateTime.Now;

                        _context.Components.Add(componentModel);

                        await _context.SaveChangesAsync();

                        FormComponent formComponentModel = new FormComponent();
                        formComponentModel.FormTypeId = query.Id;
                        formComponentModel.ComponentId = componentModel.Id;
                        formComponentModel.Order = long.Parse(value.order);
                        formComponentModel.CreatedAt = DateTime.Now;
                        formComponentModel.UpdatedAt = DateTime.Now;

                        _context.FormComponents.Add(formComponentModel);

                        await _context.SaveChangesAsync();

                        attached.Add(int.Parse(formComponentModel.ComponentId.ToString()));
                    }
                    index++;
                }

                

                if(request.components.Count() < formComponents.Count())
                {

                    FormComponent formComponentDeleted = new FormComponent();

                    for (var i = 0; i < formComponents.Count; i++)
                    {
                        int findIndex = request.components.FindIndex(o => int.Parse(o.order) == formComponents[i].Id);

                        if (findIndex == -1)
                        {
                            detached.Add(i.ToString(), formComponents[i].ComponentId);

                            formComponentDeleted.ComponentId = formComponents[i].ComponentId;
                        }
                    }

                    FormComponent formComponent = _context.FormComponents.Where(x => x.ComponentId == formComponentDeleted.ComponentId && x.FormTypeId == query.Id).FirstOrDefault();

                    _context.FormComponents.Remove(formComponent);
                    await _context.SaveChangesAsync();

                }
                
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";
                error.success = false;
                return error;
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = query.Slug;
            data.file = query.File;
            data.refinery_id = query.RefineryId;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.ord = null;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.pos = "0";
            data.neg = "0";
            data.elipsis = Attributes.getElipsisAttribue(query.Name);

            dynamic success = new ExpandoObject();
            success.attached = attached;

            if (detached.Count() > 0)
            {
                success.detached = detached;
            }
            else
            {
                success.detached = new string[] { };
            }
            success.updated = updated;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = success;

            return response;
        }

        // POST: api/FormComponents
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostFormComponent(RequestItem request)
        {
            var validator = new Validator();
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

            FormComponent formComponent = new FormComponent();

            try
            {
                long refinery_id = (long)user.RefineryId;

                formComponent.FormTypeId = request.form_type_id;
                formComponent.ComponentId = request.component_id;
                formComponent.Order = null;
                formComponent.CreatedAt = DateTime.Now;
                formComponent.UpdatedAt = DateTime.Now;

                _context.FormComponents.Add(formComponent);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return error;
            }

            dynamic data = new ExpandoObject();
            data.id = formComponent.Id;
            data.form_type_id = formComponent.FormTypeId;
            data.component_id = formComponent.ComponentId;
            data.created_at = formComponent.CreatedAt;
            data.updated_at = formComponent.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        [HttpPost("list")]
        public async Task<ActionResult<object>> GetList()
        {

            var query = await _context.FormComponents.ToListAsync();

            List<object> listDatas = new List<object>();
            foreach (var value in query.ToList())
            {
                dynamic o = new ExpandoObject();

                o.id = value.Id;
                o.text = value.FormTypeId;
                listDatas.Add(o);

            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas.ToList();

            return response;
        }

        // DELETE: api/FormComponents/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FormComponent>> DeleteFormComponent(long id)
        {
            var query = _context.FormComponents
                            .Where(x => x.Id == id)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.FormComponents.Remove(query);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception)
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

        private bool FormComponentExists(long id)
        {
            return _context.FormComponents.Any(e => e.Id == id);
        }

        public string getQueryTable(string table, string search, string slug)
        {
            string[] columns = { "form_components.*", "form_types.name as FormName", "components.slug as ComponentSlug", "components.name as ComponentName" };
            string[] columnsSearch = { "form_components.id", "form_types.name", "components.name", "form_components.created_at" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN form_types ON form_types.id = form_components.form_type_id ");
            SqlStr.Append("INNER JOIN components ON components.id = form_components.component_id ");

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

                var form = _context.FormTypes.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();
                if (form != null)
                {
                    SqlStr.Append(" AND form_components.form_type_id = " + form.Id + " ");
                }
                else
                {
                    return "";
                }
            }
            else
            {
                var form = _context.FormTypes.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();
                if (form != null)
                {
                    SqlStr.Append(" form_components.form_type_id = " + form.Id + " ");
                }
                else
                {
                    return "";
                }
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public class DataComponent
        {
            public string order { get; set; }
            public string component { get; set; }
        }

        public class RequestItem
        {
            public List<DataComponent> components { get; set; }
            public string _method { get; set; }
            public int form_type_id { get; set; }
            public int component_id { get; set; }
        }

        private class Validator : AbstractValidator<RequestItem>
        {
            public Validator()
            {
                RuleFor(m => m.components).NotEmpty().WithMessage("required");
            }
        }
    }
}
