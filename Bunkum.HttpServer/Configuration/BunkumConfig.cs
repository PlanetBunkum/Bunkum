namespace Bunkum.HttpServer.Configuration;

public class BunkumConfig : Config
{
    public override int CurrentConfigVersion => 2;
    public override int Version { get; set; }
    protected internal override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }
    
    public string ExternalUrl { get; set; } = "http://127.0.0.1:10061";

    public string ListenHost { get; set; } = "0.0.0.0";
    public int ListenPort { get; set; } = 10061;
}