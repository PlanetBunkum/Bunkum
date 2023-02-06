namespace Bunkum.HttpServer;

internal static class BunkumFileSystem
{
    internal static readonly string DataDirectory;
    internal static readonly bool UsingCustomDirectory;
    
    static BunkumFileSystem()
    {
        string? path = Environment.GetEnvironmentVariable("BUNKUM_DATA_FOLDER");
        
        UsingCustomDirectory = path != null;
        path ??= Environment.CurrentDirectory;

        DataDirectory = path;
        
        // Create directory if it doesnt exist already
        if (UsingCustomDirectory && !Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }
}