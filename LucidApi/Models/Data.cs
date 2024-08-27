using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LucidApi.Models
{
    public class Data
    {
        [JsonPropertyName("collections")]
        public List<Dictionary<string, object>>? Collections { get; set; }
    }
}