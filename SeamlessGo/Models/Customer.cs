using System.ComponentModel.DataAnnotations;
namespace SeamlessGo.Models

{
    public class Customer
    {
        public string? CustomerID { get; set; }
        public string? CustomerCode { get; set; }
        public string? FullName { get; set; }
        public int? CityID { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? PhoneNumber2 { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? CustomerTypeID { get; set; }
        public decimal? CustomerBalance { get; set; }
        public decimal? AccountLimit { get; set; }
        public int? CustomerGroupID { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public string? CustomerNote { get; set; }


    }
}
