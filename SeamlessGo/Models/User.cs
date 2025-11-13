using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeamlessGo.Models
{
    public class User
    {
        [Key]
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PasswordSalt { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public string? DisplayName { get; set; }
        public int? UserRoleID { get; set; }
        public Guid? ClientID { get; set; }        // foreign key
        public Client? client { get; set; }
        public StockLocation? stockLocation { get; set; }


    }
}
