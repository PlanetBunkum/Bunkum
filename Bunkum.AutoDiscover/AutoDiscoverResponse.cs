using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bunkum.AutoDiscover;

[JsonObject(MemberSerialization.OptIn)]
internal class AutoDiscoverResponse
{
    private const int CurrentVersion = 3;

    [JsonProperty("version")]
    public int Version { get; set; } = CurrentVersion;

    [JsonProperty("serverBrand")]
    public required string ServerBrand { get; set; }
    
    [JsonProperty("serverDescription")]
    public string ServerDescription { get; set; } = "";
        
    [JsonProperty("url")]
    public required string Url { get; set; }

    [JsonProperty("bannerImageUrl")]
    public string? BannerImageUrl { get; set; }

    [JsonProperty("usesCustomDigestKey")]
    public required bool UsesCustomDigestKey { get; set; }
}