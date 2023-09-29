namespace Bunkum.Core.Storage;

/// <summary>
/// A <see cref="IDataStore"/> that operates on the file system.
/// </summary>
/// <remarks>
/// This data store tries to protect against path traversal by default. You can disable this with a boolean in the constructor.
/// </remarks>
/// <seealso cref="BunkumFileSystem"/>
public class FileSystemDataStore : IDataStore
{
    private static readonly string DataStoreDirectory = Path.Combine(BunkumFileSystem.DataDirectory, "dataStore" + Path.DirectorySeparatorChar);
    /// <summary>
    /// Whether or not to protect against path traversal.
    /// These protections are enabled by default, but this can be disabled for (minuscule) performance improvements.
    /// </summary>
    private bool EnableTraversalSafety { get; init; } = true;

    /// <summary>
    /// Instantiates the DataStore. This constructor creates a dataStore directory if it does not exist.
    /// </summary>
    public FileSystemDataStore()
    {
        if (!Directory.Exists(DataStoreDirectory))
            Directory.CreateDirectory(DataStoreDirectory);
    }

    /// <summary>
    /// Instantiates the DataStore. This constructor creates a dataStore directory if it does not exist.
    /// </summary>
    /// <param name="enableTraversalSafety">Whether or not to protect against path traversal.</param>
    public FileSystemDataStore(bool enableTraversalSafety)
    {
        this.EnableTraversalSafety = enableTraversalSafety;
    }

    private string GetPath(string key)
    {
        if (!key.Contains('/')) return this.GetFullPathFromKey(key);
        
        // normalize file paths
        if (Path.DirectorySeparatorChar != '/') 
            key = key.Replace('/', Path.DirectorySeparatorChar);

        string[] dirs = key.Split(Path.DirectorySeparatorChar);
        string path = DataStoreDirectory;
        
        // create non-existing directory tree
        for (int i = 0; i < dirs.Length - 1; i++)
        {
            string dir = dirs[i];

            path = Path.Combine(path, dir);
            
            if(!Directory.Exists(path)) // checking beforehand avoids alloc
                Directory.CreateDirectory(path);
        }

        return this.GetFullPathFromKey(key);
    }

    private string GetFullPathFromKey(string key)
    {
        string path = DataStoreDirectory + key;
        
        // If traversal safety is disabled, then we just use the concatenated key
        if (!this.EnableTraversalSafety) return path;
        
        string fullPath = Path.GetFullPath(path); // Resolve the path
        if (!fullPath.StartsWith(DataStoreDirectory)) throw new FormatException("This key lands outside of the dataStore directory. It's likely this is path traversal.");

        return fullPath;
    }

    /// <inheritdoc />
    public bool ExistsInStore(string key) => File.Exists(this.GetPath(key));

    /// <inheritdoc />
    public bool WriteToStore(string key, byte[] data)
    {
        try
        {
            File.WriteAllBytes(this.GetPath(key), data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public byte[] GetDataFromStore(string key) => File.ReadAllBytes(this.GetPath(key));
    /// <inheritdoc />
    public bool RemoveFromStore(string key)
    {
        if (!this.ExistsInStore(key)) return false;
        
        File.Delete(this.GetPath(key));
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
            using FileStream fileStream = File.OpenWrite(this.GetPath(key));
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
        return File.OpenWrite(this.GetPath(key));
    }

    /// <inheritdoc />
    public Stream GetStreamFromStore(string key)
    {
        FileStream stream = File.OpenRead(this.GetPath(key));
        return stream;
    }
}