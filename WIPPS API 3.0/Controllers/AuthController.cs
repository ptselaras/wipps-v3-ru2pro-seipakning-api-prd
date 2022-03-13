using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using static WIPPS_API_3._0.Controllers.Response.Auth;

namespace WIPPS_API_3._0.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SafetymanContext _context;
        private readonly JWTSettings _jwtsettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public AuthController(SafetymanContext context, IOptions<JWTSettings> jwtsettings, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _jwtsettings = jwtsettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("info")]
        public async Task<ActionResult<string>> info()
        {
            return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        }

        [HttpPost("check-email")]
        public async Task<ActionResult<object>> CheckEmail(RequestItem request)
        {
            var user = _context.Users.Where(x => x.Email == request.email).FirstOrDefault();

            if (request.current_email != "" && user != null)
            {
                if (user.Email == request.current_email)
                {
                    user = null;
                }
            }

            dynamic response = new ExpandoObject();
            response.success = user != null ? false : true;
            response.data = user != null ? "email_not_available" : "email_available";

            return response;
        }

        [HttpPost("check-username")]
        public async Task<ActionResult<object>> CheckUsername(RequestItem request)
        {
            var user = _context.Users.Where(x => x.Username == request.username).FirstOrDefault();

            if(request.current_username != "" && user != null)
            {
                if(user.Username == request.current_username)
                {
                    user = null;
                }
            }

            dynamic response = new ExpandoObject();
            response.success = user != null ? false : true;
            response.data = user != null ? "username_not_available" : "username_available";

            return response;
        }

        [HttpPost("source")]
        public async Task<ActionResult<object>> Source(RequestItem request)
        {
            string fileName = "";
            string plural = "";
            if(request.file == null)
            {
                fileName = "";
                plural = "";
            }
            else
            {
                string[] explodeFile = request.file.Split("/");
                fileName = explodeFile[explodeFile.Length - 1];
                plural = request.file.Replace("/", "\\");
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, plural);

            if ((System.IO.File.Exists(filePath)))
            {
                string file = Attributes.ImageToBase64(filePath);
                string mime = Converter.base64ToMime(file);

                dynamic response = new ExpandoObject();
                response.name = fileName;
                response.file = string.Format("data:{0};base64,{1}", mime, file);

                return response;
            }
            else
            {
                return NotFound("Not Found");
            }
        }

        

        [HttpPost("users/details/{id}")]
        public async Task<ActionResult<object>> DetailUser(long id)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var query = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

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

            if (model_has_roles != null)
            {

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

                data.role = dataRoles;
            }
            else
            {
                data.role = null;
            }

            dynamic response = new ExpandoObject();
            response.data = data;

            return response;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ResponseUser>> Login([FromBodyOrDefault] RequestUser request)
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            Request.Headers.TryGetValue("Content-Type", out var content);

            var username = "";
            var password = "";

            if (content[0] == "application/json")
            {

                username = request.username;
                password = request.password;
            }
            else
            {
                username = request.username;
                password = request.password;
            }
            //string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

            var user = await _context.Users
                                .Where(x => x.Username == username)
                                .FirstOrDefaultAsync();

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new ResponseUser()
                {
                    user = null,
                    token = null,
                    success = false
                };
            }

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

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new ResponseUser()
            {
                user = new DataUser
                {
                    id = user.Id,
                    name = user.Name,
                    slug = user.Slug,
                    username = user.Username,
                    email = user.Email,
                    photo = user.Photo,
                    refinery_id = user.RefineryId,
                    email_verified_at = user.EmailVerifiedAt,
                    created_at = user.CreatedAt,
                    updated_at = user.UpdatedAt,
                    role = role
                },
                token = tokenHandler.WriteToken(token),
                success = true
            };
        }

        
    }

    public class RequestUser
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class DataUser
    {
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public object photo { get; set; }
        public long? refinery_id { get; set; }
        public object email_verified_at { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DataRole role { get; set; }
    }

    public class DataRole
    {
        public long id { get; set; }
        public string name { get; set; }
        public string guard_name { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string description { get; set; }
        public string display_name { get; set; }
        public Pivot pivot { get; set; }
    }

    public class Pivot
    {
        public long model_id { get; set; }
        public long role_id { get; set; }
        public string model_type { get; set; }
    }

    public class Data
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Type { get; set; }
        public long? Safe { get; set; }
        public long? Unsafe { get; set; }
        public long? Sum { get; set; }
        public string Percent { get; set; }
        public string Sub { get; set; }
        public string Footnote { get; set; }
    }

    public class DataFinal
    {
        public string text { get; set; }
        public long? pos { get; set; }
        public long? neg { get; set; }
        public long? total { get; set; }
        public string percent { get; set; }
    }

    public class DataForm
    {
        public long Id { get; set; }
        public long AreaId { get; set; }
        public long JobId { get; set; }
        public long CompanyId { get; set; }
        public long FormTypeId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public long CreatedUserId { get; set; }
        public long UpdatedUserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Equipment { get; set; }
        public string MonthYear { get; set; }
        public string JobName { get; set; }
        public string Observer { get; set; }
        public string Safe { get; set; }
        public string UnSafe { get; set; }
        public string Total { get; set; }
        public string Percent { get; set; }
        public object Percentage { get; set; }
        public Area Area { get; set; }
        public Company Company { get; set; }
        public FormType FormType { get; set; }
    }

    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }


    public class RequestItem
    {
        public string current_username { get; set; }
        public string username { get; set; }
        public string current_email { get; set; }
        public string email { get; set; }
        public string file { get; set; }
    }

    public class RequestExport
    {
        public string from { get; set; }
        public string to { get; set; }
    }
}
