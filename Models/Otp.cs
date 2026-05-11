using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TANA.Models
{
    [Table("otp")]
    public class Otp
    {
        [Key]
        public int id { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? otp_code { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public bool is_verified { get; set; } = false;
    }

    public class OtpVerifyDTO
    {
        [Required]
        public string username { get; set; } = default!;

        [Required]
        public string otp_code { get; set; } = default!;
    }

    public class OtpRetryDTO
    {
        [Required]
        public string username { get; set; } = default!;
    }
}
