namespace SeamlessGo.Models
{
    public class CustomerVisits
    {
        public int RouteID { get; set; }
        public Guid CustomerID { get; set; }
        public int VisitNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public byte Status { get; set; }
        public string Note { get; set; }
    }
}
