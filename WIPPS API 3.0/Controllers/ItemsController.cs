using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPPS_API_3._0.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static WIPPS_API_3._0.Controllers.Response.General;
using WIPPS_API_3._0.Utils;
using System.Text;
using FluentValidation;
using System.Dynamic;
using WIPPS_API_3._0.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerTrait<Item>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "items";
        private static string plural = "items";
        private static string[] fields = { "id", "name", "slug", "refinery_id", "created_at", "updated_at" };
        private static string cond = "slug";

        public ItemsController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<object>> GetItems(
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

            var query = _context.Items.FromSqlRaw(sQuery).ToList();

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
                o.elipsis = Attributes.getElipsisAttribue(item.Name);
                o.row_number = initNumber;
                o.percentage = "0.00";
                o.percentagen = "0.00";
                o.total = "0.00";
                o.neg = "0";
                o.pos = "0";

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas.Skip(start).Take(length).ToList();

            return response;
        }

        // GET: api/Items/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetItem(string slug)
        {
            var query = _context.Items
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.elipsis = Attributes.getElipsisAttribue(query.Name);
            data.file = "";
            data.slug = query.Slug;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.neg = "0";
            data.pos = "0";

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpGet("form")]
        public async Task<ActionResult<object>> GetForm()
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            long refinery_id = (long)user.RefineryId;

            var query = await _context.Items
                                       .Where(x => x.RefineryId == refinery_id)
                                       .OrderByDescending(x => x.Id)
                                       .ToListAsync();

            List<object> data = new List<object>();

            int num = 1;
            foreach (var value in query)
            {
                dynamic o = new ExpandoObject();
                o.num = num;
                o.id = value.Id;
                o.name = value.Name;
                o.elipsis = Attributes.getElipsisAttribue(value.Name);
                o.file = "";
                o.slug = value.Slug;
                o.created = value.CreatedAt.Value.ToString("yyyy, dd ddd HH:mm:ss");
                o.percentage = "0.00";
                o.percentagen = "0.00";
                o.total = "0.00";
                o.neg = "0";
                o.pos = "0";

                num += 1;

                data.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = data.Count() > 0 ? true : false;

            return response;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutItem(string slug, RequestItem request)
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

            var query = _context.Items
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            try
            {
                Item item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.Items.Update(item);
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
            data.elipsis = Attributes.getElipsisAttribue(query.Name);
            data.file = "";
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.neg = "0";
            data.pos = "0";

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PostItemSlug(string slug, RequestItem request)
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

            var query = _context.Items
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            try
            {
                Item item = query;
                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.Items.Update(item);
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
            data.elipsis = Attributes.getElipsisAttribue(query.Name);
            data.file = "";
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.neg = "0";
            data.pos = "0";

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Items
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostItem(RequestItem request)
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

            Item item = new Item();

            try
            {
                long refinery_id = (long) user.RefineryId;

                item.Name = request.name;
                item.Slug = StringExtensions.Slugify(request.name);
                item.RefineryId = refinery_id;
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
                item.File = "";

                _context.Items.Add(item);
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

            var query = await _context.Items.Where(x => x.Id == item.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = request.name;
            data.elipsis = Attributes.getElipsisAttribue(query.Name);
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.neg = "0";
            data.pos = "0";

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

            var query = await _context.Items.Where(x => x.RefineryId == user.RefineryId).OrderBy(x => x.Name).ToListAsync();

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

        // DELETE: api/Items/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteItem(string slug)
        {
            var query = _context.Items
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Items.Remove(query);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.title = "Error";
                error.content = "app.user_by_another_data";
                error.data = "Error on delete your data";
                error.success = false;

                return Ok(error);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        private bool ItemExists(long id)
        {
            return _context.Items.Any(e => e.Id == id);
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
