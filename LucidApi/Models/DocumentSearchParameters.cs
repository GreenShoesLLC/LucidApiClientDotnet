namespace LucidApi.Models
{
    public class DocumentSearchParameters
    {
        // public List<string> Product { get; set; } = new List<string> { "lucidchart", "lucidscale", "lucidspark" };
        public List<string> Product { get; set; } = new List<string> { "lucidchart" };
        public DateTime? CreatedStartTime { get; set; }
        public DateTime? CreatedEndTime { get; set; }
        public DateTime? LastModifiedAfter { get; set; }
        public string Keywords { get; set; }
    }
}