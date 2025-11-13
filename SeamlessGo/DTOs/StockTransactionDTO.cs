using SeamlessGo.Models;

namespace SeamlessGo.DTOs
{
    public class StockTransactionDTO
    {
        public string? StockTransactionID { get; set; }
        public int? SupplierID { get; set; }
        public int? SourceLocationID { get; set; }
        public int? DeliveryLocationID { get; set; }
        public int? Status { get; set; }
        public int? StockTransactionTypeID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public decimal? TotalQuantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Note { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? TotalRemainingAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public float? DiscountPerc { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? ImportDuty { get; set; }
        public int? RouteID { get; set; }
        public int? CreatedByUserID { get; set; }
        public int? SyncStatus { get; set; }
        public DateTime? LastModifiedUtc { get; set; }
        public List<StockTransactionLinesDTO>? StockTransactionLine { get; set; }
    }
}
