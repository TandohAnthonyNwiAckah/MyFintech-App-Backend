using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("roles")]
    public class Roles : BaseRecord
    {
        [Key]
        public int role_id { get; set; } = default!;
        public string role_name { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class RolesAddDTO
    {
        [Required]
        public string role_name { get; set; } = default!;
    }

    public class RolesEditDTO
    {
        [RequiredIfEmpty]
        public string role_name { get; set; } = default!;
    }


}
