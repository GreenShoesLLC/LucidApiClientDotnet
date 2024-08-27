using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class DocumentContent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("product")]
        public Product? Product { get; set; }

        [JsonPropertyName("pages")]
        public List<Page>? Pages { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }
    }
}