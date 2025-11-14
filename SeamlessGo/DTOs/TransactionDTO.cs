using SeamlessGo.Models;

namespace SeamlessGo.DTOs
{
    public class TransactionDTO
    {
        public string? TransactionID { get; set; }
        public string? CustomerID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? TransactionTypeID { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? TotalRemainingAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public float? DiscountPerc { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? Tax { get; set; }
        public float? TaxPerc { get; set; }
        public int? Status { get; set; }
        public int? CreatedByUserID { get; set; }
        public int? RouteID { get; set; }
        public bool? IsVoided { get; set; }
        public string? Note { get; set; }
        public string? SourceTransactionID { get; set; }
        public string? SourceOrderID { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        // Navigation property (not mapped to database)
        public List<TransactionLineDTO>? TransactionLine { get; set; }
    }
}
