namespace SeamlessGo.Models
{
    public class Payments
    {

        public string? PaymentID { get; set; }
        public decimal? Amount { get; set; }
        public int? ChequeID { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CurrencyID { get; set; }
        public string? CustomerID { get; set; }
        public int? PaymentMethod { get; set; }
        public int? PaymentStatus { get; set; }
        public int? RouteID { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsVoided { get; set; }
        public Guid? ClientID { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        public List <PaymentAllocations>? Allocations { get; set; }

    }
}
