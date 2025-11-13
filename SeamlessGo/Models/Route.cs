using System.ComponentModel.DataAnnotations;

namespace SeamlessGo.Models
{
    public class Route
    {

        [Key]
        public int? RouteID { get; set; }
        public int? PlanID { get; set; }
        public int? UserID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte? Status { get; set; }
    }
}
