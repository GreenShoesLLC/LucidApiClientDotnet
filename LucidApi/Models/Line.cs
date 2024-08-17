using System.Collections.Generic;

namespace LucidApi.Models
{
    public class Line
    {
        public string Id { get; set; }
        public Endpoint Endpoint1 { get; set; }
        public Endpoint Endpoint2 { get; set; }
        public List<TextArea> TextAreas { get; set; }
        public List<DataPair> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
    }
}