namespace SeamlessGo.Models
{
    public class Sequence
    {
        public int UserID { get; set; }
        public string TableName { get; set; }
        public string TablePrefix { get; set; }
        public int LastSequence { get; set; }
    }
}
