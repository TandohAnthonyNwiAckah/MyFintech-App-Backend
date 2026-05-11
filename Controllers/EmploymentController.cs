#nullable enable
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
namespace TANA.Controller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using CsvHelper;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using TANA.Data;
    using TANA.Helpers;
    using TANA.Models;
    using WkWrap.Core;

    /// <summary>
    /// Controller for Employment api
    /// </summary>
    public class EmploymentController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly IHttpContextAccessor HttpAccessor;
        private readonly AppDBContext DB;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public EmploymentController(
            AppDBContext dbContext,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration Configuration,
            EmailHelper mailer,
            Rbac _rbac
        )
            : base(dbContext, httpContextAccessor, environment, Configuration)
        {
            Config = Configuration;
            Mailer = mailer;
            HttpAccessor = httpContextAccessor;
            DB = dbContext;
            Mapper = mapper;
            hostEnvironment = environment;
            rbac = _rbac;
        }

        /// <summary>
        /// List  Employment records
        /// Support searching, filtering and ordering of table records
        /// </summary>
        /// <param name="fieldname">Filter records based on table field.</param>
        /// <param name="fieldvalue">Filter value.</param>
        /// <returns>JSON Array of Employment records</returns>
        [HttpGet]
        [Route("api/employment/index")]
        public async Task<IActionResult> Index(
            string? fieldname,
            string? fieldvalue,
            string? search,
            string? orderby,
            string ordertype = "desc",
            int page = 1,
            int limit = 10
        )
        {
            try
            {
                var query =
                    from Employment in DB.Employment
                    select new Employment
                    {
                        id = Employment.id,
                        user_id = Employment.user_id,
                        name_employer = Employment.name_employer,
                        years_employ = Employment.years_employ,
                        address = Employment.address,
                        nature_business = Employment.nature_business,
                        location = Employment.location,
                        income_range = Employment.income_range,
                        date_created = Employment.date_created,
                        date_updated = Employment.date_updated,
                    };
                if (search != null)
                {
                    query = query.Where(p =>
                        EF.Functions.Like(p.id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.name_employer, $"%{search}%")
                        || EF.Functions.Like(p.years_employ, $"%{search}%")
                        || EF.Functions.Like(p.address, $"%{search}%")
                        || EF.Functions.Like(p.nature_business, $"%{search}%")
                        || EF.Functions.Like(p.location, $"%{search}%")
                        || EF.Functions.Like(p.income_range, $"%{search}%")
                    );
                }
                if (orderby != null)
                {
                    bool asc = ordertype.Equals("asc", StringComparison.CurrentCultureIgnoreCase);
                    query = query.OrderByField(orderby, asc);
                }
                else
                {
                    query = query.OrderByField("id", false);
                }
                if (fieldvalue != null)
                {
                    // filter record by table field
                    query = query.Where(fieldname + " == @0", fieldvalue);
                }
                // export page records
                if (Request.Query.ContainsKey("export"))
                {
                    var exportRecords = query.ToList();
                    return await ExportListRecords(exportRecords);
                }

                int totalRecords = query.Count();
                int offset = ((page - 1) * limit);
                var records = query.Skip(offset).Take(limit).ToList();
                int recordCount = records.Count;
                double t = totalRecords / limit;
                int totalPages = (int)Math.Ceiling(t);
                var result = new
                {
                    records,
                    totalRecords,
                    recordCount,
                    totalPages,
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Import csv file data into Employment
        /// </summary>
        /// <returns>Number of records imported</returns>
        public ActionResult ImportData([FromForm(Name = "file")] List<IFormFile> CsvFiles)
        {
            try
            {
                int totalRows = 0;
                foreach (IFormFile file in CsvFiles)
                {
                    if (file.Length > 0) // ensures file has readable content
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string ext = Path.GetExtension(fileName).Trim('.').ToLower();
                        if (ext != "csv")
                        {
                            return BadRequest(ext + " files not allowed");
                        }
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            TextReader textReader = new StreamReader(memoryStream);
                            var config = new CsvHelper.Configuration.CsvConfiguration(
                                System.Globalization.CultureInfo.InvariantCulture
                            )
                            {
                                HasHeaderRecord = true,
                            };

                            var csvReader = new CsvReader(textReader, config);
                            var records = csvReader.GetRecords<Employment>().ToList();

                            DB.Employment.AddRange(records);
                            DB.SaveChanges();
                            totalRows += records.Count;
                        }
                    }
                }
                return Ok(totalRows + " Records Imported");
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Select single Employment record by ID
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>JSON Object of Employment record</returns>
        [HttpGet]
        [Route("api/employment/view/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var query =
                    from Employment in DB.Employment
                    select new Employment
                    {
                        id = Employment.id,
                        user_id = Employment.user_id,
                        name_employer = Employment.name_employer,
                        years_employ = Employment.years_employ,
                        address = Employment.address,
                        nature_business = Employment.nature_business,
                        location = Employment.location,
                        income_range = Employment.income_range,
                        date_created = Employment.date_created,
                        date_updated = Employment.date_updated,
                    };
                query = query.Where(p => p.id.Equals(id));
                // export page records
                if (Request.Query.ContainsKey("export"))
                {
                    var exportRecords = query.ToList();
                    return await ExportViewRecords(exportRecords);
                }

                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                return Ok(record);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Save form record to the  table
        /// </summary>
        /// <returns>JSON Object of newly created Employment record</returns>
        [HttpPost]
        [Route("api/employment/add")]
        public ActionResult Add([FromBody] EmploymentAddDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var modeldata = new Employment();
                modeldata.user_id = postdata.user_id;
                modeldata.name_employer = postdata.name_employer;
                modeldata.years_employ = postdata.years_employ;
                modeldata.address = postdata.address;
                modeldata.nature_business = postdata.nature_business;
                modeldata.location = postdata.location;
                modeldata.income_range = postdata.income_range;
                modeldata.date_created = DateTime.Now;
                modeldata.date_updated = DateTime.Now;
                // save Employment record
                DB.Employment.Add(modeldata);
                var record = modeldata; //newly created record
                var audit = GetAuditLog();
                DB.SaveChanges(audit);
                var recId = record.id; //newly created record id
                return Ok(record);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Get  Employment record for edit
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Employment record</returns>
        [HttpGet]
        [Route("api/employment/edit/{id}")]
        public ActionResult Edit(int id)
        {
            try
            {
                var query =
                    from Employment in DB.Employment
                    select new Employment
                    {
                        id = Employment.id,
                        user_id = Employment.user_id,
                        name_employer = Employment.name_employer,
                        years_employ = Employment.years_employ,
                        address = Employment.address,
                        nature_business = Employment.nature_business,
                        location = Employment.location,
                        income_range = Employment.income_range,
                    };
                query = query.Where(p => p.id.Equals(id));
                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                return Ok(record);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Update  Employment record with form data
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Employment record</returns>
        [HttpPost]
        [Route("api/employment/edit/{id}")]
        public ActionResult Edit(int id, [FromBody] EmploymentEditDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var query = from Employment in DB.Employment select Employment;
                query = query.Where(p => p.id.Equals(id));
                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                var modeldata = record;
                modeldata.user_id = postdata.user_id;
                modeldata.name_employer = postdata.name_employer;
                modeldata.years_employ = postdata.years_employ;
                modeldata.address = postdata.address;
                modeldata.nature_business = postdata.nature_business;
                modeldata.location = postdata.location;
                modeldata.income_range = postdata.income_range;
                modeldata.date_updated = DateTime.Now;
                DB.Update(modeldata);
                var audit = GetAuditLog();
                DB.SaveChanges(audit);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Soft Delete Employment record
        /// Support multi delete by separating record id by comma.
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>Record ID </returns>
        [HttpGet]
        [Route("api/employment/delete/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                List<string> arrId = id.Split(",").ToList();
                var query = DB.Employment.AsQueryable();
                query = query.Where(p => arrId.Contains(p.id.ToString()));
                var records = query.ToList();
                foreach (var record in records)
                {
                    record.date_deleted = DateTime.Now;
                    DB.Update(record);
                }
                var audit = GetAuditLog();
                DB.SaveChanges(audit);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Export  Employment records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportListRecords(List<Employment> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportList"; // html template for pdf

            try
            {
                var fileName = "EmploymentReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "Name Employer", Key = "name_employer" },
                        new DataHeader { Header = "Years Employ", Key = "years_employ" },
                        new DataHeader { Header = "Address", Key = "address" },
                        new DataHeader { Header = "Nature Business", Key = "nature_business" },
                        new DataHeader { Header = "Location", Key = "location" },
                        new DataHeader { Header = "Income Range", Key = "income_range" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Employment"; // Excel worksheet title
                    return ExportToExcel(dataTable, fileName, exportFormat);
                }
                else if (exportFormat == "pdf")
                {
                    var htmlContent = await this.RenderViewAsync(viewName, records, true);
                    var wkhtmltoExePath = Config["WKHTML_EXE_PATH"];
                    var wkhtmltopdf = new FileInfo(wkhtmltoExePath);
                    var converter = new HtmlToPdfConverter(wkhtmltopdf);
                    var settings = new ConversionSettings(
                        pageSize: PageSize.A3,
                        orientation: PageOrientation.Portrait,
                        enableExternalLinks: true,
                        enableImages: true
                    );
                    var pdfBytes = converter.ConvertToPdf(htmlContent, Encoding.UTF8, settings);
                    return File(pdfBytes, "application/pdf", $"{fileName}.pdf");
                }
                else if (exportFormat == "print")
                {
                    return View(viewName, records);
                }
                return BadRequest("Export Format not Supported");
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Export  Employment records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportViewRecords(List<Employment> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportView"; // html template for pdf

            try
            {
                var fileName = "EmploymentReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "Name Employer", Key = "name_employer" },
                        new DataHeader { Header = "Years Employ", Key = "years_employ" },
                        new DataHeader { Header = "Address", Key = "address" },
                        new DataHeader { Header = "Nature Business", Key = "nature_business" },
                        new DataHeader { Header = "Location", Key = "location" },
                        new DataHeader { Header = "Income Range", Key = "income_range" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Employment"; // Excel worksheet title
                    return ExportToExcel(dataTable, fileName, exportFormat);
                }
                else if (exportFormat == "pdf")
                {
                    var htmlContent = await this.RenderViewAsync(viewName, records, true);
                    var wkhtmltoExePath = Config["WKHTML_EXE_PATH"];
                    var wkhtmltopdf = new FileInfo(wkhtmltoExePath);
                    var converter = new HtmlToPdfConverter(wkhtmltopdf);
                    var settings = new ConversionSettings(
                        pageSize: PageSize.A3,
                        orientation: PageOrientation.Portrait,
                        enableExternalLinks: true,
                        enableImages: true
                    );
                    var pdfBytes = converter.ConvertToPdf(htmlContent, Encoding.UTF8, settings);
                    return File(pdfBytes, "application/pdf", $"{fileName}.pdf");
                }
                else if (exportFormat == "print")
                {
                    return View(viewName, records);
                }
                return BadRequest("Export Format not Supported");
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }
    }
}
