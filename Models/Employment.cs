using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("employment")]
    public class Employment : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public int user_id { get; set; } = default!;
        public string? name_employer { get; set; } = default!;
        public string? years_employ { get; set; } = default!;
        public string? address { get; set; } = default!;
        public string? nature_business { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? income_range { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class EmploymentAddDTO
    {
        [Required]
        public int user_id { get; set; } = default!;
        public string? name_employer { get; set; } = default!;
        public string? years_employ { get; set; } = default!;
        public string? address { get; set; } = default!;
        public string? nature_business { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? income_range { get; set; } = default!;
    }

    public class EmploymentEditDTO
    {
        [RequiredIfEmpty]
        public int user_id { get; set; } = default!;
        public string? name_employer { get; set; } = default!;
        public string? years_employ { get; set; } = default!;
        public string? address { get; set; } = default!;
        public string? nature_business { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? income_range { get; set; } = default!;
    }
}

