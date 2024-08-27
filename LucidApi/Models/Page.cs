using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Page
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("items")]
        public Items? Items { get; set; }

        [JsonPropertyName("customData")]
        public List<DataPair>? CustomData { get; set; }

        [JsonPropertyName("linkedData")]
        public List<LinkedData>? LinkedData { get; set; }
    }
}