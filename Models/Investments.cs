using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("investments")]
    public class Investments : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public int user_id { get; set; } = default!;
        public string? investment_type { get; set; } = default!;
        public string? product_name { get; set; } = default!;
        public string? account_number { get; set; } = default!;
        public int? account_amount { get; set; } = default!;
        public string? payment_channel { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class InvestmentsAddDTO
    {
        [Required]
        public int user_id { get; set; } = default!;

        [Required]
        public string? investment_type { get; set; } = default!;

        [Required]
        public string? product_name { get; set; } = default!;

        [Required]
        public string? account_number { get; set; } = default!;

        [Required]
        public int? account_amount { get; set; } = default!;

        [Required]
        public string? payment_channel { get; set; } = default!;
    }

    public class InvestmentsEditDTO
    {
        [RequiredIfEmpty]
        public int user_id { get; set; } = default!;

        [RequiredIfEmpty]
        public string? investment_type { get; set; } = default!;

        [RequiredIfEmpty]
        public string? product_name { get; set; } = default!;

        [RequiredIfEmpty]
        public string? account_number { get; set; } = default!;

        [RequiredIfEmpty]
        public int? account_amount { get; set; } = default!;

        [RequiredIfEmpty]
        public string? payment_channel { get; set; } = default!;
    }

}
