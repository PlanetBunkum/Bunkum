using Bunkum.Core.Configuration;

namespace Bunkum.Protocols.Gopher.Responses.Items;

public class GophermapLink : GophermapItem
{
    public GophermapLink(string displayText, Uri destination) : this(GophermapItemType.Directory, displayText, destination)
    {}
    
    public GophermapLink(char itemType, string displayText, Uri destination)
    {
        this.ItemType = itemType;
        this.DisplayText = displayText;

        this.Hostname = destination.Host;
        this.Port = (ushort)destination.Port;
        this.Selector = destination.PathAndQuery;
    }
    
    public GophermapLink(string displayText, BunkumConfig config, string localEndpoint) : this(GophermapItemType.Directory, displayText, config, localEndpoint)
    {}
    
    public GophermapLink(char itemType, string displayText, BunkumConfig config, string localEndpoint)
    {
        this.ItemType = itemType;
        this.DisplayText = displayText;

        Uri uri = new(config.ExternalUrl);

        this.Hostname = uri.Host;
        this.Port = (ushort)uri.Port;
        this.Selector = localEndpoint;
    }
}