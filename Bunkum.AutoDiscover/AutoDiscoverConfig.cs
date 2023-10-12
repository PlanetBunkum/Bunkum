namespace Bunkum.AutoDiscover;

internal struct AutoDiscoverConfig
{
    internal readonly string ServerBrand;
    internal readonly string ServerDescription;
    internal readonly string BaseEndpoint;
    internal readonly string? BannerImageUrl;
    internal readonly bool UsesCustomDigestKey;

    internal AutoDiscoverConfig(string serverBrand, string baseEndpoint, bool usesCustomDigestKey, string serverDescription = "", string? bannerImageUrl = null)
    {
        this.ServerBrand = serverBrand;
        this.BaseEndpoint = baseEndpoint;
        this.UsesCustomDigestKey = usesCustomDigestKey;
        
        this.ServerDescription = serverDescription;
        this.BannerImageUrl = bannerImageUrl;
    }
}