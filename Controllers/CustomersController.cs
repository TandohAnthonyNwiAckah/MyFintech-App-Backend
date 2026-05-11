#nullable enable
#pragma warning disable CS8602
#pragma warning disable CS8600
#pragma warning disable CS8604
#pragma warning disable CS8629
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
    using Microsoft.AspNetCore.Authorization;
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
    /// Controller for Customers (account opening KYC data)
    /// </summary>
    public class CustomersController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly EmailHelper Mailer;
        private readonly IHttpContextAccessor HttpAccessor;
        private readonly AppDBContext DB;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly Rbac rbac;

        public CustomersController(
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
        /// List Customers records with search, filter and ordering
        /// </summary>
        [HttpGet]
        [Route("api/customers/index")]
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
                    from c in DB.Customers
                    select new Customers
                    {
                        id = c.id,
                        firstname = c.firstname,
                        surname = c.surname,
                        phone = c.phone,
                        email = c.email,
                        marital_status = c.marital_status,
                        residential_address = c.residential_address,
                        ghana_card_number = c.ghana_card_number,
                        expiry_date = c.expiry_date,
                        place_of_issuance = c.place_of_issuance,
                        investment_objective = c.investment_objective,
                        emergency_fund = c.emergency_fund,
                        investment_knowledge = c.investment_knowledge,
                        withdrawals = c.withdrawals,
                        risk_appetite = c.risk_appetite,
                        risk_assessment = c.risk_assessment,
                        date_created = c.date_created,
                        date_updated = c.date_updated,
                        customer_no = c.customer_no,
                    };

                if (search != null)
                {
                    query = query.Where(p =>
                        EF.Functions.Like(p.id.ToString(), $"%{search}%")
                        || EF.Functions.Like(p.firstname, $"%{search}%")
                        || EF.Functions.Like(p.surname, $"%{search}%")
                        || EF.Functions.Like(p.email, $"%{search}%")
                        || EF.Functions.Like(p.phone, $"%{search}%")
                        || EF.Functions.Like(p.customer_no, $"%{search}%")
                        || EF.Functions.Like(p.ghana_card_number, $"%{search}%")
                        || EF.Functions.Like(p.marital_status, $"%{search}%")
                        || EF.Functions.Like(p.investment_objective, $"%{search}%")
                        || EF.Functions.Like(p.risk_assessment, $"%{search}%")
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
                    query = query.Where(fieldname + " == @0", fieldvalue);
                }

                if (Request.Query.ContainsKey("export"))
                {
                    var exportRecords = query.ToList();
                    return await ExportListRecords(exportRecords);
                }

                int totalRecords = query.Count();
                int offset = ((page - 1) * limit);
                var records = query.Skip(offset).Take(limit).ToList();
                int recordCount = records.Count;
                double t = (double)totalRecords / limit;
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
        /// Search Customers records by Phone or Email
        /// </summary>
        [HttpGet]
        [Route("api/customers/searchusername/{param}")]
        public async Task<IActionResult> SearchUsername(string param)
        {
            try
            {
                var query =
                    from c in DB.Customers
                    where c.phone == param || c.email == param
                    select new
                    {
                        id = c.id,
                        firstname = c.firstname,
                        surname = c.surname,
                        phone = c.phone,
                        email = c.email,
                        investment_objective = c.investment_objective,
                        emergency_fund = c.emergency_fund,
                        account_number = c.account_number,
                        fund_name = c.fund_name,
                        debit_amount = c.debit_amount,
                        customer_no = c.customer_no,
                    };

                var records = await query.ToListAsync();

                if (records.Count > 0)
                {
                    return Ok(
                        new
                        {
                            success = true,
                            code = 200,
                            message = "Customers found successfully.",
                            data = records,
                        }
                    );
                }
                else
                {
                    return NotFound(
                        new
                        {
                            success = false,
                            code = 404,
                            message = "No customer records found.",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Select single Customers record by ID
        /// </summary>
        [HttpGet]
        [Route("api/customers/view/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var query =
                    from c in DB.Customers
                    where c.id == id
                    select new Customers
                    {
                        id = c.id,
                        title = c.title,
                        firstname = c.firstname,
                        middlename = c.middlename,
                        surname = c.surname,
                        maiden_name = c.maiden_name,
                        phone = c.phone,
                        email = c.email,
                        dob = c.dob,
                        gender = c.gender,
                        digital_address = c.digital_address,
                        residential_status = c.residential_status,
                        country = c.country,
                        marital_status = c.marital_status,
                        residential_address = c.residential_address,
                        ghana_card_number = c.ghana_card_number,
                        expiry_date = c.expiry_date,
                        place_of_issuance = c.place_of_issuance,
                        selfie = c.selfie,
                        id_card_front = c.id_card_front,
                        id_card_back = c.id_card_back,
                        investment_objective = c.investment_objective,
                        emergency_fund = c.emergency_fund,
                        investment_knowledge = c.investment_knowledge,
                        withdrawals = c.withdrawals,
                        risk_appetite = c.risk_appetite,
                        risk_assessment = c.risk_assessment,
                        source_of_funds = c.source_of_funds,
                        anticipated_activity = c.anticipated_activity,
                        deposit_frequency = c.deposit_frequency,
                        withdrawal_frequency = c.withdrawal_frequency,
                        regular_topup_amount = c.regular_topup_amount,
                        regular_withdrawal_amount = c.regular_withdrawal_amount,
                        employment_status = c.employment_status,
                        years_of_employment = c.years_of_employment,
                        employer_name = c.employer_name,
                        business_address = c.business_address,
                        nature_of_business = c.nature_of_business,
                        city_town = c.city_town,
                        monthly_income_range = c.monthly_income_range,
                        bank_name = c.bank_name,
                        account_name = c.account_name,
                        bank_branch = c.bank_branch,
                        account_number = c.account_number,
                        fund_name = c.fund_name,
                        debit_amount = c.debit_amount,
                        date_transaction = c.date_transaction,
                        frequency_deduction = c.frequency_deduction,
                        end_date = c.end_date,
                        head_of_state_in_gh = c.head_of_state_in_gh,
                        head_of_state_in_gh_name = c.head_of_state_in_gh_name,
                        head_of_state_out_gh = c.head_of_state_out_gh,
                        head_of_state_out_gh_name = c.head_of_state_out_gh_name,
                        signature_path = c.signature_path,
                        selfie_path = c.selfie_path,
                        front_path = c.front_path,
                        back_path = c.back_path,
                        add_beneficiary = c.add_beneficiary,
                        num_beneficiaries = c.num_beneficiaries,
                        beneficiary_name = c.beneficiary_name,
                        beneficiary_phone = c.beneficiary_phone,
                        beneficiary_relationship = c.beneficiary_relationship,
                        beneficiary_allocation = c.beneficiary_allocation,
                        customer_no = c.customer_no,
                        date_created = c.date_created,
                        date_updated = c.date_updated,
                    };

                var record = await query.FirstOrDefaultAsync();
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
        /// Save new Customers record
        /// </summary>
        [HttpPost]
        [Route("api/customers/add")]
        public async Task<ActionResult> Add([FromBody] CustomersAddDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var modeldata = new Customers();
                modeldata.title = postdata.title;
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
                modeldata.marital_status = postdata.marital_status;
                modeldata.residential_address = postdata.residential_address;
                modeldata.ghana_card_number = postdata.ghana_card_number;
                modeldata.expiry_date = postdata.expiry_date;
                modeldata.place_of_issuance = postdata.place_of_issuance;
                modeldata.selfie = postdata.selfie;
                modeldata.id_card_front = postdata.id_card_front;
                modeldata.id_card_back = postdata.id_card_back;
                modeldata.investment_objective = postdata.investment_objective;
                modeldata.emergency_fund = postdata.emergency_fund;
                modeldata.investment_knowledge = postdata.investment_knowledge;
                modeldata.withdrawals = postdata.withdrawals;
                modeldata.risk_appetite = postdata.risk_appetite;
                modeldata.risk_assessment = postdata.risk_assessment;
                modeldata.source_of_funds = postdata.source_of_funds;
                modeldata.anticipated_activity = postdata.anticipated_activity;
                modeldata.deposit_frequency = postdata.deposit_frequency;
                modeldata.withdrawal_frequency = postdata.withdrawal_frequency;
                modeldata.regular_topup_amount = postdata.regular_topup_amount;
                modeldata.regular_withdrawal_amount = postdata.regular_withdrawal_amount;
                modeldata.employment_status = postdata.employment_status;
                modeldata.years_of_employment = postdata.years_of_employment;
                modeldata.employer_name = postdata.employer_name;
                modeldata.business_address = postdata.business_address;
                modeldata.nature_of_business = postdata.nature_of_business;
                modeldata.city_town = postdata.city_town;
                modeldata.monthly_income_range = postdata.monthly_income_range;
                modeldata.bank_name = postdata.bank_name;
                modeldata.account_name = postdata.account_name;
                modeldata.bank_branch = postdata.bank_branch;
                modeldata.account_number = postdata.account_number;
                modeldata.fund_name = postdata.fund_name;
                modeldata.debit_amount = postdata.debit_amount;
                modeldata.date_transaction = postdata.date_transaction;
                modeldata.frequency_deduction = postdata.frequency_deduction;
                modeldata.end_date = postdata.end_date;
                modeldata.head_of_state_in_gh = postdata.head_of_state_in_gh;
                modeldata.head_of_state_in_gh_name = postdata.head_of_state_in_gh_name;
                modeldata.head_of_state_out_gh = postdata.head_of_state_out_gh;
                modeldata.head_of_state_out_gh_name = postdata.head_of_state_out_gh_name;
                modeldata.signature_path = HandleBase64Image(
                    postdata.signature_path,
                    "signature_path"
                );
                modeldata.selfie_path = HandleBase64Image(postdata.selfie_path, "selfie_path");
                modeldata.front_path = HandleBase64Image(postdata.front_path, "front_path");
                modeldata.back_path = HandleBase64Image(postdata.back_path, "back_path");
                modeldata.add_beneficiary = postdata.add_beneficiary;
                modeldata.num_beneficiaries = postdata.num_beneficiaries;
                modeldata.beneficiary_name = postdata.beneficiary_name;
                modeldata.beneficiary_phone = postdata.beneficiary_phone;
                modeldata.beneficiary_relationship = postdata.beneficiary_relationship;
                modeldata.beneficiary_allocation = postdata.beneficiary_allocation;
                modeldata.date_created = DateTime.Now;
                modeldata.date_updated = DateTime.Now;

                // Generate a unique customer number (13 digits starting with 1000)
                modeldata.customer_no = GenerateUniqueCustomerNo();

                DB.Customers.Add(modeldata);
                var audit = GetAuditLog();
                DB.SaveChanges(audit);
                await SendAccountApprovalEmail(modeldata);
                return Ok(modeldata);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Get Customers record for edit
        /// </summary>
        [HttpGet]
        [Route("api/customers/edit/{id}")]
        public ActionResult Edit(int id)
        {
            try
            {
                var query =
                    from c in DB.Customers
                    select new Customers
                    {
                        id = c.id,
                        title = c.title,
                        firstname = c.firstname,
                        middlename = c.middlename,
                        surname = c.surname,
                        maiden_name = c.maiden_name,
                        phone = c.phone,
                        email = c.email,
                        dob = c.dob,
                        gender = c.gender,
                        digital_address = c.digital_address,
                        residential_status = c.residential_status,
                        country = c.country,
                        marital_status = c.marital_status,
                        residential_address = c.residential_address,
                        ghana_card_number = c.ghana_card_number,
                        expiry_date = c.expiry_date,
                        place_of_issuance = c.place_of_issuance,
                        selfie = c.selfie,
                        id_card_front = c.id_card_front,
                        id_card_back = c.id_card_back,
                        investment_objective = c.investment_objective,
                        emergency_fund = c.emergency_fund,
                        investment_knowledge = c.investment_knowledge,
                        withdrawals = c.withdrawals,
                        risk_appetite = c.risk_appetite,
                        risk_assessment = c.risk_assessment,
                        source_of_funds = c.source_of_funds,
                        anticipated_activity = c.anticipated_activity,
                        deposit_frequency = c.deposit_frequency,
                        withdrawal_frequency = c.withdrawal_frequency,
                        regular_topup_amount = c.regular_topup_amount,
                        regular_withdrawal_amount = c.regular_withdrawal_amount,
                        employment_status = c.employment_status,
                        years_of_employment = c.years_of_employment,
                        employer_name = c.employer_name,
                        business_address = c.business_address,
                        nature_of_business = c.nature_of_business,
                        city_town = c.city_town,
                        monthly_income_range = c.monthly_income_range,
                        bank_name = c.bank_name,
                        account_name = c.account_name,
                        bank_branch = c.bank_branch,
                        account_number = c.account_number,
                        fund_name = c.fund_name,
                        debit_amount = c.debit_amount,
                        date_transaction = c.date_transaction,
                        frequency_deduction = c.frequency_deduction,
                        end_date = c.end_date,
                        head_of_state_in_gh = c.head_of_state_in_gh,
                        head_of_state_in_gh_name = c.head_of_state_in_gh_name,
                        head_of_state_out_gh = c.head_of_state_out_gh,
                        head_of_state_out_gh_name = c.head_of_state_out_gh_name,
                        signature_path = c.signature_path,
                        selfie_path = c.selfie_path,
                        front_path = c.front_path,
                        back_path = c.back_path,
                        add_beneficiary = c.add_beneficiary,
                        num_beneficiaries = c.num_beneficiaries,
                        beneficiary_name = c.beneficiary_name,
                        beneficiary_phone = c.beneficiary_phone,
                        beneficiary_relationship = c.beneficiary_relationship,
                        beneficiary_allocation = c.beneficiary_allocation,
                        customer_no = c.customer_no,
                        date_created = c.date_created,
                        date_updated = c.date_updated,
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
        /// Update Customers record
        /// </summary>
        [HttpPost]
        [Route("api/customers/edit/{id}")]
        public ActionResult Edit(int id, [FromBody] CustomersEditDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var query = from c in DB.Customers select c;
                query = query.Where(p => p.id.Equals(id));
                var record = query.FirstOrDefault();
                if (record == null)
                {
                    return NotFound("No record found");
                }

                record.title = postdata.title;
                record.firstname = postdata.firstname;
                record.middlename = postdata.middlename;
                record.surname = postdata.surname;
                record.maiden_name = postdata.maiden_name;
                record.phone = postdata.phone;
                record.email = postdata.email;
                record.dob = postdata.dob;
                record.gender = postdata.gender;
                record.digital_address = postdata.digital_address;
                record.residential_status = postdata.residential_status;
                record.country = postdata.country;
                record.marital_status = postdata.marital_status;
                record.residential_address = postdata.residential_address;
                record.ghana_card_number = postdata.ghana_card_number;
                record.expiry_date = postdata.expiry_date;
                record.place_of_issuance = postdata.place_of_issuance;
                record.selfie = postdata.selfie;
                record.id_card_front = postdata.id_card_front;
                record.id_card_back = postdata.id_card_back;
                record.investment_objective = postdata.investment_objective;
                record.emergency_fund = postdata.emergency_fund;
                record.investment_knowledge = postdata.investment_knowledge;
                record.withdrawals = postdata.withdrawals;
                record.risk_appetite = postdata.risk_appetite;
                record.risk_assessment = postdata.risk_assessment;
                record.source_of_funds = postdata.source_of_funds;
                record.anticipated_activity = postdata.anticipated_activity;
                record.deposit_frequency = postdata.deposit_frequency;
                record.withdrawal_frequency = postdata.withdrawal_frequency;
                record.regular_topup_amount = postdata.regular_topup_amount;
                record.regular_withdrawal_amount = postdata.regular_withdrawal_amount;
                record.employment_status = postdata.employment_status;
                record.years_of_employment = postdata.years_of_employment;
                record.employer_name = postdata.employer_name;
                record.business_address = postdata.business_address;
                record.nature_of_business = postdata.nature_of_business;
                record.city_town = postdata.city_town;
                record.monthly_income_range = postdata.monthly_income_range;
                record.bank_name = postdata.bank_name;
                record.account_name = postdata.account_name;
                record.bank_branch = postdata.bank_branch;
                record.account_number = postdata.account_number;
                record.fund_name = postdata.fund_name;
                record.debit_amount = postdata.debit_amount;
                record.date_transaction = postdata.date_transaction;
                record.frequency_deduction = postdata.frequency_deduction;
                record.end_date = postdata.end_date;
                record.head_of_state_in_gh = postdata.head_of_state_in_gh;
                record.head_of_state_in_gh_name = postdata.head_of_state_in_gh_name;
                record.head_of_state_out_gh = postdata.head_of_state_out_gh;
                record.head_of_state_out_gh_name = postdata.head_of_state_out_gh_name;
                record.signature_path = HandleBase64Image(
                    postdata.signature_path,
                    "signature_path"
                );
                record.selfie_path = HandleBase64Image(postdata.selfie_path, "selfie_path");
                record.front_path = HandleBase64Image(postdata.front_path, "front_path");
                record.back_path = HandleBase64Image(postdata.back_path, "back_path");
                record.add_beneficiary = postdata.add_beneficiary;
                record.num_beneficiaries = postdata.num_beneficiaries;
                record.beneficiary_name = postdata.beneficiary_name;
                record.beneficiary_phone = postdata.beneficiary_phone;
                record.beneficiary_relationship = postdata.beneficiary_relationship;
                record.beneficiary_allocation = postdata.beneficiary_allocation;

                record.date_updated = DateTime.Now;

                DB.Update(record);
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
        /// Create a new customer record. Checks for existing records are bypassed.
        /// </summary>
        [HttpPost]
        [Route("api/customers/upsert")]
        [AllowAnonymous]
        public async Task<ActionResult> Upsert([FromBody] CustomersAddDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var modeldata = new Customers
                {
                    title = postdata.title,
                    firstname = postdata.firstname,
                    middlename = postdata.middlename,
                    surname = postdata.surname,
                    maiden_name = postdata.maiden_name,
                    phone = postdata.phone,
                    email = postdata.email,
                    dob = postdata.dob,
                    gender = postdata.gender,
                    digital_address = postdata.digital_address,
                    residential_status = postdata.residential_status,
                    country = postdata.country,
                    marital_status = postdata.marital_status,
                    residential_address = postdata.residential_address,
                    ghana_card_number = postdata.ghana_card_number,
                    expiry_date = postdata.expiry_date,
                    place_of_issuance = postdata.place_of_issuance,
                    selfie = postdata.selfie,
                    id_card_front = postdata.id_card_front,
                    id_card_back = postdata.id_card_back,
                    investment_objective = postdata.investment_objective,
                    emergency_fund = postdata.emergency_fund,
                    investment_knowledge = postdata.investment_knowledge,
                    withdrawals = postdata.withdrawals,
                    risk_appetite = postdata.risk_appetite,
                    risk_assessment = postdata.risk_assessment,

                    source_of_funds = postdata.source_of_funds,
                    anticipated_activity = postdata.anticipated_activity,
                    deposit_frequency = postdata.deposit_frequency,
                    withdrawal_frequency = postdata.withdrawal_frequency,
                    regular_topup_amount = postdata.regular_topup_amount,
                    regular_withdrawal_amount = postdata.regular_withdrawal_amount,

                    employment_status = postdata.employment_status,
                    years_of_employment = postdata.years_of_employment,
                    employer_name = postdata.employer_name,
                    business_address = postdata.business_address,
                    nature_of_business = postdata.nature_of_business,
                    city_town = postdata.city_town,
                    monthly_income_range = postdata.monthly_income_range,

                    bank_name = postdata.bank_name,
                    account_name = postdata.account_name,
                    bank_branch = postdata.bank_branch,
                    account_number = postdata.account_number,
                    fund_name = postdata.fund_name,
                    debit_amount = postdata.debit_amount,
                    date_transaction = postdata.date_transaction,
                    frequency_deduction = postdata.frequency_deduction,
                    end_date = postdata.end_date,

                    head_of_state_in_gh = postdata.head_of_state_in_gh,
                    head_of_state_in_gh_name = postdata.head_of_state_in_gh_name,
                    head_of_state_out_gh = postdata.head_of_state_out_gh,
                    head_of_state_out_gh_name = postdata.head_of_state_out_gh_name,
                    signature_path = HandleBase64Image(
                        postdata.signature_path,
                        "signature_path"
                    ),
                    selfie_path = HandleBase64Image(postdata.selfie_path, "selfie_path"),
                    front_path = HandleBase64Image(postdata.front_path, "front_path"),
                    back_path = HandleBase64Image(postdata.back_path, "back_path"),

                    add_beneficiary = postdata.add_beneficiary,
                    num_beneficiaries = postdata.num_beneficiaries,
                    beneficiary_name = postdata.beneficiary_name,
                    beneficiary_phone = postdata.beneficiary_phone,
                    beneficiary_relationship = postdata.beneficiary_relationship,
                    beneficiary_allocation = postdata.beneficiary_allocation,

                    date_created = DateTime.Now,
                    date_updated = DateTime.Now,
                };

                // Generate a unique customer number (13 digits starting with 1000)
                modeldata.customer_no = GenerateUniqueCustomerNo();

                DB.Customers.Add(modeldata);

                // Find associated user if any
                var userFound = DB.Users.FirstOrDefault(p => (p.email != null && p.email == postdata.email) || (p.phone != null && p.phone == postdata.phone));
                int userId = userFound?.id ?? 0;

                // Save Bank Account information
                if (!string.IsNullOrEmpty(postdata.bank_name))
                {
                    var bankAccount = new BankAccount
                    {
                        user_id = userId,
                        bank_name = postdata.bank_name,
                        account_name = postdata.account_name,
                        fund_name = postdata.fund_name,
                        debit_amount = postdata.debit_amount,
                        date_transaction = postdata.date_transaction,
                        frequency_deduction = postdata.frequency_deduction,
                        end_date = postdata.end_date,
                        date_created = DateTime.Now,
                        date_updated = DateTime.Now
                    };
                    DB.BankAccount.Add(bankAccount);
                }

                // Save Employment information
                if (!string.IsNullOrEmpty(postdata.employer_name))
                {
                    var employment = new Employment
                    {
                        user_id = userId,
                        name_employer = postdata.employer_name,
                        years_employ = postdata.years_of_employment,
                        address = postdata.business_address,
                        nature_business = postdata.nature_of_business,
                        location = postdata.city_town,
                        income_range = postdata.monthly_income_range,
                        date_created = DateTime.Now,
                        date_updated = DateTime.Now
                    };
                    DB.Employment.Add(employment);
                }

                var audit = GetAuditLog();
                DB.SaveChanges(audit);
                await SendAccountApprovalEmail(modeldata);
                return Ok(modeldata);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Soft Delete Customers record
        /// </summary>
        [HttpGet]
        [Route("api/customers/delete/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                List<string> arrId = id.Split(",").ToList();
                var query = DB.Customers.AsQueryable();
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

        [HttpGet]
        private async Task<IActionResult> ExportListRecords(List<Customers> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportList";
            try
            {
                var fileName = "CustomersReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "First Name", Key = "firstname" },
                        new DataHeader { Header = "Surname", Key = "surname" },
                        new DataHeader { Header = "Phone", Key = "phone" },
                        new DataHeader { Header = "Email", Key = "email" },
                        new DataHeader { Header = "Marital Status", Key = "marital_status" },
                        new DataHeader
                        {
                            Header = "Residential Address",
                            Key = "residential_address",
                        },
                        new DataHeader { Header = "Ghana Card Number", Key = "ghana_card_number" },
                        new DataHeader
                        {
                            Header = "Investment Objective",
                            Key = "investment_objective",
                        },
                        new DataHeader { Header = "Emergency Fund", Key = "emergency_fund" },
                        new DataHeader
                        {
                            Header = "Investment Knowledge",
                            Key = "investment_knowledge",
                        },
                        new DataHeader { Header = "Withdrawals", Key = "withdrawals" },
                        new DataHeader { Header = "Risk Appetite", Key = "risk_appetite" },
                        new DataHeader { Header = "Risk Assessment", Key = "risk_assessment" },
                        new DataHeader { Header = "Account No.", Key = "customer_no" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                        new DataHeader { Header = "Date Updated", Key = "date_updated" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Customers";
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

        [HttpGet]
        private async Task<IActionResult> ExportViewRecords(List<Customers> records)
        {
            string exportFormat = Request.Query["export"].ToString().ToLower();
            string viewName = "ExportView";
            try
            {
                var fileName = "CustomersReport";
                if (exportFormat == "excel" || exportFormat == "csv")
                {
                    List<DataHeader> Columns = new List<DataHeader>()
                    {
                        new DataHeader { Header = "Id", Key = "id" },
                        new DataHeader { Header = "First Name", Key = "firstname" },
                        new DataHeader { Header = "Surname", Key = "surname" },
                        new DataHeader { Header = "Phone", Key = "phone" },
                        new DataHeader { Header = "Email", Key = "email" },
                        new DataHeader { Header = "Marital Status", Key = "marital_status" },
                        new DataHeader
                        {
                            Header = "Residential Address",
                            Key = "residential_address",
                        },
                        new DataHeader { Header = "Ghana Card Number", Key = "ghana_card_number" },
                        new DataHeader
                        {
                            Header = "Investment Objective",
                            Key = "investment_objective",
                        },
                        new DataHeader { Header = "Emergency Fund", Key = "emergency_fund" },
                        new DataHeader
                        {
                            Header = "Investment Knowledge",
                            Key = "investment_knowledge",
                        },
                        new DataHeader { Header = "Withdrawals", Key = "withdrawals" },
                        new DataHeader { Header = "Risk Assessment", Key = "risk_assessment" },
                        new DataHeader { Header = "Account No.", Key = "customer_no" },
                        new DataHeader { Header = "Date Created", Key = "date_created" },
                    };
                    var dataTable = records.ToDataTable(Columns);
                    dataTable.TableName = "Customers";
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

        private string? HandleBase64Image(string? base64Data, string fieldName)
        {
            if (string.IsNullOrEmpty(base64Data) || !base64Data.Contains(";base64,"))
            {
                return base64Data;
            }

            try
            {
                var parts = base64Data.Split(',');
                var info = parts[0];
                var data = parts[1];
                var bytes = Convert.FromBase64String(data);

                var ext = info.Contains("image/png") ? "png" : "jpg";
                var fileName = $"{Guid.NewGuid()}.{ext}";
                var uploadDir = Config["UPLOAD_TEMP_DIR"] ?? "uploads/temp";
                var webRoot = hostEnvironment.WebRootPath;
                var filePath = Path.Combine(webRoot, uploadDir, fileName);

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                System.IO.File.WriteAllBytes(filePath, bytes);

                // Use the BaseController's MoveUploadedFiles pattern if preferred,
                // but for now, we'll store it in the upload dir directly
                var relativePath = Path.Combine(uploadDir, fileName).Replace("\\", "/");
                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving base64 image [{fieldName}]: {ex.Message}");
                return base64Data;
            }
        }

        private async Task SendAccountApprovalEmail(Customers customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.email))
                    return;

                string fullName = string.Join(
                        " ",
                        new[] { customer.firstname, customer.middlename, customer.surname }.Where(
                            s => !string.IsNullOrWhiteSpace(s)
                        )
                    )
                    .Trim();
                string subject = "MyFintech Account Number";
                
                ViewBag.FullName = fullName;
                ViewBag.CustomerNo = customer.customer_no;
                var message = await this.RenderViewAsync("AccountApprovalTemplate", "");
                var targetEmail = customer.email;

                _ = Task.Run(() =>
                {
                    try
                    {
                        Mailer.SendEmail(targetEmail, subject, message);
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Error sending account approval email: {innerEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error preparing account approval email: {ex.Message}");
            }
        }

        private string GenerateUniqueCustomerNo()
        {
            string customerNo;
            bool exists;
            do
            {
                // Generate 13 digits starting with 1000
                long timestampPart = DateTime.Now.Ticks % 1000000000;
                customerNo = "4000" + timestampPart.ToString("D9");
                exists = DB.Customers.Any(c => c.customer_no == customerNo);
            } while (exists);
            return customerNo;
        }
    }
}
