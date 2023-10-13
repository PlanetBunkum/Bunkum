using Bunkum.Core;
using Bunkum.Core.Configuration;

namespace Bunkum.Protocols.Gopher.Responses.Items;

public class GophermapLink : GophermapItem
{
    public GophermapLink(string displayText, Uri destination)
    {
        this.ItemType = GophermapItemType.Directory;
        this.DisplayText = displayText;

        this.Hostname = destination.Host;
        this.Port = (ushort)destination.Port;
        this.Selector = destination.PathAndQuery;
    }
    
    public GophermapLink(string displayText, BunkumConfig config, string localEndpoint)
    {
        this.ItemType = GophermapItemType.Directory;
        this.DisplayText = displayText;

        Uri uri = new(config.ExternalUrl);

        this.Hostname = uri.Host;
        this.Port = (ushort)uri.Port;
        this.Selector = localEndpoint;
    }
}