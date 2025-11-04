namespace SeamlessGo.DTOs
{
    public class StockTransactionLinesDTO
    {
        public string StockTransactionLineID { get; set; }
        public string? StockTransactionID { get; set; }
        public string? ItemPackID { get; set; }
        public int? Quantity { get; set; }
        public decimal? Coast { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
