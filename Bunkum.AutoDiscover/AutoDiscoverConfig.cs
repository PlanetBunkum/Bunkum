namespace Bunkum.AutoDiscover;

/// <summary>
/// A configuration for AutoDiscover
/// </summary>
public struct AutoDiscoverConfig
{
    /// <summary>
    /// A friendly name of the server.
    /// </summary>
    public readonly string ServerBrand;
    /// <summary>
    /// An optional friendly description of the server.
    /// </summary>
    public readonly string ServerDescription;
    /// <summary>
    /// The default endpoint to patch clients to.
    /// </summary>
    public readonly string? BaseEndpoint;

    /// <summary>
    /// A method that determines the endpoint to patch clients to.
    /// </summary>
    public readonly AutoDiscoverDelegate? UrlDelegate;
    /// <summary>
    /// An optional URL to the image that the client may retrieve.
    /// </summary>
    public readonly string? BannerImageUrl;
    /// <summary>
    /// An optional boolean, when true it represents that the LBP server will only accept the digest key CustomServerDigest.
    /// </summary>
    public readonly bool UsesCustomDigestKey;

    /// <summary>
    /// Instantiates an AutoDiscoverConfig.
    /// </summary>
    public AutoDiscoverConfig(string serverBrand, string baseEndpoint, bool usesCustomDigestKey, string serverDescription = "", string? bannerImageUrl = null)
    {
        this.ServerBrand = serverBrand;
        this.BaseEndpoint = baseEndpoint;
        this.UsesCustomDigestKey = usesCustomDigestKey;
        
        this.ServerDescription = serverDescription;
        this.BannerImageUrl = bannerImageUrl;
    }
    
    /// <summary>
    /// Instantiates an AutoDiscoverConfig.
    /// </summary>
    public AutoDiscoverConfig(string serverBrand, AutoDiscoverDelegate urlDelegate, bool usesCustomDigestKey, string serverDescription = "", string? bannerImageUrl = null)
    {
        this.ServerBrand = serverBrand;
        this.UrlDelegate = urlDelegate;
        this.UsesCustomDigestKey = usesCustomDigestKey;
        
        this.ServerDescription = serverDescription;
        this.BannerImageUrl = bannerImageUrl;
    }
}