namespace SeamlessGo.DTOs
{
    public class CreateItemPackDTO
    {
        public string ItemPackID { get; set; }  
        public float? Equivalency { get; set; }
        public bool? IsWeightable { get; set; }
        public int? UnitID { get; set; }
        public string? BarCode { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public bool? Enabled { get; set; }
        public string? ImagePath { get; set; }
    }
}
