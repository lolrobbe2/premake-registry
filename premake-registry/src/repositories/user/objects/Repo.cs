using Newtonsoft.Json;

namespace premake.repositories.user.objects
{
    public class UserRepo
    {
        [JsonProperty("name")]
        public string name {  get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
        [JsonProperty("open_issues")]
        public int issues { get; set; }
    }
}
