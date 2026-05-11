using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TANA.Helpers;

namespace TANA.Models
{
    [Table("users")]
    public class Users : BaseRecord
    {
        [Key]
        public int id { get; set; } = default!;
        public string? title { get; set; } = default!;
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? email { get; set; } = default!;
        public string? photo { get; set; } = default!;
        public string? password { get; set; } = default!;
        public string? dob { get; set; } = default!;
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;
        public string? country { get; set; } = default!;
        public int? user_role_id { get; set; } = default!;
        public DateTime? date_created { get; set; } = default!;
        public DateTime? date_updated { get; set; } = default!;
        public DateTime? date_deleted { get; set; } = default!;
    }

    public class UsersRegisterDTO
    {
        [Required]
        public string firstname { get; set; } = default!;

        [Required]
        public string surname { get; set; } = default!;

        public string? middlename { get; set; } // Optional

        [RecordUnique("phone", "users")]
        [Required]
        [MinLength(9)]
        [MaxLength(12)]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number must contain only numbers")]
        public string phone { get; set; } = default!;

        [RecordUnique("email", "users")]
        [Required]
        [EmailAddress]
        public string email { get; set; } = default!;

        public string? photo { get; set; } // Optional

        [Required]
        public string password { get; set; } = default!;

        [Compare("password", ErrorMessage = "Passwords do not match")]
        [NotMapped]
        public string confirm_password { get; set; } = default!;
    }

    public class UsersAccounteditDTO
    {
        public string? title { get; set; } = default!;
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;
        public string? phone { get; set; } = default!;
        public string? photo { get; set; } = default!;
        public string? dob { get; set; } = default!;
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;
        public string? country { get; set; } = default!;
        public int? user_role_id { get; set; } = default!;
    }

    public class UsersAddDTO
    {
        [Required]
        public string? title { get; set; } = default!;
        public string? photo { get; set; } = default!;

        [Required]
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;

        [Required]
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;

        [RecordUnique("phone", "users")]
        [Required]
        public string? phone { get; set; } = default!;

        [RecordUnique("email", "users")]
        [Required]
        [EmailAddress]
        public string? email { get; set; } = default!;

        [Required]
        public string? password { get; set; } = default!;

        [Compare("password")]
        [NotMapped]
        public string Confirm_Password { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? dob { get; set; } = default!;

        [Required]
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;

        [Required]
        public string? country { get; set; } = default!;
        public int? user_role_id { get; set; } = default!;
    }

    public class UsersEditDTO
    {
        [RequiredIfEmpty]
        public string? title { get; set; } = default!;
        public string? photo { get; set; } = default!;

        [RequiredIfEmpty]
        public string? firstname { get; set; } = default!;
        public string? middlename { get; set; } = default!;

        [RequiredIfEmpty]
        public string? surname { get; set; } = default!;
        public string? maiden_name { get; set; } = default!;

        [RequiredIfEmpty]
        public string? phone { get; set; } = default!;

        [RequiredIfEmpty]
        [EmailAddress]
        public string? email { get; set; } = default!;

        [DataType(DataType.Date)]
        public string? dob { get; set; } = default!;

        [RequiredIfEmpty]
        public string? gender { get; set; } = default!;
        public string? digital_address { get; set; } = default!;
        public string? residential_status { get; set; } = default!;

        [RequiredIfEmpty]
        public string? country { get; set; } = default!;
        public int? user_role_id { get; set; } = default!;
    }
}
