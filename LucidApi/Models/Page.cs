using System.Collections.Generic;

namespace LucidApi.Models
{
    public class Page
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public Items Items { get; set; }
        public List<DataPair>? CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
    }
}