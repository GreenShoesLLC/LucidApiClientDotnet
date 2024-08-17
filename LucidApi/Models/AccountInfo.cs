using Newtonsoft.Json;
namespace LucidApi.Models
{
    public class AccountInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}