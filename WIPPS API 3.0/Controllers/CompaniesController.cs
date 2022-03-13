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
using Toycloud.AspNetCore.Mvc.ModelBinding;
using WIPPS_API_3._0.Helpers;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerTrait<Company>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "type", "slug", "created_at" };
        private static string table = "companies";
        private static string plural = "companies";
        private static string[] fields = { "id", "name", "type", "slug", "refinery_id", "created_at", "updated_at" };
        private static string cond = "slug";

        public CompaniesController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<object>> GetCompanies(
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

            string sQuery = Attributes.getQueryTable(form, table, search, user.RefineryId, order);

            var query = _context.Companies.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
                o.type = item.Type;
                o.slug = item.Slug;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas.Skip(start).Take(length).ToList();

            return response;
        }

        // GET: api/Companies/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetCompany(string slug)
        {
            var query = _context.Companies
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.type = query.Type;
            data.slug = query.Slug;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutCompany(string slug, RequestItem request)
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

            var query = _context.Companies
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            try
            {
                Company item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;
                item.Type = request.type;

                _context.Companies.Update(item);
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
            data.type = request.type;
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

        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PostCompanySlug(string slug, RequestItem request)
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

            var query = _context.Companies
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            try
            {
                Company item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;
                item.Type = request.type;

                _context.Companies.Update(item);
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
            data.type = request.type;
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

        // POST: api/Companies
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostCompany(RequestItem request)
        {

            var validator = new Validator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                var errors = new ExpandoObject() as IDictionary<string, Object>;
                foreach(var error in result.Errors)
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

            Company company = new Company();

            try
            {
                long refinery_id = (long)user.RefineryId;

                company.Name = request.name;
                company.Slug = StringExtensions.Slugify(request.name);
                company.RefineryId = refinery_id;
                company.CreatedAt = DateTime.Now;
                company.UpdatedAt = DateTime.Now;
                company.Type = request.type;

                _context.Companies.Add(company);
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

            var query = await _context.Companies.Where(x => x.Id == company.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.type = query.Type;
            data.slug = query.Slug;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;

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

            var query = await _context.Companies.Where(x => x.RefineryId == user.RefineryId).OrderBy(x => x.Name).ToListAsync();

            List<object> listDatas = new List<object>();
            foreach (var value in query.ToList())
            {
                dynamic o = new ExpandoObject();

                o.id = value.Id;
                o.text = value.Name + " (" + value.Type + ")";
                listDatas.Add(o);

            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas.ToList();

            return response;
        }

        [HttpPost("list-type")]
        public async Task<ActionResult<object>> GetListByType([FromBodyOrDefault] RequestItem request, [FromQuery(Name = "type")] string type)
        {
            string types = "";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if(content.Count > 0)
            {
                if (content[0] == "application/json")
                {
                    types = request.type;
                }
            }
            else
            {
                types = type;
            }

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var query = await _context.Companies.Where(x => x.RefineryId == user.RefineryId && x.Type == types).OrderBy(x => x.Name).ToListAsync();

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

        // DELETE: api/Companies/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteCompany(string slug)
        {
            var query = _context.Companies
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Companies.Remove(query);
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

        private bool CompanyExists(long id)
        {
            return _context.Companies.Any(e => e.Id == id);
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
                RuleFor(m => m.type).NotEmpty().WithMessage("required");
            }
        }
    }
}
