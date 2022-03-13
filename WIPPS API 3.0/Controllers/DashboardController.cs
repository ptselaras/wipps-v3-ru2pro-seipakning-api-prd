using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPPS_API_3._0.Models;
using WIPPS_API_3._0.Models.Procedure;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers
{
    [Authorize]
    [Route("api/dashboard-v2")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly SafetymanContext _context;
        public DashboardController(SafetymanContext context)
        {
            _context = context;
        }

        [HttpGet("chart/companies-history")]
        public async Task<ActionResult<object>> GetHistory(
            [FromQuery(Name = "search[value]")] string search,
            [FromQuery] IEnumerable<Dictionary<string, string>> order,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "length")] int length,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "to")] string to,
            [FromQuery(Name = "company")] string company,
            [FromQuery(Name = "form")] string form
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            string sQuery = getQueryTable("forms", search, company, form, from, to, order);

            var query = _context.Set<Form2>()
                .FromSqlRaw(sQuery)
                .ToList();

            List<object> listDatas = new List<object>();

            int initNumber = start;

            foreach (var item in query)
            {
                initNumber = initNumber + 1;

                dynamic o = new ExpandoObject();
                o.id = item.Id;
                o.slug = item.Slug;
                o.area = item.AreaName;
                o.company = item.CompanyName;
                o.job = item.JobName;
                o.form = item.FormName;
                o.created_at = item.CreatedAt;
                o.row_number = initNumber;

                listDatas.Add(o);
            }

            

            string sqlResults = "";

            if(form != null)
            {
                var queryCompany = _context.Companies.Where(x => x.Slug.ToLower() == company.ToLower()).FirstOrDefault();
                var queryForm = _context.FormTypes.Where(x => x.Slug.ToLower() == form.ToLower()).FirstOrDefault();

                sqlResults = string.Format(@"
                    SELECT form_types.*,
                    CASE WHEN DATALENGTH(form_types.name)>16 THEN CONCAT(LEFT(form_types.name,14), '..') ELSE form_types.name END AS elipsis,
                    CAST(CASE WHEN total.total IS NOT NULL THEN total.total ELSE 0 END AS decimal(10,2)) AS checklists_count, 
	                CAST(CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE 0 END AS decimal(10,2)) AS checklists_pos_count, 
	                CAST(CASE WHEN negative.total IS NOT NULL THEN negative.total ELSE 0 END AS decimal(10,2)) AS checklists_neg_count, 
	                CAST(CASE WHEN positive.total IS NOT NULL THEN total.safe ELSE 0 END AS decimal(10,2)) AS pos, 
	                CAST(CASE WHEN negative.total IS NOT NULL THEN total.unsafe ELSE 0 END AS decimal(10,2)) AS neg, 
                    CAST(ROUND(((CASE WHEN negative.total IS NOT NULL THEN negative.total ELSE NULL END))/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END)*100,2) AS DECIMAL(10,2)) AS percentagen,
                    CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) AS percentage, 
                    CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) AS totals
                    FROM form_types 
                    LEFT JOIN ( 
                        SELECT SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS total, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND forms.form_type_id = {1} 
                        AND component_checklists.created_at BETWEEN '{2}' AND '{3}' 
                        AND component_checklists.status = 1 
                        GROUP BY forms.form_type_id
                    ) AS positive ON positive.id = form_types.id 
                    LEFT JOIN ( 
                        SELECT SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS total, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND forms.form_type_id = {1} 
                        AND component_checklists.created_at BETWEEN '{2}' AND '{3}' 
                        AND component_checklists.status = 2 
                        GROUP BY forms.form_type_id 
                    ) AS negative ON negative.id = form_types.id 
                    LEFT JOIN ( 
                        SELECT (SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value))+SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value))) AS total, 
                        SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS safe, 
                        SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS unsafe, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND forms.form_type_id = {1} 
                        AND component_checklists.created_at BETWEEN '{2}' AND '{3}' 
                        AND component_checklists.status <> 3 GROUP BY forms.form_type_id 
                    ) AS total ON total.id = form_types.id 
                    WHERE form_types.refinery_id = {4} 
                    AND CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) IS NOT NULL 
                    ORDER BY totals DESC
                ", queryCompany.Id, queryForm.Id, from, to, user.RefineryId);
            }
            else
            {
                var queryCompany = _context.Companies.Where(x => x.Slug.ToLower() == company.ToLower()).FirstOrDefault();
                sqlResults = string.Format(@"
                    SELECT form_types.*,
                    CASE WHEN DATALENGTH(form_types.name)>16 THEN CONCAT(LEFT(form_types.name,14), '..') ELSE form_types.name END AS elipsis,
                    CAST(CASE WHEN total.total IS NOT NULL THEN total.total ELSE 0 END AS decimal(10,2)) AS checklists_count, 
	                CAST(CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE 0 END AS decimal(10,2)) AS checklists_pos_count, 
	                CAST(CASE WHEN negative.total IS NOT NULL THEN negative.total ELSE 0 END AS decimal(10,2)) AS checklists_neg_count, 
	                CAST(CASE WHEN positive.total IS NOT NULL THEN total.safe ELSE 0 END AS decimal(10,2)) AS pos, 
	                CAST(CASE WHEN negative.total IS NOT NULL THEN total.unsafe ELSE 0 END AS decimal(10,2)) AS neg, 
                    CAST(ROUND(((CASE WHEN negative.total IS NOT NULL THEN negative.total ELSE NULL END))/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END)*100,2) AS DECIMAL(10,2)) AS percentagen,
                    CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) AS percentage, 
                    CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) AS totals
                    FROM form_types 
                    LEFT JOIN ( 
                        SELECT SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS total, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND component_checklists.created_at BETWEEN '{1}' AND '{2}' 
                        AND component_checklists.status = 1 
                        GROUP BY forms.form_type_id
                    ) AS positive ON positive.id = form_types.id 
                    LEFT JOIN ( 
                        SELECT SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS total, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND component_checklists.created_at BETWEEN '{1}' AND '{2}'  
                        AND component_checklists.status = 2 GROUP BY forms.form_type_id 
                    ) AS negative ON negative.id = form_types.id 
                    LEFT JOIN ( 
                        SELECT (SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value))+SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value))) AS total, 
                        SUM(component_checklists.safe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS safe, 
                        SUM(component_checklists.unsafe_value/(component_checklists.safe_value+component_checklists.unsafe_value)) AS unsafe, 
                        forms.form_type_id AS id 
                        FROM component_checklists 
                        INNER JOIN forms ON forms.id = component_checklists.form_id 
                        WHERE forms.company_id = {0} 
                        AND component_checklists.created_at BETWEEN '{1}' AND '{2}' 
                        AND component_checklists.status <> 3 
                        GROUP BY forms.form_type_id 
                    ) AS total ON total.id = form_types.id 
                    WHERE form_types.refinery_id = {3} 
                    AND CAST(ROUND((CASE WHEN positive.total IS NOT NULL THEN positive.total ELSE NULL END)/(CASE WHEN total.total IS NOT NULL THEN total.total ELSE NULL END )*100,2) AS DECIMAL(10,2)) IS NOT NULL 
                    ORDER BY totals DESC
                ", queryCompany.Id, from, to, user.RefineryId);
            }

            var results = _context.FormTypes2
                .FromSqlRaw(sqlResults)
                .ToList();

            var dropdown = _context.Forms
                .Include(x => x.FormType)
                .Include(x => x.Company)
                .Where(x => x.Company.Slug.ToLower() == company.ToLower())
                .GroupBy(x => new
                {
                    x.FormType.Slug,
                    x.FormType.Name
                })
                .Select(x => new DataDropdown
                {
                    value = x.Key.Slug,
                    key = x.Key.Slug,
                    text = x.Key.Name
                })
                .ToList();

            dynamic response = new ExpandoObject();
            response.total = listDatas.Count();
            response.data = listDatas
                .Skip(start)
                .Take(length)
                .ToList();
            response.results = results;
            response.dropdown = dropdown;

            return Ok(response);
        }

        [HttpGet("chart/areas")]
        public async Task<ActionResult<object>> ChartAreas(
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

            var query = _context.GetAreaSafe
                .FromSqlRaw($"getAreaSafe '{from}', '{to}', {user.RefineryId}").ToList();
            
            return Ok(query);
        }

        [HttpGet("chart/areaus")]
        public async Task<ActionResult<object>> ChartAreasus(
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

            var query = _context.GetAreaSafe
                .FromSqlRaw($"getAreaUnsafe '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/inspection")]
        public async Task<ActionResult<object>> ChartInspection(
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

            var query = _context.GetInspections
                .FromSqlRaw($"getInspections '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/types")]
        public async Task<ActionResult<object>> ChartTypes(
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

            var query = _context.GetTypeSafe
                .FromSqlRaw($"getTypeSafe '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/location")]
        public async Task<ActionResult<object>> ChartLocation(
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

            var query = _context.GetLocations
                .FromSqlRaw($"getLocations '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/company")]
        public async Task<ActionResult<object>> ChartCompany(
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

            var query = _context.GetCompanies
                .FromSqlRaw($"getCompanies '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/inspector")]
        public async Task<ActionResult<object>> ChartInspector(
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

            var query = _context.GetInspectorInspections
                .FromSqlRaw($"getInspectorInspections '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/company_inspection")]
        public async Task<ActionResult<object>> ChartCompanyInspection(
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

            var query = _context.GetCompanyInspections
                .FromSqlRaw($"getCompanyInspections '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/inspector_score")]
        public async Task<ActionResult<object>> ChartInspectorScore(
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

            var query = _context.GetInspectorScores
                .FromSqlRaw($"getInspectorScore '{from}', '{to}', {user.RefineryId}").ToList();

            return Ok(query);
        }

        [HttpGet("chart/svsu")]
        public async Task<ActionResult<object>> ChartSvsu(
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

            var query = _context.GetSvsu
                .FromSqlRaw($"getSvsu '{from}', '{to}', {user.RefineryId}").ToList();

            List<ResponseSvsu> data = new List<ResponseSvsu>();
            data.Add(new ResponseSvsu
            {
                status = 1,
                category = "Safe",
                total = 0
            });
            data.Add(new ResponseSvsu
            {
                status = 2,
                category = "Unsafe",
                total = 0
            });

            if(query[0] != null)
            {
                if (query[0].safe != null)
                {
                    data[0].total = query[0].safe;
                }

                if (query[0].@unsafe != null)
                {
                    data[1].total = query[0].@unsafe;
                }
            }

            

            return Ok(data);
        }

        public string getQueryTable(string table, string search, string company, string form, string from, string to, IEnumerable<Dictionary<string, string>> order)
        {
            string[] columns = { "forms.*", "areas.name as AreaName",  "form_types.name as FormName", "companies.name as CompanyName", "jobs.name as JobName"};
            string[] columnsSearch = { "forms.id", "areas.name", "companies.name", "jobs.created_at", "form_types.name", "forms.created_at" };

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN form_types ON form_types.id = forms.form_type_id ");
            SqlStr.Append("INNER JOIN companies ON companies.id = forms.company_id ");
            SqlStr.Append("INNER JOIN jobs ON jobs.id = forms.job_id ");
            SqlStr.Append("INNER JOIN areas ON areas.id = forms.area_id ");

            SqlStr.Append(" WHERE companies.slug = '" + company + "' ");
            if(form != null)
            {
                SqlStr.Append(" AND form_types.slug = '" + form + "' ");
            }
            if (search != null)
            {
                var index = 0;
                foreach (var column in columnsSearch)
                {
                    if ((columnsSearch.Length - 1) == index)
                    {
                        SqlStr.Append(column + " like '%" + search + "%' ");
                    }
                    else
                    {
                        SqlStr.Append(column + " like '%" + search + "%' OR ");
                    }

                    index++;
                }
            }

            if (from != null && to != null)
            {
                SqlStr.Append(" AND forms.created_at BETWEEN '" + from + "' AND '" + to + "' ");
            }

            foreach (var ord in order.ToList())
            {
                if (ord["column"] == "0")
                {
                    SqlStr.Append(" ORDER BY forms.created_at " + ord["dir"] + " ");
                }
                else
                {
                    SqlStr.Append(" ORDER BY " + columnsSearch[int.Parse(ord["column"])] + " " + ord["dir"] + " ");
                }
            }

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public class DataDropdown
        {
            public string value { get; set; }
            public string key { get; set; }
            public string text { get; set; }
        }

        public class ResponseSvsu
        {
            public int status { get; set; }
            public string category { get; set; }
            public decimal? total { get; set; }
        }
    }
}
