namespace Bunkum.Protocols.Gopher.Responses;

public class GophermapItem
{
    public GophermapItemType ItemType { get; init; } = GophermapItemType.File;
    public string DisplayText { get; init; } = "";
    public string Selector { get; set; } = "fake";
    public string Hostname { get; set; } = "(NULL)";
    public ushort Port { get; set; } = 0;
}