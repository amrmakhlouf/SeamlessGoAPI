namespace SeamlessGo.DTOs
{
    public class PaymentAllocationsDTO
    {
        public string? TransactionID { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public int? SyncStatus { get; set; }
    }
}
