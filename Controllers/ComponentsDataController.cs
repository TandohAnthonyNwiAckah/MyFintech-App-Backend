#nullable enable
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
namespace TANA.Controller
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    /// <summary>
    /// Components Data Controller
    /// </summary>
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using TANA.Data;
    using TANA.Helpers;
    using TANA.Models;

    [AllowAnonymous]
    public class ComponentsData : BaseController
    {
        private readonly IConfiguration Config;
        private readonly AppDBContext DB;
        private readonly IWebHostEnvironment hostEnvironment;

        public ComponentsData(
            AppDBContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration Configuration
        )
            : base(dbContext, httpContextAccessor, environment, Configuration)
        {
            DB = dbContext;
            Config = Configuration;
            hostEnvironment = environment;
        }

        /// <summary>
        /// Get user_id_option_list records
        /// </summary>
        /// <returns>Array of option labels and value object</returns>
        [HttpGet]
        [Route("api/components_data/user_id_option_list")]
        public ActionResult user_id_option_list()
        {
            string sqlText = "SELECT  DISTINCT id AS value,surname AS label FROM users ORDER BY id";
            var queryParams = new List<QueryParam>();
            var records = DB.RawSqlQuery(
                sqlText,
                queryParams,
                record => new { value = record["value"], label = record["label"] }
            );
            return Ok(records);
        }

        /// <summary>
        /// Get user_id_option_list_2 records
        /// </summary>
        /// <returns>Array of option labels and value object</returns>
        [HttpGet]
        [Route("api/components_data/user_id_option_list_2")]
        public ActionResult user_id_option_list_2()
        {
            string sqlText = "SELECT  DISTINCT id AS value,phone AS label FROM users ORDER BY id";
            var queryParams = new List<QueryParam>();
            var records = DB.RawSqlQuery(
                sqlText,
                queryParams,
                record => new { value = record["value"], label = record["label"] }
            );
            return Ok(records);
        }

        /// <summary>
        /// Get fund_name_option_list records
        /// </summary>
        /// <returns>Array of option labels and value object</returns>
        [HttpGet]
        [Route("api/components_data/fund_name_option_list")]
        public ActionResult fund_name_option_list()
        {
            string sqlText =
                "SELECT  DISTINCT id AS value,name AS label FROM product_name ORDER BY id";
            var queryParams = new List<QueryParam>();
            var records = DB.RawSqlQuery(
                sqlText,
                queryParams,
                record => new { value = record["value"], label = record["label"] }
            );
            return Ok(records);
        }

        /// <summary>
        /// Get investment_type_option_list records
        /// </summary>
        /// <returns>Array of option labels and value object</returns>
        [HttpGet]
        [Route("api/components_data/investment_type_option_list")]
        public ActionResult investment_type_option_list()
        {
            string sqlText =
                "SELECT  DISTINCT id AS value,name AS label FROM investment_name ORDER BY id";
            var queryParams = new List<QueryParam>();
            var records = DB.RawSqlQuery(
                sqlText,
                queryParams,
                record => new { value = record["value"], label = record["label"] }
            );
            return Ok(records);
        }

        /// <summary>
        /// Get role_id_option_list records
        /// </summary>
        /// <returns>Array of option labels and value object</returns>
        [HttpGet]
        [Route("api/components_data/role_id_option_list")]
        public ActionResult role_id_option_list()
        {
            string sqlText = "SELECT role_id as value, role_name as label FROM roles";
            var queryParams = new List<QueryParam>();
            var records = DB.RawSqlQuery(
                sqlText,
                queryParams,
                record => new { value = record["value"], label = record["label"] }
            );
            return Ok(records);
        }

        /// <summary>
        /// check if field value already exist in a Users table
        /// </summary>
        /// <param name="id">value to check if exists in a table records.</param>
        /// <returns>True or False</returns>
        [HttpGet]
        [Route("api/components_data/users_surname_exist/{id}")]
        public ActionResult users_surname_exist(string id = "")
        {
            bool exist = DB.Users.Any(p => p.surname == id);
            if (exist == true)
            {
                return Ok("true");
            }
            return Ok("false");
        }

        /// <summary>
        /// check if field value already exist in a Users table
        /// </summary>
        /// <param name="id">value to check if exists in a table records.</param>
        /// <returns>True or False</returns>
        [HttpGet]
        [Route("api/components_data/users_email_exist/{id}")]
        public ActionResult users_email_exist(string id = "")
        {
            bool exist = DB.Users.Any(p => p.email == id);
            if (exist == true)
            {
                return Ok("true");
            }
            return Ok("false");
        }

        /// <summary>
        /// check if field value already exist in a Users table
        /// </summary>
        /// <param name="id">value to check if exists in a table records.</param>
        /// <returns>True or False</returns>
        [HttpGet]
        [Route("api/components_data/users_phone_exist/{id}")]
        public ActionResult users_phone_exist(string id = "")
        {
            bool exist = DB.Users.Any(p => p.phone == id);
            if (exist == true)
            {
                return Ok("true");
            }
            return Ok("false");
        }

        /// <summary>
        /// Get getcount_users record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_users")]
        public ActionResult getcount_users()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM users";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }

        /// <summary>
        /// Get getcount_customers record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_customers")]
        public ActionResult getcount_customers()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM customers";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }

        /// <summary>
        /// Get getcount_investments record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_investments")]
        public ActionResult getcount_investments()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM investments";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }

        /// <summary>
        /// Get getcount_bankaccount record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_bankaccount")]
        public ActionResult getcount_bankaccount()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM bank_account";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }

        /// <summary>
        /// Get getcount_investmentname record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_investmentname")]
        public ActionResult getcount_investmentname()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM investment_name";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }

        /// <summary>
        /// Get getcount_productname record
        /// </summary>
        /// <returns>Single value</returns>
        [HttpGet]
        [Route("api/components_data/getcount_productname")]
        public ActionResult getcount_productname()
        {
            string sqlText = "SELECT COUNT(*) AS num FROM product_name";
            var queryParams = new List<QueryParam>();
            var value = DB.RawSqlQueryScalar(sqlText, queryParams);
            return Ok(value);
        }
    }
}
