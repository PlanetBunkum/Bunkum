namespace Bunkum.Protocols.Gopher.Responses;

public class GophermapItem
{
    public required char ItemType { get; set; }
    public required string DisplayString { get; set; }
    public string Selector { get; set; } = "fake";
    public string Hostname { get; set; } = "(NULL)";
    public ushort Port { get; set; } = 0;
}