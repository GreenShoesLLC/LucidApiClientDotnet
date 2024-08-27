using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class HierarchicalDropdownValue
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}