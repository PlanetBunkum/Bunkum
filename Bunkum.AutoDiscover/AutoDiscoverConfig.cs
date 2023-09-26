namespace Bunkum.AutoDiscover;

internal struct AutoDiscoverConfig
{
    internal readonly string ServerBrand;
    internal readonly string BaseEndpoint;
    internal readonly bool UsesCustomDigestKey;

    internal AutoDiscoverConfig(string serverBrand, string baseEndpoint, bool usesCustomDigestKey)
    {
        this.ServerBrand = serverBrand;
        this.BaseEndpoint = baseEndpoint;
        this.UsesCustomDigestKey = usesCustomDigestKey;
    }
}