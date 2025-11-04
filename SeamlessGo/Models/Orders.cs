namespace SeamlessGo.Models
{
    public class Orders
    {
        public string? OrderID { get; set; }
        public string? CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? OrderTypeID { get; set; }
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
        public string? InvoicedID { get; set; }
        public int? SyncStatus { get; set; }

        // Navigation property (not mapped to database)
        public List<OrderLines>? OrderLines { get; set; }
    }
}
