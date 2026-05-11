using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("bank_account")]
    public class BankAccount : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public int? user_id { get; set; } = default!;
        public string? bank_name { get; set; } = default!;
        public string? account_name { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;
        public string? date_transaction { get; set; } = default!;
        public string? frequency_deduction { get; set; } = default!;
        public string? end_date { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class BankaccountAddDTO
    {
        [Required]
        public int? user_id { get; set; } = default!;

        [Required]
        public string? bank_name { get; set; } = default!;

        [Required]
        public string? account_name { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;

        [Required]
        public string? frequency_deduction { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? date_transaction { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? end_date { get; set; } = default!;
    }

    public class BankaccountEditDTO
    {
        [RequiredIfEmpty]
        public int? user_id { get; set; } = default!;

        [RequiredIfEmpty]
        public string? bank_name { get; set; } = default!;

        [RequiredIfEmpty]
        public string? account_name { get; set; } = default!;
        public string? fund_name { get; set; } = default!;
        public string? debit_amount { get; set; } = default!;

        [RequiredIfEmpty]
        public string? frequency_deduction { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? date_transaction { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? end_date { get; set; } = default!;
    }
}

