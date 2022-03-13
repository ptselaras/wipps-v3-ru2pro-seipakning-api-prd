using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPPS_API_3._0.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WIPPS_API_3._0.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly SafetymanContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public ExportController(SafetymanContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<object>> ExportAll(
            [FromQuery(Name = "to")] string to,
            [FromQuery(Name = "from")] string from
        )
        {
            string username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Name).Value;
            }

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var newData = GetExportData(user.RefineryId, from, to);

            return newData;
        }

        [HttpGet("all/excel")]
        public async Task<ActionResult<object>> ExportAllExcel(
            [FromQuery(Name = "to")] string to,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "token")] string token
        )
        {
            var jwt = token;
            var handler = new JwtSecurityTokenHandler();
            string username = handler.ReadJwtToken(jwt).Claims.First().Value;

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var newData = GetExportData(user.RefineryId, from, to);

            var groupData = newData.GroupBy(x => x.Type)
                .Select(g => new { g.Key, Data = g.ToList() })
                .ToDictionary(x => x.Key, x => x.Data);

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = string.Format("Export All {0}.xlsx", DateTime.Now.ToString("dd MMM yyy"));
            string fileImageLogo = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "logo.png");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet ws = workbook.Worksheets.Add("Worksheet");

                    string last_column = "L";

                    ws.Cell("A1").Value = "WIPPS REPORT";
                    ws.Range(string.Format("A1:{0}1", last_column)).Row(1).Merge();
                    ws.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Cell("A1").Style.Font.FontSize = 30;
                    ws.Cell("A1").Style.Font.Bold = true;

                    string fileImageIdentitas = "";
                    string nameIdentitas = "";

                    if (user.RefineryId == 1)
                    {
                        nameIdentitas = "RU IV";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_iv.png");
                    }
                    else if (user.RefineryId == 2)
                    {
                        nameIdentitas = "RU VI";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_vi.png");
                    }
                    else if (user.RefineryId == 3)
                    {
                        nameIdentitas = "RU V";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_v.png");
                    }
                    else if (user.RefineryId == 4)
                    {
                        nameIdentitas = "RU II";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_ii.png");
                    }
                    else if (user.RefineryId == 5)
                    {
                        nameIdentitas = "RU III";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_iii.png");
                    }
                    else if (user.RefineryId == 6)
                    {
                        nameIdentitas = "RU II";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_ii.png");
                    }
                    else if (user.RefineryId == 7)
                    {
                        nameIdentitas = "RU VII";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_vii.png");
                    }

                    var imageIdentitas = ws.AddPicture(fileImageIdentitas)
                        .MoveTo(ws.Cell("H3"))
                        .Scale(0.19);
                    imageIdentitas.Name = nameIdentitas;

                    var imageLogo = ws.AddPicture(fileImageLogo)
                        .MoveTo(ws.Cell("A3"))
                        .Scale(0.09);
                    imageLogo.Name = "WIPPS Logo";

                    var dateRange = ws.Range(string.Format("A10:{0}10", last_column)).Row(1).RowBelow();
                    dateRange.Merge();
                    dateRange.Value = string.Format("Date range : {0} - {1}", from, to);
                    dateRange.RowBelow().Merge();

                    var dataRow = dateRange.RowBelow().RowBelow();

                    List<string> titleBold = new List<string>();
                    double? finalTotal = 0;
                    double? finalPos = 0;
                    double? finalNeg = 0;
                    List<DataFinal> arrayFinal = new List<DataFinal>();

                    DataTable table = new DataTable();
                    foreach (string value in groupData.Keys)
                    {
                        table.Columns.Add(value, typeof(string));
                        titleBold.Add(value);
                    }

                    table.Columns.Add("", typeof(string));
                    table.Columns.Add("Pos", typeof(string));
                    table.Columns.Add("Neg", typeof(string));
                    table.Columns.Add("Total", typeof(string));
                    table.Columns.Add("%", typeof(string));

                    foreach (KeyValuePair<string, List<Data>> kvp in groupData)
                    {
                        double? pos = 0;
                        double? neg = 0;
                        double? total = 0;

                        table.Rows.Add(kvp.Key, "Pos", "Neg", "Total", "%");

                        foreach (var value in kvp.Value)
                        {
                            pos += value.Safe;
                            neg += value.Unsafe;
                            total += value.Sum;

                            table.Rows.Add(value.Name, value.Safe, value.Unsafe, value.Sum, value.Percent);
                        }

                        decimal percent = (Math.Floor((decimal)(((pos / total) * 100) * 100)) / 100);

                        table.Rows.Add("Category Total", pos, neg, total, percent + "%");
                        table.Rows.Add("", "", "", "", "");

                        finalTotal += total;
                        finalPos += pos;
                        finalNeg += neg;

                        arrayFinal.Add(new DataFinal
                        {
                            text = kvp.Key,
                            pos = (long?)pos,
                            neg = (long?)neg,
                            total = (long?)total,
                            percent = percent.ToString()
                        });
                    }

                    table.Rows.Add("Category Totals", "Pos", "Neg", "Total", "%");

                    foreach (DataFinal value in arrayFinal)
                    {
                        table.Rows.Add(value.text, value.pos, value.neg, value.total, value.percent + "%");
                    }

                    if(arrayFinal.Count > 0)
                    {
                        decimal finalPercent = (Math.Floor((decimal)((finalPos / finalTotal) * 100) * 100) / 100);
                        table.Rows.Add("Grand Total", finalPos, finalNeg, finalTotal, finalPercent + "%");
                    }
                    else
                    {
                        table.Rows.Add("Grand Total", finalPos, finalNeg, finalTotal, 0 + "%");
                    }

                    titleBold.Add("Category Total");
                    titleBold.Add("Category Totals");
                    titleBold.Add("Grand Total");

                    ws.Cell(dataRow.RowNumber(), 1).InsertData(table.AsEnumerable());

                    for (var i = 0; i < table.Rows.Count; i++)
                    {
                        ws.Column(2).Cell(dataRow.RowNumber() + i).CopyTo(ws.Cell(string.Format("H{0}", (dataRow.RowNumber() + i))));
                        ws.Column(3).Cell(dataRow.RowNumber() + i).CopyTo(ws.Cell(string.Format("I{0}", (dataRow.RowNumber() + i))));
                        ws.Column(4).Cell(dataRow.RowNumber() + i).CopyTo(ws.Cell(string.Format("J{0}", (dataRow.RowNumber() + i))));
                        ws.Column(5).Cell(dataRow.RowNumber() + i).CopyTo(ws.Cell(string.Format("K{0}", (dataRow.RowNumber() + i))));
                        ws.Range((dataRow.RowNumber() + i), 1, (dataRow.RowNumber() + i), 7).Row(1).Merge();

                        foreach (string value in titleBold)
                        {
                            if (table.AsEnumerable().ToList()[i].ItemArray.ToList()[0].Equals(value))
                            {
                                ws.Range((dataRow.RowNumber() + i), 1, (dataRow.RowNumber() + i), 11).Row(1).Style.Font.Bold = true;
                            }
                        }
                    }


                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet("all/excel-form")]
        public async Task<ActionResult<object>> ExportAllExcelForm(
            [FromQuery(Name = "to")] string to,
            [FromQuery(Name = "from")] string from,
            [FromQuery(Name = "token")] string token
        )
        {
            var jwt = token;
            var handler = new JwtSecurityTokenHandler();
            string username = handler.ReadJwtToken(jwt).Claims.First().Value;

            var user = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var sb = new StringBuilder();
            sb.AppendFormat(@"
                SELECT forms.*
                FROM forms
                INNER JOIN users ON users.id = forms.created_user_id
                WHERE users.refinery_id = {0}
            ", user.RefineryId);

            if (from != "" && to != "")
            {
                sb.AppendFormat(@" AND forms.created_at BETWEEN '{0}' AND '{1}'", from, to);
            }

            string sql = string.Format(sb.ToString());

            var query = _context.Forms
                .FromSqlRaw(sql)
                .Include(x => x.Area)
                .Include(x => x.FormType)
                .Include(x => x.Company)
                .ToList();

            List<DataForm> dataForms = new List<DataForm>();
            foreach (var value in query)
            {
                DataForm dataForm = new DataForm();
                dataForm.Id = value.Id;
                dataForm.CreatedAt = value.CreatedAt;
                dataForm.Name = value.Name;
                dataForm.Equipment = value.Equipment;
                dataForm.Area = value.Area;
                dataForm.Company = value.Company;
                dataForm.FormType = value.FormType;
                dataForm.MonthYear = value.CreatedAt.Value.ToString("MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));
                Job job = _context.Jobs.Where(x => x.Id == value.JobId).FirstOrDefault();
                dataForm.JobName = job.Name;

                User observer = _context.Users.Where(x => x.Id == value.CreatedUserId).FirstOrDefault();
                dataForm.Observer = observer.Name;

                var formPercentages = _context.ComponentChecklists
                    .Where(x => x.FormId == value.Id)
                    .GroupBy(x => x.FormId)
                    .Select(cc => new FormPercentage
                    {
                        FormId = cc.Key,
                        Inspection = "1",
                        Safe = Math.Round((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Unsafe = Math.Round((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))), 2).ToString(),
                        Percentage = Decimal.Round((decimal)((decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) / (((decimal)cc.Sum(s => (s.UnsafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))) + (decimal)cc.Sum(s => (s.SafeValue ?? 0.0 / (s.SafeValue ?? 0.0 + s.UnsafeValue ?? 0.0))))) * 100), 2).ToString()
                    }).ToList();

                dataForm.Percentage = formPercentages;
                dataForm.Safe = formPercentages[0].Safe != null ? formPercentages[0].Safe.Replace(".", ",") : "0";
                dataForm.UnSafe = formPercentages[0].Unsafe != null ? formPercentages[0].Unsafe.Replace(".", ",") : "0";
                if (formPercentages[0].Safe != null && formPercentages[0].Unsafe != null)
                {
                    dataForm.Total = string.Format((double.Parse(formPercentages[0].Safe) + double.Parse(formPercentages[0].Unsafe)).ToString());
                }
                else
                {
                    dataForm.Total = "0";
                }

                dataForm.Percent = formPercentages[0].Percentage.Replace(".", ",") ?? "0";

                dataForms.Add(dataForm);
            }

            var groupData = dataForms.GroupBy(x => x.MonthYear)
                .Select(g => new { g.Key, Data = g.ToList() })
                .ToDictionary(x => x.Key, x => x.Data);

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = string.Format("Export All Form {0}.xlsx", DateTime.Now.ToString("dd MMM yyy"));

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    if(groupData.Count > 0)
                    {
                        foreach (KeyValuePair<string, List<DataForm>> kvp in groupData)
                        {
                            IXLWorksheet ws = workbook.Worksheets.Add(kvp.Key);

                            DataTable table = new DataTable();
                            table.Columns.Add("Jenis CLSR", typeof(string));
                            table.Columns.Add("Tanggal", typeof(string));
                            table.Columns.Add("Jam", typeof(string));
                            table.Columns.Add("Pelaksana", typeof(string));
                            table.Columns.Add("Lokasi", typeof(string));
                            table.Columns.Add("Equipment", typeof(string));
                            table.Columns.Add("Pekerjaan", typeof(string));
                            table.Columns.Add("Observer", typeof(string));
                            table.Columns.Add("Total Safe", typeof(string));
                            table.Columns.Add("Total Unsafe", typeof(string));
                            table.Columns.Add("Safe + Unsafe", typeof(string));
                            table.Columns.Add("Presentasi Perilaku Aman", typeof(string));

                            table.Rows.Add(
                                    "Jenis CLSR",
                                    "Tanggal",
                                    "Jam",
                                    "Pelaksana",
                                    "Lokasi",
                                    "Equipment",
                                    "Pekerjaan",
                                    "Observer",
                                    "Total Safe",
                                    "Total Unsafe",
                                    "Safe + Unsafe",
                                    "Presentasi Perilaku Aman"
                                );

                            foreach (var value in kvp.Value)
                            {
                                table.Rows.Add(
                                    value.FormType.Name,
                                    value.CreatedAt.Value.ToString("dd MMMM yyy"),
                                    value.CreatedAt.Value.ToString("HH:mm:ss"),
                                    value.Company.Name,
                                    value.Area.Name,
                                    value.Equipment,
                                    value.JobName,
                                    value.Observer,
                                    value.Safe,
                                    value.UnSafe,
                                    value.Total,
                                    value.Percent
                                );
                            }

                            ws.Cell(1, 1).InsertData(table.AsEnumerable());
                            ws.Range("A1:L1").Row(1).Style.Font.Bold = true;

                            for (var i = 1; i <= 12; i++)
                            {
                                if (i == 7)
                                {
                                    ws.Column(i).Width = 50;
                                }
                                else
                                {
                                    ws.Column(i).Width = 20;
                                }

                            }
                        }
                    }
                    else
                    {
                        IXLWorksheet ws = workbook.Worksheets.Add("Workbook");

                        DataTable table = new DataTable();
                        table.Columns.Add("Jenis CLSR", typeof(string));
                        table.Columns.Add("Tanggal", typeof(string));
                        table.Columns.Add("Jam", typeof(string));
                        table.Columns.Add("Pelaksana", typeof(string));
                        table.Columns.Add("Lokasi", typeof(string));
                        table.Columns.Add("Equipment", typeof(string));
                        table.Columns.Add("Pekerjaan", typeof(string));
                        table.Columns.Add("Observer", typeof(string));
                        table.Columns.Add("Total Safe", typeof(string));
                        table.Columns.Add("Total Unsafe", typeof(string));
                        table.Columns.Add("Safe + Unsafe", typeof(string));
                        table.Columns.Add("Presentasi Perilaku Aman", typeof(string));

                        table.Rows.Add(
                                "Jenis CLSR",
                                "Tanggal",
                                "Jam",
                                "Pelaksana",
                                "Lokasi",
                                "Equipment",
                                "Pekerjaan",
                                "Observer",
                                "Total Safe",
                                "Total Unsafe",
                                "Safe + Unsafe",
                                "Presentasi Perilaku Aman"
                            );

                        ws.Cell(1, 1).InsertData(table.AsEnumerable());
                        ws.Range("A1:L1").Row(1).Style.Font.Bold = true;

                        for (var i = 1; i <= 12; i++)
                        {
                            if (i == 7)
                            {
                                ws.Column(i).Width = 50;
                            }
                            else
                            {
                                ws.Column(i).Width = 20;
                            }

                        }
                    }
                    

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }

                }
            }
            catch (Exception e)
            {
                return e.Message;
            }


        }

        [HttpGet("single/excel")]
        public async Task<ActionResult<object>> ExportSingleExcel([FromQuery(Name = "slug")] string slug, [FromQuery(Name = "token")] string token)
        {
            var jwt = token;
            var handler = new JwtSecurityTokenHandler();
            string username = handler.ReadJwtToken(jwt).Claims.First().Value;

            var users = await _context.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

            var model = await _context.Forms.Where(x => x.Slug.ToLower() == slug.ToLower())
                .Include(companies => companies.Company)
                .Include(areas => areas.Area)
                .ThenInclude(areasEmail => areasEmail.AreaEmails)
                .Include(forms => forms.FormType)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(attachmants => attachmants.ComponentAttachments)
                .Include(components => components.ComponentChecklists)
                .ThenInclude(formComponents => formComponents.FormComponent)
                .ThenInclude(components => components.Component)
                .FirstOrDefaultAsync();

            var base_url = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

            DataReport dataReport = new DataReport();
            dataReport.area_id = model.Area.Name;

            Job job = _context.Jobs.Where(x => x.Id == model.JobId).FirstOrDefault();
            dataReport.job_id = job.Name;

            dataReport.form_type_id = model.FormType.Name;
            dataReport.companies = model.Company;

            List<Components> componentsList = new List<Components>();
            foreach (var value in model.ComponentChecklists)
            {
                componentsList.Add(new Components
                {

                    note = value.Description,
                    id = value.Id,
                    status = value.Status,
                    text = value.FormComponent.Component.Name ?? "-",
                    images = value.ComponentAttachments.ToList()
                });
            }

            List<Components> components = new List<Components>();

            foreach (var value in componentsList)
            {
                List<ComponentAttachment> images = new List<ComponentAttachment>();

                foreach (var image in value.images)
                {
                    ComponentAttachment v = new ComponentAttachment();

                    if (image.File != "")
                    {
                        v.File = base_url + "/storage/app/public/" + image.File;
                    }

                    images.Add(v);
                }

                Components val = new Components();
                val.note = value.note;
                val.id = value.id;
                val.status = value.status;
                val.text = value.text;
                val.images = images;

                string[] splitter = value.text.Split("<sub>");
                if (splitter.Count() == 2)
                {
                    val.sub = splitter[0];
                    val.text = splitter[1];
                }

                string[] splitter2 = value.text.Split("<footnote>");
                if (splitter2.Count() == 2)
                {
                    val.footnote = splitter2[1];
                    val.text = splitter2[0];
                }

                components.Add(val);
            }

            dataReport.components = components;

            var user = _context.Users.Where(x => x.Id == model.CreatedUserId).FirstOrDefault();
            dataReport.user = user.Name;

            var created_at = "";
            if (model.CreatedAt.Value != null)
            {
                created_at = model.CreatedAt.Value.ToString("dd MMM yyy");
            }
            else
            {
                created_at = DateTime.Now.ToString("dd MMM yyy");
            }
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = string.Format("{0} {1}.xlsx", dataReport.job_id, created_at);
            string fileImageLogo = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "logo.png");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet ws = workbook.Worksheets.Add("Worksheet");

                    string last_column = "L";

                    ws.Cell("A1").Value = "WIPPS REPORT";
                    ws.Range(string.Format("A1:{0}1", last_column)).Row(1).Merge();
                    ws.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Cell("A1").Style.Font.FontSize = 30;
                    ws.Cell("A1").Style.Font.Bold = true;

                    string fileImageIdentitas = "";
                    string nameIdentitas = "";

                    if (user.RefineryId == 1)
                    {
                        nameIdentitas = "RU IV";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_iv.png");
                    }
                    else if (user.RefineryId == 2)
                    {
                        nameIdentitas = "RU VI";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_vi.png");
                    }
                    else if (user.RefineryId == 3)
                    {
                        nameIdentitas = "RU V";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_v.png");
                    }
                    else if (user.RefineryId == 4)
                    {
                        nameIdentitas = "RU II";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_ii.png");
                    }
                    else if (user.RefineryId == 5)
                    {
                        nameIdentitas = "RU III";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_iii.png");
                    }
                    else if (user.RefineryId == 6)
                    {
                        nameIdentitas = "RU II";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_ii.png");
                    }
                    else if (user.RefineryId == 7)
                    {
                        nameIdentitas = "RU VII";
                        fileImageIdentitas = Path.Combine(_hostingEnvironment.WebRootPath, "assets\\image", "pertamina_vii.png");
                    }

                    var imageIdentitas = ws.AddPicture(fileImageIdentitas)
                        .MoveTo(ws.Cell("A3"))
                        .Scale(0.19);
                    imageIdentitas.Name = nameIdentitas;

                    var imageLogo = ws.AddPicture(fileImageLogo)
                        .MoveTo(ws.Cell("H3"))
                        .Scale(0.09);
                    imageLogo.Name = "WIPPS Logo";

                    var dataAwal = ws.Range(string.Format("A11:{0}11", last_column)).Row(1).Merge();
                    dataAwal.Value = "Data Awal";
                    dataAwal.Style.Font.Bold = true;
                    ws.Cell("A12").Value = "Area";
                    ws.Cell("B12").Value = ":";
                    var cArea = ws.Range(string.Format("C12:{0}12", last_column)).Row(1).Merge();
                    cArea.Value = dataReport.area_id;

                    ws.Cell("A13").Value = "Pekerjaan";
                    ws.Cell("B13").Value = ":";
                    var cJob = ws.Range(string.Format("C13:{0}13", last_column)).Row(1).Merge();
                    cJob.Value = dataReport.job_id;

                    ws.Cell("A14").Value = "Judul";
                    ws.Cell("B14").Value = ":";
                    var cFormType = ws.Range(string.Format("C14:{0}14", last_column)).Row(1).Merge();
                    cFormType.Value = dataReport.form_type_id;

                    ws.Cell("A15").Value = "Pengguna";
                    ws.Cell("B15").Value = ":";
                    var cUser = ws.Range(string.Format("C15:{0}15", last_column)).Row(1).Merge();
                    cUser.Value = dataReport.user;

                    ws.Range(string.Format("A16:{0}16", last_column)).Row(1).Merge();
                    var dataCompany = ws.Range(string.Format("A17:{0}17", last_column)).Row(1).Merge();
                    dataCompany.Value = "Pelaksana Pekerjaan ";
                    dataCompany.Style.Font.Bold = true;

                    ws.Cell("A18").Value = "Pelaksana";
                    ws.Cell("B18").Value = ":";
                    var cCompany = ws.Range(string.Format("C18:{0}18", last_column)).Row(1).Merge();
                    cCompany.Value = dataReport.companies.Name;

                    ws.Range(string.Format("A19:{0}19", last_column)).Row(1).Merge();
                    var dataComponent = ws.Range(string.Format("A20:{0}20", last_column)).Row(1).Merge();
                    dataComponent.Value = "Isi Formulir  ";
                    dataComponent.Style.Font.Bold = true;

                    DataTable table = new DataTable();
                    table.Columns.Add("Komponen", typeof(string));
                    table.Columns.Add("Catatan", typeof(string));
                    table.Columns.Add("Gambar", typeof(string));
                    table.Columns.Add("Status", typeof(string));

                    table.Rows.Add("Komponen", "Catatan", "Gambar", "Status");

                    foreach (var value in dataReport.components)
                    {
                        string status = "";
                        if (value.status == 1)
                        {
                            status = "Safe";
                        }
                        else if (value.status == 2)
                        {
                            status = "Unsafe";
                        }
                        else
                        {
                            status = "N/A";
                        }
                        table.Rows.Add(value.text, value.note, "", status);
                    }

                    ws.Cell(21, 1).InsertData(table.AsEnumerable());

                    ws.Range(string.Format("A21:{0}21", last_column)).Row(1).Style.Font.Bold = true;

                    for (var i = 0; i < table.Rows.Count; i++)
                    {
                        ws.Column(2).Cell(21 + i).CopyTo(ws.Cell(string.Format("I{0}", (21 + i))));
                        ws.Column(3).Cell(21 + i).CopyTo(ws.Cell(string.Format("K{0}", (21 + i))));
                        ws.Column(4).Cell(21 + i).CopyTo(ws.Cell(string.Format("L{0}", (21 + i))));
                        ws.Range((21 + i), 1, (21 + i), 8).Row(1).Merge();
                        ws.Range((21 + i), 9, (21 + i), 10).Row(1).Merge();
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private List<Data> GetExportData(long? refinery_id, string from, string to)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"
                SELECT component_checklists.*, form_types.name as Type, form_types.slug as Slug, components.name as Name
                FROM component_checklists
                INNER JOIN forms ON forms.id = component_checklists.form_id
                INNER JOIN form_types ON form_types.id = forms.form_type_id
                INNER JOIN form_components ON form_components.id = component_checklists.form_component_id
                INNER JOIN components ON components.id = form_components.component_id
                WHERE component_checklists.status <> 3
                AND form_types.refinery_id = {0}
            ", refinery_id);

            if (from != "" && to != "")
            {
                sb.AppendFormat(@" AND component_checklists.created_at BETWEEN '{0}' AND '{1}'", from, to);
            }

            string sql = string.Format(sb.ToString());

            var areaus = _context.Set<ComponentChecklist2>()
                .FromSqlRaw(sql)
                .GroupBy(x => new { x.Type, x.Name, x.Status, x.Slug })
                .Select(cc => new ComponentChecklist2
                {
                    Total = cc.Sum(s => s.Status),
                    Name = cc.Key.Name,
                    Type = cc.Key.Type,
                    Status = cc.Key.Status,
                    Slug = cc.Key.Slug
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            List<Data> data = new List<Data>();
            foreach (ComponentChecklist2 value in areaus)
            {
                Data newstd = new Data();
                newstd.Name = value.Name;
                newstd.Slug = value.Slug;
                newstd.Type = value.Type;

                var percent = areaus.Where(x => x.Name.ToLower() == value.Name.ToLower() && x.Type.ToLower() == value.Type.ToLower()).ToList();
                double? neg = 0;
                double? pos = 0;

                foreach (var val in percent)
                {
                    if (val.Status == 1)
                    {
                        pos = val.Total;
                    }
                    else
                    {
                        neg = val.Total;
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

            return newData;
        }

        public class Components
        {
            public string note { get; set; }
            public long id { get; set; }
            public long? status { get; set; }
            public int? safe_value { get; set; }
            public int? unsafe_value { get; set; }
            public string text { get; set; }
            public List<ComponentAttachment> images { get; set; }
            public string sub { get; set; }
            public string footnote { get; set; }
        }

        public class DataComponent
        {
            public long id { get; set; }
            public string note { get; set; }
            public string text { get; set; }
            public List<DataImage> images { get; set; }
            public long status { get; set; }
            public int? people { get; set; }
            public int? total { get; set; }
        }

        public class DataImage
        {
            public int index { get; set; }
            public string file { get; set; }
        }

        public class DataReport
        {
            public string area_id { get; set; }
            public string job_id { get; set; }
            public string form_type_id { get; set; }
            public string user { get; set; }
            public Company companies { get; set; }
            public List<Components> components { get; set; }
        }
    }
}
