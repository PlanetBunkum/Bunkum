using Newtonsoft.Json;

namespace Bunkum.AutoDiscover;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
internal class AutoDiscoverResponse
{
    private const int CurrentVersion = 2;

    [JsonProperty("version")]
    public int Version { get; set; } = CurrentVersion;

    [JsonProperty("serverBrand")]
    public string ServerBrand { get; set; }
        
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("usesCustomDigestKey")]
    public bool UsesCustomDigestKey { get; set; }
}