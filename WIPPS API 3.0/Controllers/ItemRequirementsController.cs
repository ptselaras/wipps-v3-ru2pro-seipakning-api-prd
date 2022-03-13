using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
    [Route("api/item-requirements")]
    [ApiController]
    public class ItemRequirementsController : ControllerTrait<ItemRequirement>
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static string[] form = { "id", "item_id", "requirement_id", "created_at" };
        private static string table = "item_requirements";
        private static string plural = "item-requirements";
        private static string[] fields = { "id", "created_at", "updated_at" };
        private static string cond = "id";

        public ItemRequirementsController(SafetymanContext context, IWebHostEnvironment hostingEnvironment) : base(context, hostingEnvironment, plural, cond, table, form, fields)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/ItemRequirements
        [HttpGet]
        public async Task<ActionResult<object>> GetItemRequirements(
            [FromQuery(Name = "item")] string item,
            [FromQuery(Name = "search")] string search
        )
        {

            string sQuery = getQueryTable(table, search, item);

            if(sQuery == "")
            {
                return NotFound();
            }

            var query = _context.Set<ItemRequirement2>()
                .FromSqlRaw(sQuery)
                .ToList();

            List<object> listDatas = new List<object>();

            int num = 1;

            foreach (var value in query)
            {
                num = num + 1;

                dynamic o = new ExpandoObject();
                o.id = value.Id;
                o.item = value.ItemName;
                o.slug = value.RequirementSlug;
                o.item_id = value.ItemId;
                o.requirement_id = value.RequirementId;
                o.requirement = value.RequirementName;
                o.created_at = value.CreatedAt;
                o.num = num;
                o.created = value.CreatedAt.Value.ToString("ddd. yyyy, dd MMMM HH:mm:ss");

                string sub = "";
                string footnote = "";

                string[] splitter = value.RequirementName.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    o.sub = splitter[0];
                    o.text = splitter[1];
                    sub = splitter[0];
                }

                string[] splitter2 = value.RequirementName.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    o.footnote = splitter2[1];
                    o.text = splitter2[0];
                    footnote = splitter2[1];
                }

                if(value.Order == null)
                {
                    o.order = value.Id;
                }
                else {
                    o.order = value.Order;
                }

                if (!(sub != ""))
                {
                    o.sub = "Others";
                }

                if (!(footnote != ""))
                {
                    o.footnote = "Others";
                }

                listDatas.Add(o);
            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas;
            response.success = true;

            return response;
        }

        // GET: api/ItemRequirements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetItemRequirement(long id)
        {
            var query = _context.ItemRequirements
                             .Where(x => x.Id == id)
                             .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.item_id = query.ItemId;
            data.requirement_id = query.RequirementId;
            data.order = query.Order;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = true;

            return response;
        }

        // PUT: api/ItemRequirements/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{slug}")]
        public async Task<ActionResult<object>> PutItemRequirement(string slug, RequestItem request)
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

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
                return NotFound("Not Found");
            }

            List<int> attached = new List<int>();
            List<long> updated = new List<long>();
            var detached = new ExpandoObject() as IDictionary<string, Object>;

            try
            {
                List<object> requirementsIds = new List<object>();
                List<ItemRequirement> itemRequirements = _context.ItemRequirements.Where(x => x.ItemId == query.Id).ToList();

                int index = 0;

                foreach (DataRequirement value in request.requirements)
                {
                    bool isNumeric = int.TryParse(value.requirement, out _);

                    if (isNumeric)
                    {
                        int findIndex = itemRequirements.FindIndex(o => o.Id == int.Parse(value.order) || o.Order == int.Parse(value.order));

                        if (findIndex != -1)
                        {
                            updated.Add(itemRequirements[findIndex].RequirementId);
                        }
                        else
                        {
                            attached.Add(int.Parse(value.requirement));

                            ItemRequirement itemRequirementModel = new ItemRequirement();
                            itemRequirementModel.ItemId = query.Id;
                            itemRequirementModel.RequirementId = long.Parse(value.requirement);
                            itemRequirementModel.Order = long.Parse(value.order);
                            itemRequirementModel.CreatedAt = DateTime.Now;
                            itemRequirementModel.UpdatedAt = DateTime.Now;

                            _context.ItemRequirements.Add(itemRequirementModel);

                            await _context.SaveChangesAsync();

                        }
                    }
                    else
                    {
                        Requirement requirementModel = new Requirement();
                        requirementModel.Name = value.requirement;
                        requirementModel.Slug = StringExtensions.Slugify(value.requirement);
                        requirementModel.RefineryId = (long)user.RefineryId;
                        requirementModel.CreatedAt = DateTime.Now;
                        requirementModel.UpdatedAt = DateTime.Now;

                        _context.Requirements.Add(requirementModel);

                        await _context.SaveChangesAsync();

                        ItemRequirement itemRequirementModel = new ItemRequirement();
                        itemRequirementModel.ItemId = query.Id;
                        itemRequirementModel.RequirementId = requirementModel.Id;
                        itemRequirementModel.Order = long.Parse(value.order);
                        itemRequirementModel.CreatedAt = DateTime.Now;
                        itemRequirementModel.UpdatedAt = DateTime.Now;

                        _context.ItemRequirements.Add(itemRequirementModel);

                        await _context.SaveChangesAsync();

                        attached.Add(int.Parse(itemRequirementModel.RequirementId.ToString()));
                    }
                    index++;
                }

                if (request.requirements.Count() < itemRequirements.Count())
                {
                    int index3 = 0;

                    ItemRequirement itemRequirementDeleted = new ItemRequirement();

                    for (var i = 0; i < itemRequirements.Count; i++)
                    {
                        var find = request.requirements.Find(o => int.Parse(o.order) == itemRequirements[i].Id);

                        if (find == null)
                        {
                            detached.Add(i.ToString(), itemRequirements[i].RequirementId);

                            itemRequirementDeleted.Id = itemRequirements[i].Id;
                            itemRequirementDeleted.ItemId = itemRequirements[i].ItemId;
                            itemRequirementDeleted.Item = itemRequirements[i].Item;
                            itemRequirementDeleted.ItemInspectionChecklists = itemRequirements[i].ItemInspectionChecklists;
                            itemRequirementDeleted.Order = itemRequirements[i].Order;
                            itemRequirementDeleted.Requirement = itemRequirements[i].Requirement;
                            itemRequirementDeleted.RequirementId = itemRequirements[i].RequirementId;
                            itemRequirementDeleted.UpdatedAt = itemRequirements[i].UpdatedAt;
                            itemRequirementDeleted.CreatedAt = itemRequirements[i].CreatedAt;
                        }
                    }

                    ItemRequirement itemRequirement = _context.ItemRequirements.Where(x => x.RequirementId == itemRequirementDeleted.RequirementId && x.ItemId == query.Id).FirstOrDefault();

                    _context.ItemRequirements.Remove(itemRequirement);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                dynamic error = new ExpandoObject();
                error.code = 500;
                error.data = "Error on save your data";
                error.success = false;
                return error;
            }

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.name = query.Name;
            data.slug = query.Slug;
            data.file = query.File;
            data.refinery_id = query.RefineryId;
            data.created_at = query.CreatedAt;
            data.updated_at = query.UpdatedAt;
            data.ord = null;
            data.percentage = "0.00";
            data.percentagen = "0.00";
            data.total = "0.00";
            data.pos = "0";
            data.neg = "0";
            data.elipsis = Attributes.getElipsisAttribue(query.Name);

            dynamic success = new ExpandoObject();
            success.attached = attached;

            if (detached.Count() > 0)
            {
                success.detached = detached;
            }
            else
            {
                success.detached = new string[] { };
            }
            success.updated = updated;

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = data;
            response.success = success;

            return response;
        }

        // POST: api/ItemRequirements
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<object>> PostItemRequirement(RequestAddItem request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            ItemRequirement itemRequirement = new ItemRequirement();

            try
            {

                itemRequirement.ItemId = request.item_id;
                itemRequirement.RequirementId = request.requirement_id;
                itemRequirement.CreatedAt = DateTime.Now;
                itemRequirement.UpdatedAt = DateTime.Now;

                _context.ItemRequirements.Add(itemRequirement);
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

            var query = await _context.ItemRequirements.Where(x => x.Id == itemRequirement.Id).FirstOrDefaultAsync();

            dynamic data = new ExpandoObject();
            data.id = query.Id;
            data.item_id = query.ItemId;
            data.requirement_id = query.RequirementId;
            data.order = null;
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

            var query = await _context.ItemRequirements.ToListAsync();

            List<object> listDatas = new List<object>();
            foreach (var value in query.ToList())
            {
                dynamic o = new ExpandoObject();

                o.id = value.Id;
                o.text = value.ItemId;
                listDatas.Add(o);

            }

            dynamic response = new ExpandoObject();
            response.code = 200;
            response.data = listDatas.ToList();

            return response;
        }

        // DELETE: api/ItemRequirements/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ItemRequirement>> DeleteItemRequirement(long id)
        {
            var query = _context.ItemRequirements
                            .Where(x => x.Id == id)
                            .FirstOrDefault();

            if (query == null)
            {
                return NotFound("Not Found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ItemRequirements.Remove(query);
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

        private bool ItemRequirementExists(long id)
        {
            return _context.ItemRequirements.Any(e => e.Id == id);
        }

        public string getQueryTable(string table, string search, string slug)
        {
            string[] columns = { "item_requirements.*", "items.name as ItemName", "requirements.slug as RequirementSlug", "items.id as ItemId", "requirements.id as RequirementId", "requirements.name as RequirementName" };
            string[] columnsSearch = { "item_requirements.id", "items.name", "requirements.name", "item_requirements.created_at"};

            string sQuery = "";
            var SqlStr = new StringBuilder();

            SqlStr.Append("SELECT " + string.Join(", ", columns) + " FROM " + table + " ");

            SqlStr.Append("INNER JOIN items ON items.id = item_requirements.item_id ");
            SqlStr.Append("INNER JOIN requirements ON requirements.id = item_requirements.requirement_id ");

            SqlStr.Append(" WHERE ");

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

                var items = _context.Items.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();
                if (items != null)
                {
                    SqlStr.Append(" AND item_requirements.item_id = " + items.Id + " ");
                }
                else
                {
                    return "";
                }
            }
            else
            {
                var items = _context.Items.Where(x => x.Slug.ToLower() == slug.ToLower()).FirstOrDefault();
                if (items != null)
                {
                    SqlStr.Append(" item_requirements.item_id = " + items.Id + " ");
                }
                else
                {
                    return "";
                }
            }

            SqlStr.Append(" ORDER BY item_requirements.id asc ");

            sQuery = string.Format(SqlStr.ToString());

            return sQuery;
        }

        public class DataRequirement
        {
            public string order { get; set; }
            public string requirement { get; set; }
        }

        public class RequestItem
        {
            public List<DataRequirement> requirements { get; set; }
            public string _method { get; set; }
        }

        public class RequestAddItem
        {
            public long item_id { get; set; }
            public long requirement_id { get; set; }
        }

        private class Validator : AbstractValidator<RequestItem>
        {
            public Validator()
            {
                RuleFor(m => m.requirements).NotEmpty().WithMessage("required");
            }
        }
    }
}
