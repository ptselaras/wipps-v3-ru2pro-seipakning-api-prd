using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace WIPPS_API_3._0.Helpers
{
    public abstract class ControllerTrait<T> : ControllerBase
    {
        SafetymanContext context;
        private readonly IWebHostEnvironment hostingEnvironment;

        private string cond = "name";
        private string table;
        private string plural;
        private string[] form;
        private string[] fields;

        

        public ControllerTrait(SafetymanContext _context, IWebHostEnvironment _hostingEnvironment, string plural, string cond, string table, string[] form, string[] fields)
        {
            this.plural = plural;
            this.cond = cond;
            this.table = table;
            this.context = _context;
            this.hostingEnvironment = _hostingEnvironment;
            this.form = form;
            this.fields = fields;
        }

        [HttpPost]
        [Route("check-name")]
        public virtual async Task<ActionResult<dynamic>> GetCheckName([FromBody] dynamic request)
        {

            var req = request[this.cond];
            var field = this.cond;
            string sql = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'", string.Join(", ", fields), this.table, field, req.Value);

            object data = null;

            var conn = context.Database.GetDbConnection();
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

        [HttpPost]
        [Route("upload/file")]
        public virtual async Task<ActionResult<object>> UploadFile2([FromBodyOrDefault] RequestUpload request)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            string path = Path.Combine(hostingEnvironment.WebRootPath, this.plural);

            Request.Headers.TryGetValue("Content-Type", out var content);

            if(content[0] == "application/json")
            {

                var file = saveFile(request.file.ToString(), path, request.name, user.Id);

                if(file == "error")
                {
                    dynamic error = new ExpandoObject();
                    error.data = "Image can only allow jpeg";
                    error.success = false;
                    error.code = 500;

                    return StatusCode(500, error);
                }

                dynamic response = new ExpandoObject();
                response.data = this.plural + "/" + user.Id + "/" + file;
                response.success = true;
                response.code = 200;


                return response;
            }
            else
            {
                var files = Request.Form.Files;
                if (files.Count() == 0)
                {
                    var file = saveFile(request.file.ToString(), path, request.name, user.Id);

                    dynamic response = new ExpandoObject();
                    response.data = this.plural + "/" + user.Id + "/" + file;
                    response.success = true;
                    response.code = 200;

                    return response;
                }
                else
                {
                    //var file = saveFile(request.file.ToString(), path, request.name, user.Id);
                    var file = files[0];

                    string path_user_id = path + "/" + user.Id.ToString();

                    if (!Directory.Exists(path_user_id))
                    {
                        Directory.CreateDirectory(path_user_id);
                    }

                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    string fullPath = Path.Combine(path_user_id, fileName);

                    if (file.Length > 0)
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                    }

                    dynamic response = new ExpandoObject();
                    response.data = this.plural + "/" + user.Id + "/" + fileName;
                    response.success = true;
                    response.code = 200;

                    return response;
                }
                
            }
        }

        //[HttpPost]
        //[Route("upload/file")]
        //public virtual async Task<ActionResult<object>> UploadFile([FromBody] RequestUpload request)
        //{
        //    string username = string.Empty;
        //    if (HttpContext.User.Identity is ClaimsIdentity identity)
        //    {
        //        username = identity.FindFirst(ClaimTypes.Name).Value;
        //    }

        //    var user = await context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

        //    string path = Path.Combine(hostingEnvironment.WebRootPath, this.plural);

        //    var file = saveFile(request.file.ToString(), path, request.name, user.Id);

        //    dynamic response = new ExpandoObject();
        //    response.data = this.plural + "\\" + user.Id + "\\" + file;
        //    response.success = true;
        //    response.code = 200;
            

        //    return response;
        //}

        [HttpGet("create")]
        public virtual async Task<ActionResult<object>> Create()
        {
            return NotFound("Not Found");
        }

        [HttpGet("{slug}/edit")]
        public virtual async Task<ActionResult<object>> Edit(string slug)
        {
            return NotFound("Not Found");
        }

        private string saveFile(string image64, string path, string fileName, long id)
        {
            var mimeType = Converter.base64ToMime(image64);

            if (mimeType != "image/jpeg"){
                return "error";
            }

            var bytes = Convert.FromBase64String(image64);

            var extension = Converter.mimeToText(mimeType);

            string path_user_id = path + "/" + id.ToString();

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

        
    }
}
