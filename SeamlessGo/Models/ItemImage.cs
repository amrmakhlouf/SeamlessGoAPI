namespace SeamlessGo.Models
{
    public class ItemImage
    {
        public string ItemImageID { set; get; }
        public string ItemID { set; get; }
        public string FileName { get; set; } = null!;
        public bool IsPrimary { set; get; }
        public int SortOrder { set; get; }
        public DateTime LastModifiedUtc { get; set; }
    }
}
