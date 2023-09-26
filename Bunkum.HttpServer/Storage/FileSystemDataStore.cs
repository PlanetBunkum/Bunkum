namespace Bunkum.HttpServer.Storage;

/// <summary>
/// A <see cref="IDataStore"/> that operates on the file system.
/// </summary>
/// <seealso cref="BunkumFileSystem"/>
public class FileSystemDataStore : IDataStore
{
    private static readonly string DataStoreDirectory = Path.Combine(BunkumFileSystem.DataDirectory, "dataStore" + Path.DirectorySeparatorChar);

    /// <summary>
    /// Instantiates the DataStore. This constructor creates a dataStore directory if it does not exist.
    /// </summary>
    public FileSystemDataStore()
    {
        if (!Directory.Exists(DataStoreDirectory))
            Directory.CreateDirectory(DataStoreDirectory);
    }

    private static string GetPath(string key)
    {
        if (!key.Contains('/')) return DataStoreDirectory + key;
        
        if (Path.DirectorySeparatorChar != '/') 
            key = key.Replace('/', Path.DirectorySeparatorChar);

        string[] dirs = key.Split(Path.DirectorySeparatorChar);
        string path = DataStoreDirectory;
        for (int i = 0; i < dirs.Length - 1; i++)
        {
            string dir = dirs[i];

            path = Path.Combine(path, dir);
            
            if(!Directory.Exists(path)) // checking beforehand avoids alloc
                Directory.CreateDirectory(path);
        }

        return DataStoreDirectory + key;
    }

    /// <inheritdoc />
    public bool ExistsInStore(string key) => File.Exists(GetPath(key));

    /// <inheritdoc />
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

    /// <inheritdoc />
    public byte[] GetDataFromStore(string key) => File.ReadAllBytes(GetPath(key));
    /// <inheritdoc />
    public bool RemoveFromStore(string key)
    {
        if (!this.ExistsInStore(key)) return false;
        
        File.Delete(GetPath(key));
        return true;
    }

    /// <inheritdoc />
    public string[] GetKeysFromStore() =>
        Directory.GetFiles(DataStoreDirectory, "*", SearchOption.AllDirectories)
            .Select(key => key.Replace(DataStoreDirectory, string.Empty))
            .Select(key => key.Replace(Path.DirectorySeparatorChar, '/'))
            .ToArray();

    /// <inheritdoc />
    public bool WriteToStoreFromStream(string key, Stream data)
    {
        try
        {
            using FileStream fileStream = File.OpenWrite(GetPath(key));
            data.CopyTo(fileStream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Stream OpenWriteStream(string key)
    {
        return File.OpenWrite(GetPath(key));
    }

    /// <inheritdoc />
    public Stream GetStreamFromStore(string key)
    {
        FileStream stream = File.OpenRead(GetPath(key));
        return stream;
    }
}