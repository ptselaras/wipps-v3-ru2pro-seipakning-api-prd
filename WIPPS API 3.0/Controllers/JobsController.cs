using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
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
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerTrait<Job>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "jobs";
        private static string plural = "jobs";
        private static string[] fields = { "id", "name", "slug", "refinery_id", "created_at", "updated_at" };
        private static string cond = "slug";

        public JobsController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<object>> GetJobs(
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

            var query = _context.Jobs.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
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

        // GET: api/Jobs/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetJob(string slug)
        {
            var query = _context.Jobs
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
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

        // PUT: api/Jobs/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutJob(string slug, RequestItem request)
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

            var query = _context.Jobs
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }

            try
            {
                Job item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.Jobs.Update(item);
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

        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PostJobSlug(string slug, RequestItem request)
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

            var query = _context.Jobs
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }

            try
            {
                Job item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.Jobs.Update(item);
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

        // POST: api/Jobs
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostJob(RequestItem request)
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

            Job job = new Job();

            try
            {
                long refinery_id = (long)user.RefineryId;

                job.Name = request.name;
                job.Slug = StringExtensions.Slugify(request.name);
                job.RefineryId = refinery_id;
                job.CreatedAt = DateTime.Now;
                job.UpdatedAt = DateTime.Now;

                _context.Jobs.Add(job);
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

            var query = await _context.Jobs.Where(x => x.Id == job.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
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

            var query = await _context.Jobs.Where(x => x.RefineryId == user.RefineryId).OrderBy(x => x.Name).ToListAsync();

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

        // DELETE: api/Jobs/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteJob(string slug)
        {
            var query = _context.Jobs
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Jobs.Remove(query);
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

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

        public class RequestItem
        {
            public string name { get; set; }
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
