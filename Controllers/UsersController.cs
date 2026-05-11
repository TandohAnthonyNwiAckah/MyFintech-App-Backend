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
    /// Controller for Users api
    /// </summary>
    public class UsersController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly IHttpContextAccessor HttpAccessor;
        private readonly AppDBContext DB;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public UsersController(
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
        /// List  Users records
        /// Support searching, filtering and ordering of table records
        /// </summary>
        /// <param name="fieldname">Filter records based on table field.</param>
        /// <param name="fieldvalue">Filter value.</param>
        /// <returns>JSON Array of Users records</returns>
        [HttpGet]
        [Route("api/users/index")]
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
                    from Users in DB.Users
                    select new Users
                    {
                        id = Users.id,
                        title = Users.title,
                        firstname = Users.firstname,
                        surname = Users.surname,
                        phone = Users.phone,
                        email = Users.email,
                        photo = Users.photo,
                        dob = Users.dob,
                        gender = Users.gender,
                        country = Users.country,
                        date_created = Users.date_created,
                    };
                if (search != null)
                {
                    query = query.Where(p =>
                        EF.Functions.Like(p.id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.title, $"%{search}%")
                        || EF.Functions.Like(p.firstname, $"%{search}%")
                        || EF.Functions.Like(p.surname, $"%{search}%")
                        || EF.Functions.Like(p.phone, $"%{search}%")
                        || EF.Functions.Like(p.email, $"%{search}%")
                        || EF.Functions.Like(p.dob, $"%{search}%")
                        || EF.Functions.Like(p.gender, $"%{search}%")
                        || EF.Functions.Like(p.country, $"%{search}%")
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
        /// Import csv file data into Users
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
                            var records = csvReader.GetRecords<Users>().ToList();

                            DB.Users.AddRange(records);
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
        /// Select single Users record by ID
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpGet]
        [Route("api/users/view/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var query =
                    from Users in DB.Users
                    select new Users
                    {
                        id = Users.id,
                        title = Users.title,
                        firstname = Users.firstname,
                        middlename = Users.middlename,
                        surname = Users.surname,
                        maiden_name = Users.maiden_name,
                        phone = Users.phone,
                        email = Users.email,
                        dob = Users.dob,
                        gender = Users.gender,
                        digital_address = Users.digital_address,
                        residential_status = Users.residential_status,
                        country = Users.country,
                        user_role_id = Users.user_role_id,
                        date_created = Users.date_created,
                        date_updated = Users.date_updated,
                        photo = Users.photo,
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

                record.nextRecordId = getNextRecordId(id);
                record.previousRecordId = getPreviousRecordId(id);

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
        /// <returns>JSON Object of newly created Users record</returns>
        [HttpPost]
        [Route("api/users/add")]
        public async Task<IActionResult> Add([FromBody] UsersAddDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var modeldata = new Users();
                modeldata.title = postdata.title;
                modeldata.photo = postdata.photo;
                modeldata.firstname = postdata.firstname;
                modeldata.middlename = postdata.middlename;
                modeldata.surname = postdata.surname;
                modeldata.maiden_name = postdata.maiden_name;
                modeldata.phone = postdata.phone;
                modeldata.email = postdata.email;
                modeldata.password = postdata.password;
                modeldata.dob = postdata.dob;
                modeldata.gender = postdata.gender;
                modeldata.digital_address = postdata.digital_address;
                modeldata.residential_status = postdata.residential_status;
                modeldata.country = postdata.country;
                modeldata.user_role_id = postdata.user_role_id;
                modeldata.date_created = DateTime.Now;
                modeldata.date_updated = DateTime.Now;

                // move uploaded files from temporary directory
                if (modeldata.photo != null)
                {
                    var fileInfo = await this.MoveUploadedFiles(modeldata.photo, "photo");
                    modeldata.photo = fileInfo.filepath;
                }
                modeldata.password = Hash.ComputeHash(postdata.password);

                // assign default role to user
                var roleId = DB
                    .Roles.Where(p => p.role_name == "Admin")
                    .Select(p => p.role_id)
                    .FirstOrDefault();
                modeldata.user_role_id = roleId;
                // save Users record
                DB.Users.Add(modeldata);
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
        /// Get  Users record for edit
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpGet]
        [Route("api/users/edit/{id}")]
        public ActionResult Edit(int id)
        {
            try
            {
                var query =
                    from Users in DB.Users
                    select new Users
                    {
                        title = Users.title,
                        photo = Users.photo,
                        firstname = Users.firstname,
                        middlename = Users.middlename,
                        surname = Users.surname,
                        maiden_name = Users.maiden_name,
                        phone = Users.phone,
                        email = Users.email,
                        dob = Users.dob,
                        gender = Users.gender,
                        digital_address = Users.digital_address,
                        residential_status = Users.residential_status,
                        country = Users.country,
                        user_role_id = Users.user_role_id,
                        id = Users.id,
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
        /// Update  Users record with form data
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpPost]
        [Route("api/users/edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UsersEditDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var query = from Users in DB.Users select Users;
                if (DB.Users.Any(p => p.phone.Equals(postdata.phone) && !p.id.Equals(id)))
                {
                    return BadRequest(postdata.phone + " " + " Already exist!");
                }
                if (DB.Users.Any(p => p.email.Equals(postdata.email) && !p.id.Equals(id)))
                {
                    return BadRequest(postdata.email + " " + " Already exist!");
                }
                query = query.Where(p => p.id.Equals(id));
                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                var modeldata = record;
                modeldata.title = postdata.title;
                modeldata.photo = postdata.photo;
                modeldata.firstname = postdata.firstname;
                modeldata.middlename = postdata.middlename;
                modeldata.surname = postdata.surname;
                modeldata.maiden_name = postdata.maiden_name;
                modeldata.phone = postdata.phone;
                modeldata.email = postdata.email;
                modeldata.dob = postdata.dob;
                modeldata.gender = postdata.gender;
                modeldata.digital_address = postdata.digital_address;
                modeldata.residential_status = postdata.residential_status;
                modeldata.country = postdata.country;
                modeldata.user_role_id = postdata.user_role_id;
                modeldata.date_updated = DateTime.Now;

                // move uploaded files from temporary directory
                if (modeldata.photo != null)
                {
                    var fileInfo = await this.MoveUploadedFiles(modeldata.photo, "photo");
                    modeldata.photo = fileInfo.filepath;
                }
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
        /// Soft Delete Users record
        /// Support multi delete by separating record id by comma.
        /// </summary>
        /// <param name="id">Table primary key.</param>
        /// <returns>Record ID </returns>
        [HttpGet]
        [Route("api/users/delete/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                List<string> arrId = id.Split(",").ToList();
                var query = DB.Users.AsQueryable();
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
        /// Export  Users records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        [Route("api/users/exportlist")]
        private async Task<IActionResult> ExportListRecords(List<Users> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportList"; // html template for pdf

            try
            {
                var fileName = "UsersReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "Title", Key = "title" },
                        new DataHeader { Header = "Firstname", Key = "firstname" },
                        new DataHeader { Header = "Middlename", Key = "middlename" },
                        new DataHeader { Header = "Surname", Key = "surname" },
                        new DataHeader { Header = "Maiden Name", Key = "maiden_name" },
                        new DataHeader { Header = "Phone", Key = "phone" },
                        new DataHeader { Header = "Email", Key = "email" },
                        new DataHeader { Header = "Photo", Key = "photo" },
                        new DataHeader { Header = "Dob", Key = "dob" },
                        new DataHeader { Header = "Gender", Key = "gender" },
                        new DataHeader { Header = "Digital Address", Key = "digital_address" },
                        new DataHeader
                        {
                            Header = "Residential Status",
                            Key = "residential_status",
                        },
                        new DataHeader { Header = "Country", Key = "country" },
                        new DataHeader { Header = "User Role Id", Key = "user_role_id" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Users"; // Excel worksheet title
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
        /// Export  Users records to different format such as: pdf, cvs, excel, html
        /// </summary>
        /// <returns>PDF | CSV | Excel | HTML File</returns>
        [HttpGet]
        [Route("api/users/exportview")]
        private async Task<IActionResult> ExportViewRecords(List<Users> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportView"; // html template for pdf

            try
            {
                var fileName = "UsersReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "Title", Key = "title" },
                        new DataHeader { Header = "Firstname", Key = "firstname" },
                        new DataHeader { Header = "Middlename", Key = "middlename" },
                        new DataHeader { Header = "Surname", Key = "surname" },
                        new DataHeader { Header = "Maiden Name", Key = "maiden_name" },
                        new DataHeader { Header = "Phone", Key = "phone" },
                        new DataHeader { Header = "Email", Key = "email" },
                        new DataHeader { Header = "Dob", Key = "dob" },
                        new DataHeader { Header = "Gender", Key = "gender" },
                        new DataHeader { Header = "Digital Address", Key = "digital_address" },
                        new DataHeader
                        {
                            Header = "Residential Status",
                            Key = "residential_status",
                        },
                        new DataHeader { Header = "Country", Key = "country" },
                        new DataHeader { Header = "User Role Id", Key = "user_role_id" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Users"; // Excel worksheet title
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

        private string getNextRecordId(int recid)
        {
            try
            {
                var query = DB.Users.AsQueryable();
                query = query.Where(p => p.id > recid);
                query = query.OrderByField("id", true);
                var record = query.FirstOrDefault();
                if (record != null)
                {
                    return record.id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return string.Empty;
        }

        private string getPreviousRecordId(int recid)
        {
            try
            {
                var query = DB.Users.AsQueryable();
                query = query.Where(p => p.id < recid);
                query = query.OrderByField("id", false);
                var record = query.FirstOrDefault();
                if (record != null)
                {
                    return record.id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return string.Empty;
        }
    }
}
