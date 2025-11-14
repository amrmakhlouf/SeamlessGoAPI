namespace SeamlessGo.Models
{
    public class Plan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string PlanDesc { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CreatedByUserID { get; set; }
        public int PlanUserID { get; set; }
    }
}
