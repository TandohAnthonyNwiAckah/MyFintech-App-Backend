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
    /// Controller for Audits api
    /// </summary>
    public class AuditsController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly IHttpContextAccessor HttpAccessor;
        private readonly AppDBContext DB;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public AuditsController(
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
        /// List  Audits records
        /// Support searching, filtering and ordering of table records
        /// </summary>
        /// <param name="fieldname">Filter records based on table field.</param>
        /// <param name="fieldvalue">Filter value.</param>
        /// <returns>JSON Array of Audits records</returns>
        [HttpGet]
        [Route("api/audits/index")]
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
                    from Audits in DB.Audits
                    select new Audits
                    {
                        log_id = Audits.log_id,
                        action = Audits.action,
                        page = Audits.page,
                        record_id = Audits.record_id,
                        user_id = Audits.user_id,
                        user_ip = Audits.user_ip,
                        user_agent = Audits.user_agent,
                        request_url = Audits.request_url,
                        old_values = Audits.old_values,
                        new_values = Audits.new_values,
                        timestamp = Audits.timestamp,
                        date_created = Audits.date_created,
                        date_updated = Audits.date_updated,
                    };
                if (search != null)
                {
                    query = query.Where(p =>
                        EF.Functions.Like(p.log_id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.action, $"%{search}%")
                        || EF.Functions.Like(p.page, $"%{search}%")
                        || EF.Functions.Like(p.record_id, $"%{search}%")
                        || EF.Functions.Like(p.user_id, $"%{search}%")
                        || EF.Functions.Like(p.user_ip, $"%{search}%")
                        || EF.Functions.Like(p.user_agent, $"%{search}%")
                        || EF.Functions.Like(p.request_url, $"%{search}%")
                        || EF.Functions.Like(p.old_values, $"%{search}%")
                        || EF.Functions.Like(p.new_values, $"%{search}%")
                    );
                }
                if (orderby != null)
                {
                    bool asc = ordertype.Equals("asc", StringComparison.CurrentCultureIgnoreCase);
                    query = query.OrderByField(orderby, asc);
                }
                else
                {
                    query = query.OrderByField("log_id", false);
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
        /// Select single Audits record by ID
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>JSON Object of Audits record</returns>
        [HttpGet]
        [Route("api/audits/view/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var query =
                    from Audits in DB.Audits
                    select new Audits
                    {
                        log_id = Audits.log_id,
                        action = Audits.action,
                        page = Audits.page,
                        record_id = Audits.record_id,
                        user_id = Audits.user_id,
                        user_ip = Audits.user_ip,
                        user_agent = Audits.user_agent,
                        request_url = Audits.request_url,
                        old_values = Audits.old_values,
                        new_values = Audits.new_values,
                        timestamp = Audits.timestamp,
                        date_created = Audits.date_created,
                        date_updated = Audits.date_updated,
                    };
                query = query.Where(p => p.log_id.Equals(id));
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
        /// Export  Audits records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportListRecords(List<Audits> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportList"; // html template for pdf

            try
            {
                var fileName = "AuditsReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Log Id", Key = "log_id" },
                        new DataHeader { Header = "Action", Key = "action" },
                        new DataHeader { Header = "Page", Key = "page" },
                        new DataHeader { Header = "Record Id", Key = "record_id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "User Ip", Key = "user_ip" },
                        new DataHeader { Header = "User Agent", Key = "user_agent" },
                        new DataHeader { Header = "Request Url", Key = "request_url" },
                        new DataHeader { Header = "Old Values", Key = "old_values" },
                        new DataHeader { Header = "New Values", Key = "new_values" },
                        new DataHeader { Header = "Timestamp", Key = "timestamp" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Audits"; // Excel worksheet title
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
        /// Export  Audits records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportViewRecords(List<Audits> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportView"; // html template for pdf

            try
            {
                var fileName = "AuditsReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Log Id", Key = "log_id" },
                        new DataHeader { Header = "Action", Key = "action" },
                        new DataHeader { Header = "Page", Key = "page" },
                        new DataHeader { Header = "Record Id", Key = "record_id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "User Ip", Key = "user_ip" },
                        new DataHeader { Header = "User Agent", Key = "user_agent" },
                        new DataHeader { Header = "Request Url", Key = "request_url" },
                        new DataHeader { Header = "Old Values", Key = "old_values" },
                        new DataHeader { Header = "New Values", Key = "new_values" },
                        new DataHeader { Header = "Timestamp", Key = "timestamp" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Audits"; // Excel worksheet title
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
