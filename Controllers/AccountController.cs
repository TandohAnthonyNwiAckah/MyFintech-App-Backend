#nullable enable
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
namespace TANA.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using TANA.Data;
    using TANA.Helpers;
    using TANA.Models;

    /// <summary>
    /// User Account Controller
    /// </summary>
    public class AccountController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly AppDBContext DB;
        private IQueryable<Users> Query;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public AccountController(
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
            DB = dbContext;
            Query = DB.Users;
            Mapper = mapper;
            hostEnvironment = environment;
            rbac = _rbac;
        }

        /// <summary>
        /// Route to view  user account detail
        /// </summary>
        /// <returns>JSON Object of Users record</returns>
        [HttpGet]
        [Route("api/account")]
        public ActionResult Index()
        {
            var id = CurrentUser.id;
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
                    };
                query = query.Where(p => p.id.Equals(id)); // filter by current user id
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
        /// Get  user account record for edit
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpGet]
        [Route("api/account/edit")]
        public ActionResult Edit()
        {
            try
            {
                var id = CurrentUser.id;
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
                        photo = Users.photo,
                        dob = Users.dob,
                        gender = Users.gender,
                        digital_address = Users.digital_address,
                        residential_status = Users.residential_status,
                        country = Users.country,
                        user_role_id = Users.user_role_id,
                    };
                query = query.Where(p => p.id.Equals(id)); // filter by current user id
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
        /// Update  user account record with form data
        /// </summary>
        /// <param name="id">Select record by table primary key.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpPost]
        [Route("api/account/edit")]
        public async Task<IActionResult> Edit([FromBody] UsersAccounteditDTO postdata)
        {
            var id = CurrentUser.id;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var query = from Users in DB.Users select Users;
                query = query.Where(p => p.id.Equals(id)); // filter by current user id
                var record = query.FirstOrDefault();

                if (record == null)
                {
                    return NotFound("No record found");
                }

                // map and update current record with form data
                var modeldata = record;
                modeldata.title = postdata.title;
                modeldata.firstname = postdata.firstname;
                modeldata.middlename = postdata.middlename;
                modeldata.surname = postdata.surname;
                modeldata.maiden_name = postdata.maiden_name;
                modeldata.phone = postdata.phone;
                modeldata.photo = postdata.photo;
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
                DB.SaveChanges();
                return Ok(record);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/account/currentuserdata")]
        public ActionResult CurrentUserData()
        {
            try
            {
                var userPages = new List<string>();
                var roleNames = new List<string>();
                if (CurrentUser != null)
                {
                    var userRole = CurrentUser.user_role_id;
                    userPages = rbac.GetRolePages(userRole);
                    roleNames = rbac.GetRoleNames(userRole);
                    CurrentUser.password = "";
                    var data = new
                    {
                        user = CurrentUser,
                        pages = userPages,
                        roles = roleNames,
                    };
                    return Ok(data);
                }
                return NotFound("User not found");
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Change user account password
        /// </summary>
        /// <param name="modeldata">Request form data.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpPost]
        [Route("api/account/changepassword")]
        public ActionResult ChangePassword([FromBody] ChangePassword modeldata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Query = Query.Where(p => p.id.Equals(CurrentUser.id));
                var user = Query.FirstOrDefault();
                if (user != null)
                {
                    if (!Hash.Verify(user.password, modeldata.OldPassword))
                    {
                        return BadRequest("Current password is incorrect");
                    }
                    user.password = Hash.ComputeHash(modeldata.NewPassword);
                    ;
                    DB.Update(user);
                    DB.SaveChanges();
                    return Ok();
                }
                else
                {
                    return NotFound("No record found");
                }
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }
    }
}
