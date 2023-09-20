namespace Bunkum.AutoDiscover;

public struct AutoDiscoverConfig
{
    public readonly string ServerBrand;
    public readonly string BaseEndpoint;
    public readonly bool UsesCustomDigestKey;

    public AutoDiscoverConfig(string serverBrand, string baseEndpoint, bool usesCustomDigestKey)
    {
        ServerBrand = serverBrand;
        BaseEndpoint = baseEndpoint;
        UsesCustomDigestKey = usesCustomDigestKey;
    }
}