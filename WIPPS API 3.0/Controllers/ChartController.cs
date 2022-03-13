using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WIPPS_API_3._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using WIPPS_API_3._0.Models.Chart;
using System.Text;
using WIPPS_API_3._0.Utils;
using Toycloud.AspNetCore.Mvc.ModelBinding;

namespace WIPPS_API_3._0.Controllers
{
    [Route("api/chart")]
    [Route("api")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly SafetymanContext _context;
        public ChartController(SafetymanContext context)
        {
            _context = context;
        }

        [HttpGet("chart/areas")]
        public async Task<ActionResult<object>> chartAreas(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getAreaSafe(from, to, user.RefineryId).Take(10).ToList();

            return data;
        }

        [HttpGet("chart/areaus")]
        public async Task<ActionResult<object>> chartAreaus(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getAreaUnsafe(from, to, user.RefineryId).Take(10).ToList();

            return data;
        }

        [HttpGet("chart/company")]
        public async Task<ActionResult<object>> chartCompany(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getCompanies(from, to, user.RefineryId).Take(10).ToList();

            return data;
        }

        [HttpGet("chart/company_inspection")]
        public async Task<ActionResult<object>> chartCompanyInspection(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getCompanyInspection(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/inspection")]
        public async Task<ActionResult<object>> chartInspection(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getInspections(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/inspector")]
        public async Task<ActionResult<object>> chartInspector(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getInspectorInspection(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/inspector_score")]
        public async Task<ActionResult<object>> chartInspectorScore(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getInspectorScore(from, to, user.RefineryId).OrderByDescending(x => x.total).Take(10).ToList();

            return data;
        }

        [HttpGet("chart/location")]
        public async Task<ActionResult<object>> chartLocation(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getLocations(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/svsu")]
        public async Task<ActionResult<object>> chartSvsu(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getSafeVsUnsafe(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/types")]
        public async Task<ActionResult<object>> chartTypes(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getTypeSafe(from, to, user.RefineryId);

            return data;
        }

        [HttpGet("chart/typeus")]
        public async Task<ActionResult<object>> getTypeUnsafe(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var data = getTypeUnsafe(from, to, user.RefineryId);

            return data;
        }

        [HttpGet]
        [Route("charts")]
        public async Task<ActionResult<object>> charts(
           [FromQuery(Name = "from")] string from,
           [FromQuery(Name = "to")] string to
       )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var company = getCompanyInspection(from, to, user.RefineryId);
            var topCompany = getCompanies(from, to, user.RefineryId).OrderByDescending(x => x.total).ToList();
            var types = getTypeSafe(from, to, user.RefineryId);
            var typeus = getTypeUnsafe(from, to, user.RefineryId);
            var svsu = getSafeVsUnsafe(from, to, user.RefineryId);
            var areas = getAreaSafe(from, to, user.RefineryId);
            var areaus = getAreaUnsafe(from, to, user.RefineryId);
            var dataInspection = getInspections(from, to, user.RefineryId);
            var locations = getLocations(from, to, user.RefineryId);
            var inspector = getInspectorInspection(from, to, user.RefineryId);
            var inspectorScore = getInspectorScore(from, to, user.RefineryId).OrderByDescending(x => x.total).ToList();

            dynamic response = new ExpandoObject();
            response.inspection = dataInspection;
            response.svsu = svsu;
            response.areas = areas;
            response.areaus = areaus.Take(10).ToList();
            response.types = types;
            response.typeus = typeus.Take(10).ToList();
            response.company = topCompany.Take(10).ToList();
            response.company_inspection = company.Take(10).ToList();
            response.inspector = inspector;
            response.inspector_score = inspectorScore.Take(10).ToList();
            response.location = locations.Take(10).ToList();

            return response;
        }

        [HttpGet("charts/checklists")]
        public async Task<ActionResult<object>> getDetailType(
            [FromQuery(Name = "type")] string type
        )
        {
            //string from = DateTime.Now.ToString("yyy") + "-01-01 00:00:01";
            //string to = DateTime.Now.ToString("yyy") + "-12-31 23:59:59";

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var sb = new StringBuilder();
            sb.Append(@"
                select form_types.name as type,
                form_types.slug,
                component_checklists.status,
                component_checklists.form_component_id,
                components.name,count(component_checklists.status) as total 
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join form_types on form_types.id = forms.form_type_id 
                inner join form_components on form_components.id = component_checklists.form_component_id 
                inner join components on components.id = form_components.component_id
            ");

            sb.AppendFormat(@"where form_types.refinery_id = {0}", user.RefineryId);
            sb.AppendFormat(@"and component_checklists.status != {0}", 3);
            //sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);

            var form = _context.FormTypes.Where(x => x.Slug.ToLower() == type.ToLower() && x.RefineryId == user.RefineryId).FirstOrDefault();

            sb.AppendFormat(@"and form_types.id = {0} ", form.Id);
            sb.Append(@"
                group by form_types.name, form_types.slug, component_checklists.status, components.name, component_checklists.form_component_id 
                order by total desc
            ");

            string sqlRaw = string.Format(sb.ToString());

            var areaus = (_context.ChartGetDetailType
                .FromSqlRaw(sqlRaw)
                .ToList()).OrderByDescending(x => x.total).ToList();

            List<Data> data = new List<Data>();
            foreach (var value in areaus)
            {
                Data newstd = new Data();
                newstd.Name = value.name;
                newstd.Slug = value.slug;
                newstd.Type = value.type;

                var percent = areaus.Where(x => x.form_component_id == value.form_component_id).ToList();
                double? neg = 0;
                double? pos = 0;

                foreach (var val in percent)
                {
                    if (val.status == 1)
                    {
                        pos = val.total;
                    }
                    else
                    {
                        neg = val.total;
                    }
                }

                newstd.Safe = (long?)pos;
                newstd.Unsafe = (long?)neg;

                double? sum = (pos ?? 0) + (neg ?? 0);
                newstd.Sum = (long?)sum;

                double percent1 = (((sum ?? 0)) == 0) ? 0 : (((pos ?? 0) / (sum ?? 0)) * 100);
                decimal percent2 = (decimal)(Math.Floor(percent1 * 100) / 100);
                newstd.Percent = Math.Round(percent2, 2).ToString() + "%";

                data.Add(newstd);
            }

            List<Data> newData = new List<Data>();
            foreach (Data item in data)
            {
                if (item.Safe != 0)
                {
                    Data o = new Data();
                    o.Name = item.Name;
                    o.Slug = item.Slug;
                    o.Type = item.Type;
                    o.Safe = item.Safe;
                    o.Unsafe = item.Unsafe;
                    o.Sum = item.Sum;
                    o.Percent = item.Percent;

                    string[] splitter = item.Name.Split("<sub>");
                    if (splitter.Count() == 2)
                    {
                        o.Sub = splitter[0];
                        o.Name = splitter[1];
                    }

                    string[] splitter2 = item.Name.Split("<footnote>");
                    if (splitter2.Count() == 2)
                    {
                        o.Footnote = splitter2[1];
                        o.Name = splitter2[0];
                    }

                    newData.Add(o);
                }
            }

            dynamic response = new ExpandoObject();
            response.data = newData;

            return response;
        }

        [HttpGet("charts/types")]
        public async Task<ActionResult<object>> Types(
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to
        )
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var sb = new StringBuilder();
            sb.Append(@"
                select form_types.*, 
                (
                    select count(*) 
                    from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id 
                    where form_types.id = forms.form_type_id
            ");

            if(from != null && to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                    and status <> 3
                ) as checklists_count, 
                (
                    select count(*) 
                    from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id
                    where form_types.id = forms.form_type_id
            ");

            if (from != null && to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                and status = 1) as checklists_pos_count, 
                (
                    select count(*) 
                    from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id 
                    where form_types.id = forms.form_type_id
            ");

            if (from != null && to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                    and status = 2
                ) as checklists_neg_count 
                from form_types
            ");

            sb.AppendFormat(@"where refinery_id = {0}", user.RefineryId);

            string sqlRaw = string.Format(sb.ToString());

            var type = _context.FormTypes3
                .FromSqlRaw(sqlRaw)
                .ToList();

            List<object> data = new List<object>();
            foreach(var value in type)
            {
                var name = value.name;
                var slug = value.slug;
                var file = base_url + "/storage/app/public/" + value.file;
                decimal finalPercent = value.checklists_count == 0 ? (decimal)0.00 : (Math.Ceiling((decimal)(((((double)value.checklists_pos_count / (double)value.checklists_count) * 100) * 100) / 100)));

                dynamic o = new ExpandoObject();
                o.name = name;
                o.slug = slug;
                o.file = file;
                o.percent = finalPercent;

                data.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.data = data;

            return Ok(response);
        }

        [HttpPost("charts/types")]
        public async Task<ActionResult<object>> GetTypes(
            [FromBodyOrDefault] RequestType request,
            [FromQuery(Name = "type")] string type
        )
        {
            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            string types = "";

            Request.Headers.TryGetValue("Content-Type", out var content);

            if (content.Count > 0)
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

            var typess = _context.FormTypes.Where(x => x.RefineryId == user.RefineryId && x.Slug.ToLower() == types.ToLower()).FirstOrDefault();

            string from = DateTime.Now.ToString("yyy") + "-01-01 00:00:01";
            string to = DateTime.Now.ToString("yyy") + "-12-31 23:59:59";

            //string from = "2020-01-01 00:00:01";
            //string to = "2020-12-31 23:59:59";

            var SqlUsers = string.Format(@"
                select users.* from users 
                INNER JOIN forms ON forms.created_user_id = users.id 
                where exists ( 
                    select * from users 
                    where users.id = forms.created_user_id 
                    and forms.form_type_id = {0} 
                    and forms.created_at between '{1}' and '{2}' 
                )
            ", typess.Id, from, to);

            var users = _context.Users
                .FromSqlRaw(SqlUsers)
                .GroupBy(x => new
                {
                    x.Id, x.Name, x.Slug, x.Photo, x.CreatedAt
                })
                .Select(x => new User
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    Slug = x.Key.Slug,
                    Photo = x.Key.Photo,
                    CreatedAt = x.Key.CreatedAt
                })
                .ToList();

            List<object> data = new List<object>();
            foreach(var value in users)
            {
                dynamic o = new ExpandoObject();
                o.name = value.Name;
                o.created = value.CreatedAt.Value.ToString("ddd. yyy, MMM dd HH:mm:ss");
                o.slug = value.Slug;
                if(value.Photo != null)
                {
                    o.photo = base_url + "/storage/app/public/" + value.Photo;
                }
                else
                {
                    o.photo = null;
                }
                o.id = value.Id;

                data.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.data = data;

            return Ok(response);
        }

        private List<GetAreaSafe> getAreaSafe(string from, string to, long? refinery_id)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                select areas.name,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as pos,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as neg,
                cast((sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END) / (sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END))*100.0) as decimal(10, 2)) as total 
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join areas on forms.area_id = areas.id
            ");
            sb.AppendFormat(@"where areas.refinery_id = {0}", refinery_id);
            sb.Append(@"and component_checklists.status != 3");

            if(from != null && to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                group by areas.name 
                order by total desc
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetAreaSafe
                .FromSqlRaw(Sql)
                .ToList();

            return query;
        }

        private List<GetAreaSafe> getAreaUnsafe(string from, string to, long? refinery_id)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                select areas.name,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as pos,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as neg,
                cast((sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END)/(sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END))*100.0) as decimal(10, 2)) as total 
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join areas on forms.area_id = areas.id
            ");
            sb.AppendFormat(@"where areas.refinery_id = {0}", refinery_id);
            sb.Append(@"and component_checklists.status != 3");

            if(from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                group by areas.name 
                order by total desc
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetAreaSafe
                .FromSqlRaw(Sql)
                .ToList();

            return query;
        }

        private List<GetCompany> getCompanies(string from, string to, long? refinery_id)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                select companies.name as companies,
                companies.name as company,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as safe,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as pos,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as neg,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as unsafe,
                cast((sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END)/(sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END))*100.0) as decimal(10, 2)) as total 
                from companies 
                inner join forms on companies.id = forms.company_id 
                inner join component_checklists on component_checklists.form_id = forms.id
            ");
            sb.AppendFormat(@"where companies.refinery_id = {0}", refinery_id);
            sb.Append(@"and component_checklists.status != 3");

            if(from != null && to != null)
            {
                sb.AppendFormat(@"and forms.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                group by companies.name
                order by total desc
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetCompanies
                .FromSqlRaw(Sql)
                .ToList();

            return query;
        }

        private List<Models.Procedure.GetCompanyInspection> getCompanyInspection(string from, string to, long? refinery_id)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                select companies.*, 
                (
                    select count(*) from forms 
                    where companies.id = forms.company_id 
            ");
            
            if(from != null && to != null)
            {
                sb.AppendFormat(@"and forms.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                ) as forms_count 
                from companies
            ");
            sb.AppendFormat(@"where refinery_id = {0}", refinery_id);
            sb.Append("order by forms_count desc");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetCompanyInspections
                .FromSqlRaw(Sql)
                .ToList()
                .Take(10)
                .ToList();

            List<Models.Procedure.GetCompanyInspection> data = new List<Models.Procedure.GetCompanyInspection>();
            foreach(var value in query)
            {
                Models.Procedure.GetCompanyInspection o = new Models.Procedure.GetCompanyInspection();
                o.id = value.id;
                o.name = value.name;
                o.slug = value.slug;
                o.type = value.type;
                o.refinery_id = value.refinery_id;
                o.created_at = value.created_at;
                o.updated_at = value.updated_at;
                o.forms_count = value.forms_count;
                o.companies = value.name;
                o.total = value.forms_count;

                data.Add(o);
            }

            return data;
        }

        private object getInspections(string from, string to, long? refinery_id)
        {
            List<string> ids = getUserIdByRefinery(refinery_id);

            var sb = new StringBuilder();
            sb.Append(@"
                select sum(CAST(j.inspection as INT)) as inspection,
                sum(j.safe) as safe,sum(j.unsafe) as unsafe, 
                FORMAT(forms.created_at, 'yyy MMMM') AS month,
                forms.created_at from forms 
                inner join (
                    select form_id,'1' as inspection,
                    sum(CASE WHEN status = 1 THEN 1 ELSE 0 END) as safe,
                    sum(CASE WHEN status = 2 THEN 1 ELSE 0 END) as unsafe,
                    cast((sum(CASE WHEN status = 1 THEN 1.0 ELSE 0.0 END)/((sum(CASE WHEN status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN status = 1 THEN 1.0 ELSE 0.0 END))*100.0)) as decimal(10,2)) as percentage 
                    from component_checklists group by form_id
                ) as j on j.form_id = forms.id
            ");
            sb.AppendFormat(@"where forms.created_user_id in ({0})", string.Join(", ", ids));
            if(from != null && to != null)
            {
                sb.AppendFormat(@"and forms.created_at between '{0}' and '{1}'", from, to);
            }
            sb.Append(@"group by FORMAT(forms.created_at, 'yyy MMMM'), created_at");

            string Sql = string.Format(sb.ToString());

            var query = (_context.ChartGetInspections
                .FromSqlRaw(Sql).ToList())
                .GroupBy(x => x.month)
                .Select(g => new { 
                    g.Key, 
                    Data = g.Select(x => new GetInspection
                    {
                        month = x.month,
                        created_at = x.created_at,
                        safe = g.Sum(s => s.safe),
                        @unsafe = g.Sum(s => s.@unsafe),
                        inspection = g.Sum(s => s.inspection)
                    }).FirstOrDefault()
                })
                .ToDictionary(x => x.Key, x => x.Data);

            List<DataInspections> dataInspections = new List<DataInspections>();
            foreach(var value in query.Values)
            {
                DataInspections o = new DataInspections();
                o.month = value.month;
                o.month_n = value.created_at.Value.ToString("yyy MM");
                double safe = value.safe ?? 0;
                double @unsafe = value.@unsafe ?? 0;
                o.safe = (int)safe;
                o.@unsafe = (int)@unsafe;
                o.percent = (safe + @unsafe) != 0 ? (int)Math.Round((safe / (safe + @unsafe)) * 100) : 0;
                o.inspection = value.inspection ?? 0;

                List<DataPie> pie = new List<DataPie>();

                DataPie pieSafe = new DataPie();
                pieSafe.title = "Safe";
                pieSafe.value = value.safe;

                pie.Add(pieSafe);

                DataPie pieUnsafe = new DataPie();
                pieUnsafe.title = "Unsafe";
                pieUnsafe.value = value.@unsafe;

                pie.Add(pieUnsafe);

                o.pie = pie;

                dataInspections.Add(o);
            }

            var data = dataInspections.OrderBy(x => x.month_n).ToList();

            return data;
        }

        private List<GetInspectorInspection> getInspectorInspection(string from, string to, long? refinery_id)
        {
            List<string> ids = getUserIdByRefinery(refinery_id);

            var sb = new StringBuilder();
            sb.Append(@"
                select users.name as username,
                count(forms.id) as total 
                from forms 
                inner join users on users.id = forms.created_user_id
            ");
            sb.AppendFormat(@"where forms.created_user_id in ({0})", string.Join(", ", ids));

            if(from != null & to != null)
            {
                sb.AppendFormat(@"and forms.created_at between '{0}' and '{1}'", from, to);
            }

            sb.Append(@"
                group by users.name 
                order by total desc
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetInspectorInspections
                .FromSqlRaw(Sql)
                .ToList();

            return query;
        }

        private List<GetInspectorScore> getInspectorScore(string from, string to, long? refinery_id)
        {
            List<string> ids = getUserIdByRefinery(refinery_id);

            var sb = new StringBuilder();
            sb.Append(@"
                select users.name,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as pos,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as neg,
                cast((sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END)/(sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END))*100.0) as decimal(10,2)) as total 
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join users on users.id = forms.created_user_id 
            ");
            sb.AppendFormat(@"where forms.created_user_id in ({0})", string.Join(", ", ids));
            sb.Append(@"and component_checklists.status != 3");

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                group by users.name 
                order by total desc
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetInspectorScores
                .FromSqlRaw(Sql)
                .ToList();

            return query;
        }

        private List<object> getLocations(string from, string to, long? refinery_id)
        {

            var sb = new StringBuilder();
            sb.Append(@"
                select areas.*, 
                (
                    select count(*) 
                    from forms where areas.id = forms.area_id 
            ");

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and forms.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                ) as forms_count
                from areas 
                where exists (
                    select * from forms where areas.id = forms.area_id
                )
            ");

            sb.AppendFormat(@"and refinery_id = {0} ", refinery_id);

            sb.Append(@"order by forms_count desc");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetLocations
                .FromSqlRaw(Sql)
                .ToList();

            List<object> data = new List<object>();
            foreach(var value in query)
            {
                dynamic o = new ExpandoObject();
                o.id = value.id;
                o.name = value.name;
                o.slug = value.slug;
                o.refinery_id = value.refinery_id;
                o.created_at = value.created_at;
                o.updated_at = value.updated_at;
                o.forms_count = value.forms_count;
                o.total = value.forms_count;
                o.percentage = "0.00";
                o.percentagen = "0.00";

                data.Add(o);
            }

            return data;
        }

        private List<object> getSafeVsUnsafe(string from, string to, long? refinery_id)
        {

            var sb = new StringBuilder();
            sb.Append(@"
                select component_checklists.status,
                CAST (count(component_checklists.status) as decimal(10, 2)) as total  
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join form_types on form_types.id = forms.form_type_id 
                where component_checklists.status <> 3  
            ");

            sb.AppendFormat(@"and form_types.refinery_id = {0} ", refinery_id);

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                group by component_checklists.status
            ");

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetSvsu
                .FromSqlRaw(Sql)
                .ToList();

            List<object> data = new List<object>();

            foreach(var value in query)
            {
                dynamic o = new ExpandoObject();
                o.status = value.status;

                if(value.status == 1)
                {
                    o.category = "Safe";
                }
                else
                {
                    o.category = "Unsafe";
                }

                o.total = (int)value.total;

                data.Add(o);
            }

            if(query.Count == 1)
            {
                foreach (var value in query)
                {
                    dynamic o = new ExpandoObject();

                    if(value.status == 1)
                    {
                        o.status = 2;
                        o.category = "Unsafe";
                        o.total = 0;

                        data.Add(o);
                    }
                    else if(value.status == 2)
                    {
                        o.status = 1;
                        o.category = "Safe";
                        o.total = 0;

                        data.Add(o);
                    }
                }
            }
            
            return data;
        }

        private List<object> getTypeSafe(string from, string to, long? refinery_id)
        {

            var sb = new StringBuilder();
            sb.Append(@"
                select form_types.*, 
                (
                    select count(*) from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id 
                    where form_types.id = forms.form_type_id 
            ");

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                    and status <> 3
                ) as checklists_count, 
                (
                    select count(*) from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id
                    where form_types.id = forms.form_type_id
            ");

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                    and status = 1
                ) as checklists_pos_count, 
                (
                    select count(*) from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id 
                    where form_types.id = forms.form_type_id
            ");

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                    and status = 2
                ) as checklists_neg_count 
                from form_types where exists (
                    select * from component_checklists 
                    inner join forms on forms.id = component_checklists.form_id
                    where form_types.id = forms.form_type_id 
                    and status <> 3
                )
            ");

            sb.AppendFormat(@"and refinery_id = {0}", refinery_id);

            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetTypeSafe
                .FromSqlRaw(Sql)
                .ToList();

            List<object> data = new List<object>();

            foreach(var value in query)
            {
                dynamic o = new ExpandoObject();
                o.id = value.id;
                o.name = value.name;
                o.slug = value.slug;
                o.file = value.file;
                o.refinery_id = value.refinery_id;
                o.created_at = value.created_at;
                o.updated_at = value.updated_at;
                o.ord = value.ord;
                o.checklists_count = value.checklists_count;
                o.checklists_pos_count = value.checklists_pos_count;
                o.checklists_neg_count = value.checklists_neg_count;
                o.percentage = Attributes.getPercentageAttribute(value.checklists_count, value.checklists_pos_count);
                o.percentagen = Attributes.getPercentagenAttribute(value.checklists_count, value.checklists_neg_count);
                o.total = Attributes.getTotalAttribute(value.checklists_count, value.checklists_pos_count);
                o.pos = Attributes.getPosAttribute(value.checklists_pos_count);
                o.neg = Attributes.getNegAttribute(value.checklists_neg_count);
                o.elipsis = Attributes.getElipsisAttribue(value.name);

                data.Add(o);
            }

            return data;
        }

        private List<object> getTypeUnsafe(string from, string to, long? refinery_id)
        {

            var sb = new StringBuilder();
            sb.Append(@"
                select form_types.name,form_types.slug,
                sum(CASE WHEN component_checklists.status = 1 THEN 1 ELSE 0 END) as pos,
                sum(CASE WHEN component_checklists.status = 2 THEN 1 ELSE 0 END) as neg,
                CAST((sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END)/(sum(CASE WHEN component_checklists.status = 2 THEN 1.0 ELSE 0.0 END) + sum(CASE WHEN component_checklists.status = 1 THEN 1.0 ELSE 0.0 END))*100.0) as decimal(10, 2)) as total 
                from component_checklists 
                inner join forms on forms.id = component_checklists.form_id 
                inner join form_types on forms.form_type_id = form_types.id 
            ");

            sb.AppendFormat(@"where form_types.refinery_id = {0} and component_checklists.status != 3", refinery_id);

            if (from != null & to != null)
            {
                sb.AppendFormat(@"and component_checklists.created_at between '{0}' and '{1}' ", from, to);
            }

            sb.Append(@"
                group by form_types.name, form_types.slug 
                order by total desc
            ");


            string Sql = string.Format(sb.ToString());

            var query = _context.ChartGetTypeUnsafe
                .FromSqlRaw(Sql)
                .ToList();

            List<object> data = new List<object>();

            foreach (var value in query)
            {
                dynamic o = new ExpandoObject();
                o.name = value.name;
                o.slug = value.slug;
                o.pos = value.pos;
                o.neg = value.neg;
                o.total = value.total;
                o.elipsis = Attributes.getElipsisAttribue(value.name);

                data.Add(o);
            }

            return data;
        }

        private List<string> getUserIdByRefinery(long? refinery_id)
        {
            var user = _context.Users.Where(x => x.RefineryId == refinery_id).ToList();
            List<string> list = new List<string>();
            foreach (var item in user)
            {
                list.Add(item.Id.ToString());
            }

            return list;
        }

        public class DataPie
        {
            public string title { get; set; }
            public int? value { get; set; }
        }

        public class DataInspections
        {
            public string month { get; set; }
            public string month_n { get; set; }
            public int? safe { get; set; }
            public int? @unsafe { get; set; }
            public int? percent { get; set; }
            public int? inspection { get; set; }
            public List<DataPie> pie { get; set; }
        }

        public class RequestType
        {
            public string type { get; set; }
        }

        public class DataFormType
        {
            public long id { get; set; }
            public string slug { get; set; }
            public string file { get; set; }
            public long? refinery_id { get; set; }
            public DateTime? created_at { get; set; }
            public DateTime? updated_at { get; set; }
            public long? ord { get; set; }
            public int? checklists_count { get; set; }
            public int? checklists_pos_count { get; set; }
            public int? checklists_neg_count { get; set; }
        }
    }
}
