using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using WIPPS_API_3._0.Helpers;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Response.General;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Route("api")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration configuration;

        private static string[] form = { "id", "name", "slug", "created_at" };
        private static string table = "users";
        private static string plural = "users";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "slug";

        public UsersController(SafetymanContext context, IWebHostEnvironment hostingEnvironment, IConfiguration iConfig) 
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            configuration = iConfig;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<object>> GetUsers(
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

            string[] selected = { "id", "name", "slug", "username", "email", "password", "created_at" };

            string sQuery = Attributes.getQueryTable(selected, table, search, user.RefineryId, order);

            var query = _context.Users.FromSqlRaw(sQuery).ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.name = item.Name;
                o.username = item.Username;
                o.slug = item.Slug;
                o.email = item.Email;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                var model_has_roles = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == item.Id)
                                                .FirstOrDefaultAsync();

                if(model_has_roles != null)
                {
                    switch (model_has_roles.Role.Name)
                    {
                        case "administrator":
                            o.role = "Administrator";
                            break;
                        case "safety-man":
                            o.role = "Safety Man";
                            break;
                        case "safety-inspector":
                            o.role = "Safety Inspector";
                            break;
                        case "supervisor":
                            o.role = "Safety Supervisor";
                            break;
                        case "super-admin":
                            o.role = "Super Admin";
                            break;
                    }
                }

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas.Skip(start).Take(length).ToList();

            return response;
        }

        // GET: api/Users/5
        [HttpGet("{slug}")]
        public async Task<ActionResult<object>> GetUser(string slug)
        {

            var query = _context.Users
                            .Where(x => x.Slug.ToLower() == slug.ToLower())
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            var model_has_roles = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == query.Id)
                                                .FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = query.Slug;
            data.username = query.Username;
            data.email = query.Email;
            data.photo = query.Photo;
            data.refinery_id = query.RefineryId;
            data.email_verified_at = query.EmailVerifiedAt;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            if(model_has_roles != null)
            {
                switch (model_has_roles.Role.Name)
                {
                    case "administrator":
                        data.role = "Administrator";
                        break;
                    case "safety-man":
                        data.role = "Safety Man";
                        break;
                    case "safety-inspector":
                        data.role = "Safety Inspector";
                        break;
                    case "supervisor":
                        data.role = "Safety Supervisor";
                        break;
                    case "super-admin":
                        data.role = "Super Admin";
                        break;
                }

                DataRole roles = new DataRole()
                {
                    id = model_has_roles.Role.Id,
                    name = model_has_roles.Role.Name,
                    guard_name = model_has_roles.Role.GuardName,
                    created_at = model_has_roles.Role.CreatedAt,
                    updated_at = model_has_roles.Role.UpdatedAt,
                    pivot = new Pivot
                    {
                        model_id = model_has_roles.ModelId,
                        role_id = model_has_roles.RoleId,
                        model_type = model_has_roles.ModelType
                    }
                };

                dynamic dataRoles = new ExpandoObject();
                dataRoles.id = roles.id;
                dataRoles.name = roles.name;
                dataRoles.guard_name = roles.guard_name;
                dataRoles.created_at = roles.created_at;
                dataRoles.updated_at = roles.updated_at;
                dataRoles.pivot = roles.pivot;

                List<object> listRoles = new List<object>();
                listRoles.Add(dataRoles);

                data.roles = listRoles;
            }
            else
            {
                data.roles = new List<int>();
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{slug}")]
        public async Task<ActionResult<object>> PutUser(string slug, RequestUpdateProfile request)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var validator = new Validator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                var error = new ExpandoObject() as IDictionary<string, Object>;
                foreach (var err in result.Errors)
                {
                    if (err.PropertyName.Equals("password"))
                    {
                        var errPass = new List<string>();
                        if (err.ErrorCode.Equals("MinimumLengthValidator"))
                        {
                            errPass.Add(err.ErrorMessage);
                        }
                        if (err.ErrorCode.Equals("RegularExpressionValidator"))
                        {
                            errPass.Add(err.ErrorMessage);

                        }

                        var checkExists = error.Where(x => x.Key.ToLower() == "password").ToList();
                        if (checkExists.Count != 1)
                        {
                            error.Add("password", errPass);
                        }
                    }
                    else
                    {
                        error.Add(err.PropertyName, new List<string> { err.ErrorMessage });
                    }

                }
                return UnprocessableEntity(new ResponseError
                {
                    errors = error,
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

            User users = _context.Users.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();

            try
            {
                long refinery_id = long.Parse(configuration.GetSection("Configuration").GetSection("RefineryId").Value);

                users.Name = request.name;
                users.Username = request.username;
                users.Password = BCrypt.Net.BCrypt.HashPassword(request.password);
                users.RefineryId = refinery_id;
                users.UpdatedAt = DateTime.Now;

                _context.Users.Update(users);
                await _context.SaveChangesAsync();

                var findModelHasRole = _context.ModelHasRoles.Where(x => x.ModelId == users.Id).FirstOrDefault();
                
                if(findModelHasRole != null)
                {
                    _context.ModelHasRoles.Remove(findModelHasRole);

                    await _context.SaveChangesAsync();
                }

                ModelHasRole modelHasRole = new ModelHasRole();
                modelHasRole.RoleId = request.role_id ?? 0;
                modelHasRole.ModelType = "App\\User";
                modelHasRole.ModelId = users.Id;

                _context.ModelHasRoles.Add(modelHasRole);

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

            var model_has_roles = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == users.Id)
                                                .FirstOrDefaultAsync();

            DataRole role = new DataRole()
            {
                id = model_has_roles.Role.Id,
                name = model_has_roles.Role.Name,
                guard_name = model_has_roles.Role.GuardName,
                created_at = model_has_roles.Role.CreatedAt,
                updated_at = model_has_roles.Role.UpdatedAt,
                pivot = new Pivot
                {
                    model_id = model_has_roles.ModelId,
                    role_id = model_has_roles.RoleId,
                    model_type = model_has_roles.ModelType
                }
            };

            dynamic roles = new ExpandoObject();
            roles.id = role.id;
            roles.name = role.name;
            roles.guard_name = role.guard_name;
            roles.created_at = role.created_at;
            roles.update_at = role.updated_at;
            roles.pivot = role.pivot;

            dynamic data = new ExpandoObject();
            data.id = users.Id;
            data.name = users.Name;
            data.slug = users.Slug;
            data.username = users.Username;
            data.email = users.Email;
            if (users.Photo != null)
            {
                data.photo = base_url + "/storage/app/public/" + users.Photo;
            }
            else
            {
                data.photo = users.Photo;
            }

            data.refinery_id = users.RefineryId;
            data.email_verified_at = users.EmailVerifiedAt;
            data.created_at = users.CreatedAt;
            data.updated_at = users.UpdatedAt;
            data.roles = roles;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostUser(RequestUpdateProfile request)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var validator = new Validator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                var error = new ExpandoObject() as IDictionary<string, Object>;
                foreach (var err in result.Errors)
                {
                    if (err.PropertyName.Equals("password"))
                    {
                        var errPass = new List<string>();
                        if (err.ErrorCode.Equals("MinimumLengthValidator"))
                        {
                            errPass.Add(err.ErrorMessage);
                        }
                        if (err.ErrorCode.Equals("RegularExpressionValidator"))
                        {
                            errPass.Add(err.ErrorMessage);

                        }

                        var checkExists = error.Where(x => x.Key.ToLower() == "password").ToList();
                        if(checkExists.Count != 1)
                        {
                            error.Add("password", errPass);
                        }
                    }
                    else
                    {
                        error.Add(err.PropertyName, new List<string> { err.ErrorMessage });
                    }
 
                }
                return UnprocessableEntity(new ResponseError
                {
                    errors = error,
                    message = "The given data was invalid"
                });
            }

            bool cekUsername = cekUsernameExists(request.username);
            bool cekEmail = cekEmailExists(request.email);

            var errors = new ExpandoObject() as IDictionary<string, Object>;

            if (cekUsername && cekEmail)
            {

                errors.Add("username", new List<string>() { "The username has already been taken." });
                errors.Add("email", new List<string>() { "The email has already been taken." });

                return UnprocessableEntity(new ResponseError
                {
                    errors = errors,
                    message = "The given data was invalid"
                });
            }
            else if (cekUsername)
            {
                errors.Add("username", new List<string>() { "The username has already been taken." });
                return UnprocessableEntity(new ResponseError
                {
                    errors = errors,
                    message = "The given data was invalid"
                });
            }
            else if (cekEmail)
            {
                errors.Add("email", new List<string>() { "The email has already been taken." });
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

            User users = new User();

            try
            {
                long refinery_id = long.Parse(configuration.GetSection("Configuration").GetSection("RefineryId").Value);

                users.Name = request.name;
                users.Slug = StringExtensions.Slugify(request.name);
                users.Email = request.email;
                users.Username = request.username;
                users.Password = BCrypt.Net.BCrypt.HashPassword(request.password);
                users.RefineryId = refinery_id;
                users.CreatedAt = DateTime.Now;
                users.UpdatedAt = DateTime.Now;

                _context.Users.Add(users);
                await _context.SaveChangesAsync();

                ModelHasRole modelHasRole = new ModelHasRole();
                modelHasRole.RoleId = request.role_id ?? 0;
                modelHasRole.ModelType = "App\\User";
                modelHasRole.ModelId = users.Id;

                _context.ModelHasRoles.Add(modelHasRole);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";
                var test = e.Message;
                return StatusCode(500, error);
            }

            var model_has_roles = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == users.Id)
                                                .FirstOrDefaultAsync();

            DataRole role = new DataRole()
            {
                id = model_has_roles.Role.Id,
                name = model_has_roles.Role.Name,
                guard_name = model_has_roles.Role.GuardName,
                created_at = model_has_roles.Role.CreatedAt,
                updated_at = model_has_roles.Role.UpdatedAt,
                pivot = new Pivot
                {
                    model_id = model_has_roles.ModelId,
                    role_id = model_has_roles.RoleId,
                    model_type = model_has_roles.ModelType
                }
            };

            dynamic roles = new ExpandoObject();
            roles.id = role.id;
            roles.name = role.name;
            roles.guard_name = role.guard_name;
            roles.created_at = role.created_at;
            roles.update_at = role.updated_at;
            roles.pivot = role.pivot;

            dynamic data = new ExpandoObject();
            data.id = users.Id;
            data.name = users.Name;
            data.slug = users.Slug;
            data.username = users.Username;
            data.email = users.Email;
            if (users.Photo != null)
            {
                data.photo = base_url + "/storage/app/public/" + users.Photo;
            }
            else
            {
                data.photo = users.Photo;
            }

            data.refinery_id = users.RefineryId;
            data.email_verified_at = users.EmailVerifiedAt;
            data.created_at = users.CreatedAt;
            data.updated_at = users.UpdatedAt;
            data.roles = roles;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpPost]
        [Route("check-name")]
        public virtual async Task<ActionResult<dynamic>> GetCheckName([FromBody] dynamic request)
        {

            var req = request[cond];
            var field = cond;
            string sql = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", string.Join(", ", fields), table, field, req.Value);

            object data = null;

            var conn = _context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    string query = sql;
                    command.CommandText = query;
                    DbDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var o = new ExpandoObject() as IDictionary<string, Object>;
                                for (int i = 0; i < fields.Count(); i++)
                                {
                                    o.Add(fields[i], reader[i]);
                                }

                                data = o;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Dont have rows");
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            dynamic response = new ExpandoObject();
            response.success = data != null ? false : true;
            response.data = data != null ? "name_not_available" : "name_available";

            return response;
        }

        [HttpPost("upload/file")]
        public async Task<ActionResult<object>> UploadFile(
            [FromBodyOrDefault] RequestUpload request,
            [FromForm] string name,
            [FromForm] string file
        )
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            var validator = new ValidatorUploadFile();
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

            string path = Path.Combine(_hostingEnvironment.WebRootPath, plural);

            string names = "";
            string files = "";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {

                names = request.name;
                files = request.file;

            }
            else
            {
                names = name.Replace(":","");
                files = file;
            }

            var fileName = saveFile(files, path, names, user.Id);

            dynamic data = new ExpandoObject();
            data.id = user.Id;
            data.name = user.Name;
            data.slug = user.Slug;
            data.username = user.Username;
            data.email = user.Email;
            data.photo = base_url + "/storage/app/public/" + plural + "/" + fileName;
            data.refinery_id = user.RefineryId;
            data.email_verified_at = user.EmailVerifiedAt;
            data.created_at = user.CreatedAt;
            data.updated_at = user.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpPost()]
        [Route("profile/edit")]
        public async Task<ActionResult<object>> UpdateProfile(
            [FromBodyOrDefault] RequestUpdateProfile request,
            [FromForm] string email,
            [FromForm] string name,
            [FromForm] string username
        )
        {

            string emails = "";
            string names = "";
            string usernames = "";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                emails = request.email;
                names = request.name;
                usernames = request.username;

                var validator = new ValidatorUpdateProfile();
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
            }
            else
            {
                emails = email;
                names = name;
                usernames = username;
            }

            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            string u_name = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                u_name = identity.FindFirst(ClaimTypes.Name).Value;
            }

            User user = await _context.Users.Where(x => x.Username.ToLower() == u_name.ToLower()).FirstOrDefaultAsync();

            user.Username = usernames;
            user.Email = emails;
            user.Name = names;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            var model_has_roles = await _context.ModelHasRoles
                                                .Include(r => r.Role)
                                                .Where(x => x.ModelId == user.Id)
                                                .FirstOrDefaultAsync();

            DataRole role = new DataRole()
            {
                id = model_has_roles.Role.Id,
                name = model_has_roles.Role.Name,
                guard_name = model_has_roles.Role.GuardName,
                created_at = model_has_roles.Role.CreatedAt,
                updated_at = model_has_roles.Role.UpdatedAt,
                pivot = new Pivot
                {
                    model_id = model_has_roles.ModelId,
                    role_id = model_has_roles.RoleId,
                    model_type = model_has_roles.ModelType
                }
            };

            switch (model_has_roles.Role.Name)
            {
                case "administrator":
                    role.description = "Administrator";
                    role.display_name = "administrator";
                    break;
                case "safety-man":
                    role.description = "Safety Man";
                    role.display_name = "Safety Man";
                    break;
                case "safety-inspector":
                    role.description = "Safety Inspector";
                    role.display_name = "Safety Inspector";
                    break;
                case "supervisor":
                    role.description = "Safety Supervisor";
                    role.display_name = "Safety Supervisor";
                    break;
                case "super-admin":
                    role.description = "Super Admin";
                    role.display_name = "Super Admin";
                    break;
            }

            dynamic data = new ExpandoObject();
            data.id = user.Id;
            data.name = user.Name;
            data.slug = user.Slug;
            data.username = user.Username;
            data.email = user.Email;
            if(user.Photo != null)
            {
                data.photo = base_url + "/storage/app/public/" +  user.Photo;
            }
            else
            {
                data.photo = user.Photo;
            }
            
            data.refinery_id = user.RefineryId;
            data.email_verified_at = user.EmailVerifiedAt;
            data.created_at = user.CreatedAt;
            data.updated_at = user.UpdatedAt;
            data.role = role;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpPost()]
        [Route("profile/edit/password")]
        public async Task<ActionResult<object>> UpdatePassword(
            [FromBodyOrDefault] RequestUpdatePassword request,
            [FromForm] string new_password,
            [FromForm] string old_password
        )
        {
            string newPassword = "";
            string oldPassword = "";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content[0] == "application/json")
            {
                newPassword = request.new_password;
                oldPassword = request.old_password;

                var validator = new ValidatorUpdatePassword();
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
            }
            else
            {
                newPassword = new_password;
                oldPassword = old_password;
            }


            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                var errors = new ExpandoObject() as IDictionary<string, Object>;
                errors.Add("old_password", new List<string> { "not_match" });
                return UnprocessableEntity(new ResponseError
                {
                    errors = errors,
                    message = "The given data was invalid"
                });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            dynamic data = new ExpandoObject();
            data.id = user.Id;
            data.name = user.Name;
            data.slug = user.Slug;
            data.username = user.Username;
            data.email = user.Email;
            data.photo = user.Photo;
            data.refinery_id = user.RefineryId;
            data.email_verified_at = user.EmailVerifiedAt;
            data.created_at = user.CreatedAt;
            data.updated_at = user.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        [HttpPost("list")]
        public async Task<ActionResult<object>> GetList()
        {
            var query = await _context.Users.OrderBy(x => x.Id).ToListAsync();

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

        [HttpGet("create")]
        public async Task<ActionResult<object>> Create()
        {
            return NotFound("Not Found");
        }

        [HttpGet("{slug}/edit")]
        public async Task<ActionResult<object>> Edit(string slug)
        {
            return NotFound("Not Found");
        }

        [HttpDelete("{slug}")]
        public async Task<ActionResult<object>> DeleteUser(string slug)
        {
            var query = _context.Users.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Remove(query);
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

        private bool cekEmailExists(string email)
        {
            return _context.Users.Any(x => x.Email.ToLower() == email.ToLower());
        }

        private bool cekUsernameExists(string username)
        {
            return _context.Users.Any(x => x.Username.ToLower() == username.ToLower());
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string saveFile(string image64, string path, string fileName, long id)
        {
            var mimeType = Converter.base64ToMime(image64);

            var bytes = Convert.FromBase64String(image64);

            var extension = Converter.mimeToText(mimeType);

            string path_user_id = path;

            if (!Directory.Exists(path_user_id))
            {
                Directory.CreateDirectory(path_user_id);
            }

            string file = Path.Combine(path_user_id, fileName + "." + extension);

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

        public class RequestUpload
        {
            public string name { get; set; }
            public string file { get; set; }
        }
        public class RequestUpdateProfile
        {
            public string username { get; set; }
            public string email { get; set; }
            public string name { get; set; }
            public string password { get; set; }
            public long? role_id { get; set; }
        }

        public class RequestUpdatePassword
        {
            public string old_password { get; set; }
            public string new_password { get; set; }
        }

        private class ValidatorUploadFile : AbstractValidator<RequestUpload>
        {
            public ValidatorUploadFile()
            {
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
                RuleFor(m => m.file).NotEmpty().WithMessage("required");
            }
        }

        private class ValidatorUpdateProfile : AbstractValidator<RequestUpdateProfile>
        {
            public ValidatorUpdateProfile()
            {
                RuleFor(m => m.username).NotEmpty().WithMessage("required").MinimumLength(4);
                RuleFor(m => m.email).NotEmpty().WithMessage("required").MinimumLength(4);
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
                //RuleFor(m => m.role_id).NotNull().WithMessage("required");
            }
        }
        private class ValidatorUpdatePassword : AbstractValidator<RequestUpdatePassword>
        {
            public ValidatorUpdatePassword()
            {
                RuleFor(m => m.old_password).NotEmpty().WithMessage("required");
                RuleFor(m => m.new_password).NotEmpty().WithMessage("required");
            }
        }

        private class Validator : AbstractValidator<RequestUpdateProfile>
        {
            public Validator()
            {
                RuleFor(m => m.username)
                    .NotEmpty()
                    .WithMessage("required");
                RuleFor(m => m.email)
                    .NotEmpty()
                    .WithMessage("required");
                RuleFor(m => m.name).NotEmpty().WithMessage("required");
                RuleFor(m => m.password)
                    .NotEmpty()
                    .WithMessage("required")
                    .MinimumLength(8)
                    .WithMessage("The password must be at least 8 characters")
                    .Matches("^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$")
                    .WithMessage("The password must consist of numbers, uppercase and lowercase letters");
            }
        }
    }
}
