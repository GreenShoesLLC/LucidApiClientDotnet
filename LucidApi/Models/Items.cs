using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Items
    {
        [JsonPropertyName("shapes")]
        public List<Shape>? Shapes { get; set; }

        [JsonPropertyName("lines")]
        public List<Line>? Lines { get; set; }

        [JsonPropertyName("groups")]
        public List<Group>? Groups { get; set; }

        [JsonPropertyName("layers")]
        public List<Layer>? Layers { get; set; }
    }
}