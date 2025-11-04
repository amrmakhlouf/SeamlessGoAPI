namespace SeamlessGo.DTOs
{
    public class CreateTransactionLineDTO
    {
        public string TransactionLineID { get; set; }
        public string? ItemPackID { get; set; }
        public decimal? DiscountAmount { get; set; }
        public float? DiscountPerc { get; set; }
        public decimal? Bonus { get; set; }
        public int? Quantity { get; set; }
        public decimal? ItemPrice { get; set; }
        public decimal? FullPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Note { get; set; }
    }
}
