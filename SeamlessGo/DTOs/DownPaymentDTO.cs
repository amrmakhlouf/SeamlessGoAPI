using SeamlessGo.Models;

namespace SeamlessGo.DTOs
{
    public class DownPaymentDTO
    {
        public string? DownPaymentID { get; set; }
        public string? Amount { get; set; }
        public int? ChequeID { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedFromPaymentID { get; set; }
        public int? CurrencyID { get; set; }
        public string? CustomerID { get; set; }
        public int? PaymentMethod { get; set; }
        public int? PaymentStatus { get; set; }
        public string? RemainingAmount { get; set; }
        public int? RouteID { get; set; }
        public int? SyncStatus { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsVoided { get; set; }
        public Guid? ClientID { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        public List<DownPaymentAllocationsDTO>? Allocations { get; set; }
    }
}
