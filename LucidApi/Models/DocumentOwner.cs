using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class DocumentOwner
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}