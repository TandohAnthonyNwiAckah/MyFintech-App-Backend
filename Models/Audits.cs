using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TANA.Models
{
    [Table("audits")]
    public class Audits : BaseRecord
    {
        [Key]
        public int log_id { get; set; } = default!;
        public string? action { get; set; } = default!;
        public string? page { get; set; } = default!;
        public string? record_id { get; set; } = default!;
        public string? user_id { get; set; } = default!;
        public string? user_ip { get; set; } = default!;
        public string? user_agent { get; set; } = default!;
        public string? request_url { get; set; } = default!;
        public string? old_values { get; set; } = default!;
        public string? new_values { get; set; } = default!;
        public DateTime? timestamp { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

}
