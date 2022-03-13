using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Response.General;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
    public class AreasController : ControllerTrait<Area>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "areas";
        private static string plural = "areas";
        private static string[] fields = { "id", "name", "slug", "refinery_id", "created_at", "updated_at" };
        private static string cond = "slug";
        public AreasController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/areas
        [HttpGet]
        public async Task<ActionResult<object>> GetAreas(
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

            var user = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            string sQuery = Attributes.getQueryTable(form, table, search, user.RefineryId, order);

            var query = _context.Areas.FromSqlRaw(sQuery).ToList();

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
                o.percentage = "0.00";
                o.percentagen = "0.00";
                o.total = "0.00";

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas.Skip(start).Take(length).ToList();

            return response;
        }

        // GET: api/areas/marine
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetArea(string slug)
        {

            var query = _context.Areas
                            .Where(x => x.Slug == slug)
                            .Include(emails => emails.AreaEmails)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            DetailData data = new DetailData();
            data.name = query.Name;
            data.email = new List<string>();
            foreach(var email in query.AreaEmails.ToList())
            {
                data.email.Add(email.Email);
            }
            data.emailString = string.Join(", ", data.email);

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Areas/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutArea(string slug, RequestArea request)
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

            var query = _context.Areas
                            .Where(x => x.Slug == slug)
                            .Include(emails => emails.AreaEmails)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            try
            {
                
                List<string> mails = request.email;

                List<AreaEmail> deleteMails = new List<AreaEmail>();
                List<AreaEmail> newMails = new List<AreaEmail>();

                foreach (var value in query.AreaEmails.ToList())
                {
                    bool toDelete = true;
                    for (var i = 0; i < mails.Count; i++)
                    {
                        if (mails[i] == value.Email)
                        {
                            toDelete = false;
                        }
                    }

                    if (toDelete)
                    {
                        deleteMails.Add(new AreaEmail()
                        {
                            Id = value.Id,
                            AreaId = value.AreaId,
                            Email = value.Email
                        });
                    }
                }

                _context.BulkDelete(deleteMails);

                Area area = query;
                area.Name = request.name;
                area.Slug = StringExtensions.Slugify(request.name);
                area.UpdatedAt = DateTime.Now;

                _context.Areas.Update(area);
                await _context.SaveChangesAsync();

                foreach (var value in mails)
                {
                    bool toAdd = true;
                    foreach(var val in query.AreaEmails.ToList())
                    {
                        if (val.Email == value)
                        {
                            toAdd = false;
                        }
                    }

                    if (toAdd)
                    {
                        AreaEmail areaEmail = new AreaEmail();
                        areaEmail.AreaId = query.Id;
                        areaEmail.Email = value;
                        areaEmail.CreatedAt = DateTime.Now;
                        areaEmail.UpdatedAt = DateTime.Now;

                        newMails.Add(areaEmail);
                    }
                }

                _context.BulkInsert(newMails);

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

            List<Email> emails = new List<Email>();
            foreach(var val in query.AreaEmails.ToList())
            {
                emails.Add(new Email()
                {
                    id = val.Id,
                    area_id = val.AreaId,
                    email = val.Email,
                    created_at = val.CreatedAt,
                    updated_at = val.UpdatedAt
                });
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.emails = emails;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Areas
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostArea(RequestArea request)
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

            var user = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            Area area = new Area();

            try
            {
                long refinery_id = (long)user.RefineryId;

                area.Name = request.name;
                area.Slug = StringExtensions.Slugify(request.name);
                area.RefineryId = refinery_id;
                area.CreatedAt = DateTime.Now;
                area.UpdatedAt = DateTime.Now;

                _context.Areas.Add(area);
                await _context.SaveChangesAsync();

                List<AreaEmail> areaEmails = new List<AreaEmail>();

                foreach(var email in request.email)
                {
                    AreaEmail areaEmail = new AreaEmail();
                    areaEmail.AreaId = area.Id;
                    areaEmail.Email = email;
                    areaEmail.CreatedAt = DateTime.Now;
                    areaEmail.UpdatedAt = DateTime.Now;

                    areaEmails.Add(areaEmail);
                }

                _context.BulkInsert(areaEmails);

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

            var query = await _context.Areas.Where(x => x.Id == area.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = query.Slug;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";

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

            var user = await _context.Users.Where( x => x.Username == username).FirstOrDefaultAsync();

            var query = await _context.Areas.Where(x => x.RefineryId == user.RefineryId).ToListAsync();

            List<object> listDatas = new List<object>();
            foreach(var value in query.ToList())
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

        // DELETE: api/Areas/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteArea(string slug)
        {
            var query = _context.Areas
                            .Where(x => x.Slug == slug)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                List< AreaEmail> areaEmails = _context.AreaEmails.Where(x => x.AreaId == query.Id).ToList();

                foreach(AreaEmail value in areaEmails)
                {
                    _context.AreaEmails.Remove(value);
                    await _context.SaveChangesAsync();
                }

                _context.Areas.Remove(query);
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

        private bool AreaExists(long id)
        {
            return _context.Areas.Any(e => e.Id == id);
        }

        public class RequestArea
        {
            public string name { get; set; }
            public List<string> email { get; set; }
        }

        public class DetailData
        {
            public string name { get; set; }
            public List<string> email { get; set; }
            public string emailString { get; set; }

        }

        public class Email
        {
            public long id { get; set; }
            public long area_id { get; set; }
            public string email { get; set; }
            public DateTime? created_at { get; set; }
            public DateTime? updated_at { get; set; }
        }

        private class Validator : AbstractValidator<RequestArea>
        {
            public Validator()
            {
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
                RuleFor(m => m.email).NotEmpty().WithMessage("required");
            }
        }
    }

    
}
