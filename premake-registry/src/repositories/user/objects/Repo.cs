using Google.Cloud.Firestore;
using Newtonsoft.Json;
#nullable enable
namespace premake.repositories.user.objects
{
    public class Owner
    {
        [JsonProperty("login")]
        public required string login { get; set; }
        [JsonProperty("avatar_url")]
        public required string avatar_url { get; set; }
        [JsonProperty("html_url")]
        public required string html_url { get; set; }
    }
    public class UserRepo
    {
        [JsonProperty("name")]
        public required string name {  get; set; }
        [JsonProperty("full_name")]
        public required string full_name { get; set; }
        [JsonProperty("url")]
        public required string url { get; set; }
        [JsonProperty("open_issues")]
        public int issues { get; set; }
        [JsonProperty("description")]
        public string? description { get; set; }
        [JsonProperty("license")]
        public License? license { get; set; }

        [JsonProperty("html_url")]
        public required string html_url { get; set; }
        public Owner owner { get; set; }
    }
}
