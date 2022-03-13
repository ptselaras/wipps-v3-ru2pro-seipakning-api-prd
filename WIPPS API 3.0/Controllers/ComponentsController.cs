using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
using WIPPS_API_3._0.Services;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Request;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentsController : ControllerTrait<Component>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUriService uriService;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string plural = "components";
        private static string table = "components";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "slug";

        public ComponentsController(SafetymanContext context, IUriService uriService, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            this.uriService = uriService;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Components
        [HttpGet]
        public async Task<ActionResult<object>> GetComponents(
            [FromQuery(Name = "search")] string search,
            [FromQuery(Name = "order")] string order,
            [FromQuery(Name = "ordering")] string ordering,
            [FromQuery(Name = "page")] int page,
            [FromQuery(Name = "limit")] int limit
        )
        {

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            string sQuery = getQueryTable(form, table, search, user.RefineryId, order, ordering);

            var query = _context.Components.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = 0;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
                o.slug = item.Slug;
                o.created_at = item.CreatedAt;
                o.num = initNumber;
                o.created = item.CreatedAt.Value.ToString("yyyy, dd ddd HH:mm:ss");

                listDatas.Add(o);
            }

            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(page, limit);

            dynamic response = new ExpandoObject();
            response.code = 200;
            var totalRecords = listDatas.Count();

            var pageData = listDatas
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            response.data = PaginationHelper.CreatePagedResponse<Component>(pageData, validFilter, totalRecords, uriService, route);
            return Ok(response);
        }

        // GET: api/Components/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetComponent(string slug)
        {
            var query = _context.Components
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = query.Slug;
            data.refinery_id = query.RefineryId;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Components/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutComponent(string slug, RequestItem request)
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

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var query = _context.Components
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            try
            {
                Component item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.Components.Update(item);
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
            data.id = query.Id;
            data.name = request.name;
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Components
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostComponent(RequestItem request)
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

            Component component = new Component();

            try
            {
                long refinery_id = (long)user.RefineryId;

                component.Name = request.name;
                component.Slug = StringExtensions.Slugify(request.name);
                component.RefineryId = refinery_id;
                component.CreatedAt = DateTime.Now;
                component.UpdatedAt = DateTime.Now;

                _context.Components.Add(component);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";

                return StatusCode(500, error);
            }

            dynamic data = new ExpandoObject();
            data.id = component.Id;
            data.name = component.Name;
            data.slug = component.Slug;
            data.created_at = component.CreatedAt;
            data.updated_at = component.UpdatedAt;
            data.refinery_id = component.RefineryId;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        [HttpPost("list")]
        public async Task<ActionResult<object>> GetList()
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var query = await _context.Components.Where(x => x.RefineryId == user.RefineryId).OrderBy(x => x.Name).ToListAsync();

            List<object> listDatas = new List<object>();
            foreach (var value in query.ToList())
            {
                dynamic o = new ExpandoObject();

                o.id = value.Id;
                o.text = value.Name;

                string sub = "";
                string footnote = "";

                string[] splitter = value.Name.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    o.sub = splitter[0];
                    o.text = splitter[1];
                    sub = splitter[0];
                }

                string[] splitter2 = value.Name.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    o.footnote = splitter2[1];
                    o.text = splitter2[0];
                    footnote = splitter2[1];
                }

                if (!(sub != ""))
                {
                    o.sub = null;
                }

                if (!(footnote != ""))
                {
                    o.footnote = null;
                }

                listDatas.Add(o);

            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas.ToList();

            return response;
        }

        // DELETE: api/Components/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteComponent(string slug)
        {
            var query = _context.Components
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Components.Remove(query);
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

        private bool ComponentExists(long id)
        {
            return _context.Components.Any(e => e.Id == id);
        }

        public static string getQueryTable(string[] form, string table, string search, long? refinery_id, string order, string ordering)
        {
            var columnsSearch = form;

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT * FROM " + table + " WHERE ");

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

                SqlStr.Append(" AND refinery_id = " + refinery_id + "");
            }
            else
            {
                SqlStr.Append(" refinery_id = " + refinery_id + "");
            }

            string ord = "";
            if(order == "num")
            {
                ord = "created_at";
            }
            else
            {
                ord = order;
            }

            if(ord == "null")
            {
                ord = "created_at";
            }

            if (ord == "created")
            {
                ord = "created_at";
            }

            string sort = "";
            if(ordering == "true")
            {
                sort = "desc";
            }
            else
            {
                sort = "asc";
            }

            SqlStr.Append(" ORDER BY " + ord + " " + sort + " ");

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public class RequestItem
        {
            public string name { get; set; }
            public string type { get; set; }
        }

        private class Validator : AbstractValidator<RequestItem>
        {
            public Validator()
            {
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
            }
        }
    }
}
