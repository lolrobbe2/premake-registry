using System.Text.Json.Serialization;

namespace premake.repositories.user.objects
{

    public class License
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("spdx_id")]
        public string SpdxId { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; } = string.Empty;
    }
}
