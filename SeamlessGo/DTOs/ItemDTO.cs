namespace SeamlessGo.DTOs
{
    public class ItemDTO
    {
        public string ItemID { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public int? CategoryID { get; set; }
        public int? SupplierID { get; set; }
        public int? BrandID { get; set; }
        public int? TaxID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVoided { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ItemTypeID { get; set; }
        public bool? IsStockTracked { get; set; }
        public List<ItemPackDTO>? ItemPacks { get; set; }
    }
}
