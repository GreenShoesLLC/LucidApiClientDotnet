using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Line
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("endpoint1")]
        public Endpoint? Endpoint1 { get; set; }

        [JsonPropertyName("endpoint2")]
        public Endpoint? Endpoint2 { get; set; }

        [JsonPropertyName("textAreas")]
        public List<TextArea>? TextAreas { get; set; }

        [JsonPropertyName("customData")]
        public List<DataPair>? CustomData { get; set; }

        [JsonPropertyName("linkedData")]
        public List<LinkedData>? LinkedData { get; set; }
    }
}