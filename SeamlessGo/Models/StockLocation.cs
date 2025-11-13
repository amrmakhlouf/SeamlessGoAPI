using System.ComponentModel.DataAnnotations;

namespace SeamlessGo.Models
{
    public class StockLocation
    {
        
        [Key]
        public int? LocationID { get; set; }
        public string? LocationName { get; set; }
        public string? LocationCode { get; set; }
        public int? StockLocationTypeID { get; set; }
        public string? VehiclePlate { get; set; }
        public int? DriverUserID { get; set; }
        public int? Capacity { get; set; }
        public bool? IsActive { get; set; }
        public string? Address { get; set; }
        public string? Region { get; set; }
        public int? CityID { get; set; }
        public int? CountryID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByUserID { get; set; }
        public byte? SyncStatus { get; set; }
    }
}
