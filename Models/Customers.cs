using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("customers")]
    public class Customers : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;

        // Personal Details
        public string? title { get; set; } = default!;
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? email { get; set; } = default!;
        public string? dob { get; set; } = default!;
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;
        public string? country { get; set; } = default!;
        public string? marital_status { get; set; } = default!;
        public string? residential_address { get; set; } = default!;

        // Ghana Card Details
        public string? ghana_card_number { get; set; } = default!;
        public string? expiry_date { get; set; } = default!;
        public string? place_of_issuance { get; set; } = default!;
        public string? selfie { get; set; } = default!;
        public string? id_card_front { get; set; } = default!;
        public string? id_card_back { get; set; } = default!;

        // Investment Info
        public string? investment_objective { get; set; } = default!;
        public string? emergency_fund { get; set; } = default!;
        public string? investment_knowledge { get; set; } = default!;
        public string? withdrawals { get; set; } = default!;
        public string? risk_appetite { get; set; } = default!;
        public string? risk_assessment { get; set; } = default!;

        // Activity & Funds
        public string? source_of_funds { get; set; } = default!;
        public string? anticipated_activity { get; set; } = default!;
        public string? deposit_frequency { get; set; } = default!;
        public string? withdrawal_frequency { get; set; } = default!;
        public string? regular_topup_amount { get; set; } = default!;
        public string? regular_withdrawal_amount { get; set; } = default!;

        // Employment
        public string? employment_status { get; set; } = default!;
        public string? years_of_employment { get; set; } = default!;
        public string? employer_name { get; set; } = default!;
        public string? business_address { get; set; } = default!;
        public string? nature_of_business { get; set; } = default!;
        public string? city_town { get; set; } = default!;
        public string? monthly_income_range { get; set; } = default!;

        // Bank
        public string? bank_name { get; set; } = default!;
        public string? account_name { get; set; } = default!;
        public string? bank_branch { get; set; } = default!;
        public string? account_number { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;
        public string? date_transaction { get; set; } = default!;
        public string? frequency_deduction { get; set; } = default!;
        public string? end_date { get; set; } = default!;

        // Additional
        public string? head_of_state_in_gh { get; set; } = default!;
        public string? head_of_state_in_gh_name { get; set; } = default!;
        public string? head_of_state_out_gh { get; set; } = default!;
        public string? head_of_state_out_gh_name { get; set; } = default!;
        public string? signature_path { get; set; } = default!;
        public string? selfie_path { get; set; } = default!;
        public string? front_path { get; set; } = default!;
        public string? back_path { get; set; } = default!;

        // Beneficiaries
        public string? add_beneficiary { get; set; } = default!;
        public string? num_beneficiaries { get; set; } = default!;
        public string? beneficiary_name { get; set; } = default!;
        public string? beneficiary_phone { get; set; } = default!;
        public string? beneficiary_relationship { get; set; } = default!;
        public string? beneficiary_allocation { get; set; } = default!;
        public string? customer_no { get; set; } = default!;

        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class CustomersAddDTO
    {
        public string? title { get; set; } = default!;
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? email { get; set; } = default!;
        public string? dob { get; set; } = default!;
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;
        public string? country { get; set; } = default!;
        public string? marital_status { get; set; } = default!;
        public string? residential_address { get; set; } = default!;
        public string? ghana_card_number { get; set; } = default!;
        public string? expiry_date { get; set; } = default!;
        public string? place_of_issuance { get; set; } = default!;
        public string? selfie { get; set; } = default!;
        public string? id_card_front { get; set; } = default!;
        public string? id_card_back { get; set; } = default!;
        public string? investment_objective { get; set; } = default!;
        public string? emergency_fund { get; set; } = default!;
        public string? investment_knowledge { get; set; } = default!;
        public string? withdrawals { get; set; } = default!;
        public string? risk_appetite { get; set; } = default!;
        public string? risk_assessment { get; set; } = default!;

        public string? source_of_funds { get; set; } = default!;
        public string? anticipated_activity { get; set; } = default!;
        public string? deposit_frequency { get; set; } = default!;
        public string? withdrawal_frequency { get; set; } = default!;
        public string? regular_topup_amount { get; set; } = default!;
        public string? regular_withdrawal_amount { get; set; } = default!;

        public string? employment_status { get; set; } = default!;
        public string? years_of_employment { get; set; } = default!;
        public string? employer_name { get; set; } = default!;
        public string? business_address { get; set; } = default!;
        public string? nature_of_business { get; set; } = default!;
        public string? city_town { get; set; } = default!;
        public string? monthly_income_range { get; set; } = default!;

        public string? bank_name { get; set; } = default!;
        public string? account_name { get; set; } = default!;
        public string? bank_branch { get; set; } = default!;
        public string? account_number { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;
        public string? date_transaction { get; set; } = default!;
        public string? frequency_deduction { get; set; } = default!;
        public string? end_date { get; set; } = default!;

        public string? head_of_state_in_gh { get; set; } = default!;
        public string? head_of_state_in_gh_name { get; set; } = default!;
        public string? head_of_state_out_gh { get; set; } = default!;
        public string? head_of_state_out_gh_name { get; set; } = default!;
        public string? signature_path { get; set; } = default!;
        public string? selfie_path { get; set; } = default!;
        public string? front_path { get; set; } = default!;
        public string? back_path { get; set; } = default!;

        // Beneficiaries
        public string? add_beneficiary { get; set; } = default!;
        public string? num_beneficiaries { get; set; } = default!;
        public string? beneficiary_name { get; set; } = default!;
        public string? beneficiary_phone { get; set; } = default!;
        public string? beneficiary_relationship { get; set; } = default!;
        public string? beneficiary_allocation { get; set; } = default!;
    }

    public class CustomersEditDTO
    {
        public string? title { get; set; } = default!;
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? email { get; set; } = default!;
        public string? dob { get; set; } = default!;
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;
        public string? country { get; set; } = default!;
        public string? marital_status { get; set; } = default!;
        public string? residential_address { get; set; } = default!;
        public string? ghana_card_number { get; set; } = default!;
        public string? expiry_date { get; set; } = default!;
        public string? place_of_issuance { get; set; } = default!;
        public string? selfie { get; set; } = default!;
        public string? id_card_front { get; set; } = default!;
        public string? id_card_back { get; set; } = default!;
        public string? investment_objective { get; set; } = default!;
        public string? emergency_fund { get; set; } = default!;
        public string? investment_knowledge { get; set; } = default!;
        public string? withdrawals { get; set; } = default!;
        public string? risk_appetite { get; set; } = default!;
        public string? risk_assessment { get; set; } = default!;

        public string? source_of_funds { get; set; } = default!;
        public string? anticipated_activity { get; set; } = default!;
        public string? deposit_frequency { get; set; } = default!;
        public string? withdrawal_frequency { get; set; } = default!;
        public string? regular_topup_amount { get; set; } = default!;
        public string? regular_withdrawal_amount { get; set; } = default!;

        public string? employment_status { get; set; } = default!;
        public string? years_of_employment { get; set; } = default!;
        public string? employer_name { get; set; } = default!;
        public string? business_address { get; set; } = default!;
        public string? nature_of_business { get; set; } = default!;
        public string? city_town { get; set; } = default!;
        public string? monthly_income_range { get; set; } = default!;

        public string? bank_name { get; set; } = default!;
        public string? account_name { get; set; } = default!;
        public string? bank_branch { get; set; } = default!;
        public string? account_number { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;
        public string? date_transaction { get; set; } = default!;
        public string? frequency_deduction { get; set; } = default!;
        public string? end_date { get; set; } = default!;

        public string? head_of_state_in_gh { get; set; } = default!;
        public string? head_of_state_in_gh_name { get; set; } = default!;
        public string? head_of_state_out_gh { get; set; } = default!;
        public string? head_of_state_out_gh_name { get; set; } = default!;
        public string? signature_path { get; set; } = default!;
        public string? selfie_path { get; set; } = default!;
        public string? front_path { get; set; } = default!;
        public string? back_path { get; set; } = default!;

        // Beneficiaries
        public string? add_beneficiary { get; set; } = default!;
        public string? num_beneficiaries { get; set; } = default!;
        public string? beneficiary_name { get; set; } = default!;
        public string? beneficiary_phone { get; set; } = default!;
        public string? beneficiary_relationship { get; set; } = default!;
        public string? beneficiary_allocation { get; set; } = default!;
    }
}
