using System.ComponentModel.DataAnnotations;

namespace SeamlessGo.Models
{
    public class Client
    {
        [Key]
        public Guid? ClientID { get; set; }
        public string? ClientName { get; set; }
        public string? OrganizationName { get; set; }
        public string? ContactPhone { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool? IsActive { get; set; }
       public DateTime? CreatedDate { get; set; }
    }
}
