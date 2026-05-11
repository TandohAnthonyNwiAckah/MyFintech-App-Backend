using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("beneficiaries")]
    public class Beneficiaries : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public int? user_id { get; set; } = default!;
        public string? full_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? relationship { get; set; } = default!;
        public string? allocation { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class BeneficiariesAddDTO
    {
        public int? user_id { get; set; } = default!;

        [Required]
        public string? full_name { get; set; } = default!;

        [Required]
        public string? phone { get; set; } = default!;
        public string? relationship { get; set; } = default!;
        public string? allocation { get; set; } = default!;
    }

    public class BeneficiariesEditDTO
    {
        public int? user_id { get; set; } = default!;

        [RequiredIfEmpty]
        public string? full_name { get; set; } = default!;

        [RequiredIfEmpty]
        public string? phone { get; set; } = default!;
        public string? relationship { get; set; } = default!;
        public string? allocation { get; set; } = default!;
    }
}

