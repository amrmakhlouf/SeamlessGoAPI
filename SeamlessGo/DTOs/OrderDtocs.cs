namespace SeamlessGo.DTOs
{
    public class OrderDtocs
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
        public DateTime? LastModifiedUtc { get; set; }


        // Include order lines in response
        public List<OrderLineDto>? OrderLines { get; set; }
    }
}
