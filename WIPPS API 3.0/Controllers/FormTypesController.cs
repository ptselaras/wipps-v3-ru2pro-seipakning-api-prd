using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
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

    [Route("api/form-types")]
    [Authorize]
    [ApiController]
    public class FormTypesController : ControllerTrait<FormType>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string plural = "form-types";
        private static string table = "form_types";
        private static string[] fields = { "id", "name", "slug", "refinery_id", "created_at", "updated_at" };
        private static string cond = "slug";

        public FormTypesController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/FormTypes
        [HttpGet]
        public async Task<ActionResult<object>> GetFormTypes(
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

            var query = _context.FormTypes.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
                o.file = item.File;
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

        // GET: api/FormTypes
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetFormType(string slug)
        {
            var query = _context.FormTypes
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
            data.file = query.File;
            data.slug = query.Slug;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.ord = null;
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

            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            long refinery_id = (long)user.RefineryId;

            var query = await _context.FormTypes
                                       .Where(x => x.RefineryId == refinery_id)
                                       .OrderBy(x => x.Ord)
                                       .ToListAsync();

            List<object> data = new List<object>();

            int num = 1;
            foreach(var value in query)
            {

                dynamic o = new ExpandoObject();
                o.num = num;
                o.id = value.Id;
                o.name = value.Name;
                if(value.File != null)
                {
                    o.file = base_url + "/storage/app/public/" + value.File;
                }
                else
                {
                    o.file = "";
                }
                
                o.elipsis = Attributes.getElipsisAttribue(value.Name);
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

        // PUT: api/FormTypes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutFormType(string slug, RequestItem request)
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

            var query = _context.FormTypes
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            var file = "";

            if (request.file != "")
            {
                string fileName = query.File.Substring((plural.Length + 1));

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, plural + "\\", fileName);

                if ((System.IO.File.Exists(filePath)))
                {
                    System.IO.File.Delete(filePath);
                }

                string path = Path.Combine(_hostingEnvironment.WebRootPath, plural);

                file = saveFile(request.file, path, request.name);
            }

            try
            {
                FormType item = query;
                item.Name = request.name;
                if(file != "")
                {
                    item.File = file;
                }
                else
                {
                    item.File = query.File;
                }
                item.Slug = StringExtensions.Slugify(request.name);
                item.UpdatedAt = DateTime.Now;

                _context.FormTypes.Update(item);
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
            data.file = plural + "\\" + file;
            data.slug = StringExtensions.Slugify(request.name);
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.refinery_id = query.RefineryId;
            data.ord = null;
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

        // POST: api/FormTypes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostFormType(RequestItem request)
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

            string path = Path.Combine(_hostingEnvironment.WebRootPath, plural);

            var file = saveFile(request.file, path, request.name);

            FormType formType = new FormType();

            try
            {
                long refinery_id = (long)user.RefineryId;

                formType.Name = request.name;
                formType.Slug = StringExtensions.Slugify(request.name);
                formType.RefineryId = refinery_id;
                formType.CreatedAt = DateTime.Now;
                formType.UpdatedAt = DateTime.Now;
                formType.File = plural + "/" + file;

                _context.FormTypes.Add(formType);
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

            var query = await _context.FormTypes.Where(x => x.Id == formType.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.elipsis = Attributes.getElipsisAttribue(query.Name);
            data.file = plural + "\\" + file;
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

            var query = await _context.FormTypes.Where(x => x.RefineryId == user.RefineryId).OrderBy(x => x.Name).ToListAsync();

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

        // DELETE: api/FormTypes/5
        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteFormType(string slug)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var query = _context.FormTypes
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            string fileName = query.File.Substring((plural.Length + 1));

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, plural + "\\", fileName);

            if ((System.IO.File.Exists(filePath)))
            {
                System.IO.File.Delete(filePath);
            }

            try
            {
                _context.FormTypes.Remove(query);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.title = "Error";
                error.content = "Error on delete your data";
                error.data = "Error on delete your data";
                error.success = false;

                return StatusCode(500, error);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = "Success";
            response.success = true;

            return response;
        }

        private bool FormTypeExists(long id)
        {
            return _context.FormTypes.Any(e => e.Id == id);
        }

        private string saveFile(string image64, string path, string fileName)
        {
            var encoded = image64.Split(",");

            var bytes = Convert.FromBase64String(encoded[1]);

            var mimeType = encoded[0].Split(";")[0].Split(":")[1];

            var extension = Converter.mimeToText(mimeType);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string file = Path.Combine(path, fileName + "." + extension);

            if (bytes.Length > 0)
            {
                using (var stream = new FileStream(file, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }

            return fileName + "." + extension;
        }

        public class RequestItem
        {
            public string name { get; set; }
            public string file { get; set; }
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
