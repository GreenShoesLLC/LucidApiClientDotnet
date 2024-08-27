using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class DocumentSearchParameters
    {
        [JsonPropertyName("product")]
        public List<string> Product { get; set; } = new List<string> { "lucidchart" };

        [JsonPropertyName("createdStartTime")]
        public DateTime? CreatedStartTime { get; set; }

        [JsonPropertyName("createdEndTime")]
        public DateTime? CreatedEndTime { get; set; }

        [JsonPropertyName("lastModifiedAfter")]
        public DateTime? LastModifiedAfter { get; set; }

        [JsonPropertyName("keywords")]
        public string? Keywords { get; set; }
    }
}