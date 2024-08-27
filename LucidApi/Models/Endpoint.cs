using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Endpoint
    {
        [JsonPropertyName("style")]
        public string? Style { get; set; }

        [JsonPropertyName("connectedTo")]
        public string? ConnectedTo { get; set; }
    }
}