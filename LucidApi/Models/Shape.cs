using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Shape
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("textAreas")]
        public List<TextArea>? TextAreas { get; set; }

        [JsonPropertyName("customData")]
        public List<DataPair>? CustomData { get; set; }

        [JsonPropertyName("linkedData")]
        public List<LinkedData>? LinkedData { get; set; }
    }
}