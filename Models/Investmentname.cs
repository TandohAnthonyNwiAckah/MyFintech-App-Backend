using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TANA.Models
{
    [Table("investment_name")]
    public class InvestmentName : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public string? name { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class InvestmentnameAddDTO
    {
        public string? name { get; set; } = default!;
    }

    public class InvestmentnameEditDTO
    {
        public string? name { get; set; } = default!;
    }
}

