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
    public class RolesController : ControllerTrait<Role>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "guar_name", "created_at" };
        private static string table = "roles";
        private static string plural = "roles";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "name";

        public RolesController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<object>> GetRoles(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length
        )
        {
            string sQuery = Attributes.getQueryTable(form, table, search, null, order);

            var query = _context.Roles.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
                o.guard_name = item.GuardName;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas.Skip(start).Take(length).ToList();

            return response;
        }

        // GET: api/Roles/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetRole(string slug)
        {
            var query = _context.Roles
                            .Where(x => x.Name.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.guard_name = query.GuardName;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Roles/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutRole(long id, RequestItem request)
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

            var query = _context.Roles
                            .Where(x => x.Id == id)
                            .FirstOrDefault();
            if (query == null)
            {
                return NotFound("Not Found");
            }

            try
            {
                Role role = query;
                role.Name = request.name;
                role.GuardName = request.guard_name;
                role.UpdatedAt = DateTime.Now;

                _context.Roles.Update(role);
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
            data.guard_name = request.guard_name;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Roles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostRole(RequestItem request)
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

            Role role = new Role();

            try
            {

                role.Name = request.name;
                role.GuardName = request.guard_name;
                role.CreatedAt = DateTime.Now;
                role.UpdatedAt = DateTime.Now;

                _context.Roles.Add(role);
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

            var query = await _context.Roles.Where(x => x.Id == role.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.guard_name = query.GuardName;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;

            return response;
        }

        [HttpPost("list")]
        public async Task<ActionResult<object>> GetList()
        {
            var query = await _context.Roles.OrderBy(x => x.Id).ToListAsync();

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

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteRole(long id)
        {
            var query = await _context.Roles.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Roles.Remove(query);
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

        private bool RoleExists(long id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }

        public class RequestItem
        {
            public string name { get; set; }
            public string guard_name { get; set; }
        }

        private class Validator : AbstractValidator<RequestItem>
        {
            public Validator()
            {
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
                RuleFor(m => m.guard_name).NotEmpty().WithMessage("required");
            }
        }
    }
}
