using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("permissions")]
    public class Permissions : BaseRecord
    {
        [Key]
        public int permission_id { get; set; } = default!;
        public string permission { get; set; } = default!;
        public int? role_id { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class PermissionsAddDTO
    {
        [Required]
        public string permission { get; set; } = default!;
        public int? role_id { get; set; } = default!;
    }

    public class PermissionsEditDTO
    {
        [RequiredIfEmpty]
        public string permission { get; set; } = default!;
        public int? role_id { get; set; } = default!;
    }

}
