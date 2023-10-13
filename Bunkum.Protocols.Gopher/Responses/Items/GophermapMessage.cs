namespace Bunkum.Protocols.Gopher.Responses.Items;

public class GophermapMessage : GophermapItem
{
    public GophermapMessage(string message)
    {
        this.ItemType = GophermapItemType.Message;
        this.DisplayText = message;
    }
}