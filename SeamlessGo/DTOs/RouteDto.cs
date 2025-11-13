namespace SeamlessGo.DTOs
{
    public class RouteDto
    {
        public int? RouteID { get; set; }
        public int? PlanID { get; set; }
        public int? UserID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte? Status { get; set; }
    }
}
