namespace Bunkum.HttpServer.Storage;

public class FileSystemDataStore : IDataStore
{
    private static readonly string DataStoreDirectory = Path.Combine(BunkumFileSystem.DataDirectory, "dataStore" + Path.DirectorySeparatorChar);

    public FileSystemDataStore()
    {
        if (!Directory.Exists(DataStoreDirectory))
            Directory.CreateDirectory(DataStoreDirectory);
    }

    private static string GetPath(string key)
    {
        key = key.Replace('/', Path.DirectorySeparatorChar);

        string[] dirs = key.Split(Path.DirectorySeparatorChar);
        string path = DataStoreDirectory;
        for (int i = 0; i < dirs.Length - 1; i++)
        {
            string dir = dirs[i];

            path = Path.Combine(path, dir);
            Directory.CreateDirectory(path);
        }

        return DataStoreDirectory + key;
    }
    
    public bool ExistsInStore(string key) => File.Exists(GetPath(key));

    public bool WriteToStore(string key, byte[] data)
    {
        try
        {
            File.WriteAllBytes(GetPath(key), data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public byte[] GetDataFromStore(string key) => File.ReadAllBytes(GetPath(key));
    public bool RemoveFromStore(string key)
    {
        if (!this.ExistsInStore(key)) return false;
        
        File.Delete(GetPath(key));
        return true;

    }
}