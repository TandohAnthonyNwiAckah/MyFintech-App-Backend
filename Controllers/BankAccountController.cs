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
    /// Controller for Bankaccount api
    /// </summary>
    public class BankAccountController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly IHttpContextAccessor HttpAccessor;
        private readonly AppDBContext DB;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public BankAccountController(
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
        /// List  BankAccount records
        /// Support searching, filtering and ordering of table records
        /// </summary>
        /// <param name="fieldname">Filter records based on table field.</param>
        /// <param name="fieldvalue">Filter value.</param>
        /// <returns>JSON Array of BankAccount records</returns>
        [HttpGet]
        [Route("api/bankaccount/index")]
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
                    from BankAccount in DB.BankAccount
                    select new BankAccount
                    {
                        id = BankAccount.id,
                        user_id = BankAccount.user_id,
                        bank_name = BankAccount.bank_name,
                        account_name = BankAccount.account_name,
                        fund_name = BankAccount.fund_name,
                        debit_amount = BankAccount.debit_amount,
                        date_transaction = BankAccount.date_transaction,
                        frequency_deduction = BankAccount.frequency_deduction,
                        end_date = BankAccount.end_date,
                        date_created = BankAccount.date_created,
                        date_updated = BankAccount.date_updated,
                    };
                if (search != null)
                {
                    query = query.Where(p =>
                        EF.Functions.Like(p.id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.user_id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.bank_name, $"%{search}%")
                        || EF.Functions.Like(p.account_name, $"%{search}%")
                        || EF.Functions.Like(p.fund_name, $"%{search}%")
                        || EF.Functions.Like(p.debit_amount, $"%{search}%")
                        || EF.Functions.Like(p.date_transaction, $"%{search}%")
                        || EF.Functions.Like(p.frequency_deduction, $"%{search}%")
                        || EF.Functions.Like(p.end_date, $"%{search}%")
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
        /// Import csv file data into BankAccount
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
                            var records = csvReader.GetRecords<BankAccount>().ToList();

                            DB.BankAccount.AddRange(records);
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
        /// Select single BankAccount record by ID
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>JSON Object of BankAccount record</returns>
        [HttpGet]
        [Route("api/bankaccount/view/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var query =
                    from BankAccount in DB.BankAccount
                    select new BankAccount
                    {
                        id = BankAccount.id,
                        user_id = BankAccount.user_id,
                        bank_name = BankAccount.bank_name,
                        account_name = BankAccount.account_name,
                        fund_name = BankAccount.fund_name,
                        debit_amount = BankAccount.debit_amount,
                        date_transaction = BankAccount.date_transaction,
                        frequency_deduction = BankAccount.frequency_deduction,
                        end_date = BankAccount.end_date,
                        date_created = BankAccount.date_created,
                        date_updated = BankAccount.date_updated,
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
        /// <returns>JSON Object of newly created BankAccount record</returns>
        [HttpPost]
        [Route("api/bankaccount/add")]
        public ActionResult Add([FromBody] BankaccountAddDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var modeldata = new BankAccount();
                modeldata.user_id = postdata.user_id;
                modeldata.bank_name = postdata.bank_name;
                modeldata.account_name = postdata.account_name;
                modeldata.fund_name = postdata.fund_name;
                modeldata.debit_amount = postdata.debit_amount;
                modeldata.frequency_deduction = postdata.frequency_deduction;
                modeldata.date_transaction = postdata.date_transaction;
                modeldata.end_date = postdata.end_date;
                modeldata.date_created = DateTime.Now;
                modeldata.date_updated = DateTime.Now;
                // save BankAccount record
                DB.BankAccount.Add(modeldata);
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
        /// Get  BankAccount record for edit
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of BankAccount record</returns>
        [HttpGet]
        [Route("api/bankaccount/edit/{id}")]
        public ActionResult Edit(int id)
        {
            try
            {
                var query =
                    from BankAccount in DB.BankAccount
                    select new BankAccount
                    {
                        user_id = BankAccount.user_id,
                        bank_name = BankAccount.bank_name,
                        account_name = BankAccount.account_name,
                        fund_name = BankAccount.fund_name,
                        debit_amount = BankAccount.debit_amount,
                        frequency_deduction = BankAccount.frequency_deduction,
                        date_transaction = BankAccount.date_transaction,
                        end_date = BankAccount.end_date,
                        id = BankAccount.id,
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
        /// Update  BankAccount record with form data
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of BankAccount record</returns>
        [HttpPost]
        [Route("api/bankaccount/edit/{id}")]
        public ActionResult Edit(int id, [FromBody] BankaccountEditDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var query = from BankAccount in DB.BankAccount select BankAccount;
                query = query.Where(p => p.id.Equals(id));
                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                var modeldata = record;
                modeldata.user_id = postdata.user_id;
                modeldata.bank_name = postdata.bank_name;
                modeldata.account_name = postdata.account_name;
                modeldata.fund_name = postdata.fund_name;
                modeldata.debit_amount = postdata.debit_amount;
                modeldata.frequency_deduction = postdata.frequency_deduction;
                modeldata.date_transaction = postdata.date_transaction;
                modeldata.end_date = postdata.end_date;
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
        /// Soft Delete BankAccount record
        /// Support multi delete by separating record id by comma.
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>Record ID </returns>
        [HttpGet]
        [Route("api/bankaccount/delete/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                List<string> arrId = id.Split(",").ToList();
                var query = DB.BankAccount.AsQueryable();
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
        /// Export  BankAccount records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportListRecords(List<BankAccount> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportList"; // html template for pdf

            try
            {
                var fileName = "BankAccountReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "Bank Name", Key = "bank_name" },
                        new DataHeader { Header = "Account Name", Key = "account_name" },
                        new DataHeader { Header = "Fund Name", Key = "fund_name" },
                        new DataHeader { Header = "Debit Amount", Key = "debit_amount" },
                        new DataHeader { Header = "Date Transaction", Key = "date_transaction" },
                        new DataHeader
                        {
                            Header = "Frequency Deduction",
                            Key = "frequency_deduction",
                        },
                        new DataHeader { Header = "End Date", Key = "end_date" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "BankAccount"; // Excel worksheet title
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
        /// Export  BankAccount records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        private async Task<IActionResult> ExportViewRecords(List<BankAccount> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportView"; // html template for pdf

            try
            {
                var fileName = "BankAccountReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "User Id", Key = "user_id" },
                        new DataHeader { Header = "Bank Name", Key = "bank_name" },
                        new DataHeader { Header = "Account Name", Key = "account_name" },
                        new DataHeader { Header = "Fund Name", Key = "fund_name" },
                        new DataHeader { Header = "Debit Amount", Key = "debit_amount" },
                        new DataHeader { Header = "Date Transaction", Key = "date_transaction" },
                        new DataHeader
                        {
                            Header = "Frequency Deduction",
                            Key = "frequency_deduction",
                        },
                        new DataHeader { Header = "End Date", Key = "end_date" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "BankAccount"; // Excel worksheet title
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
