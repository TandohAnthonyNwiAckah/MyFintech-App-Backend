using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TANA.Models
{
    [Table("product_name")]
    public class ProductName : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public string? name { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class ProductnameAddDTO
    {
        public string? name { get; set; } = default!;
    }

    public class ProductnameEditDTO
    {
        public string? name { get; set; } = default!;
    }
}
