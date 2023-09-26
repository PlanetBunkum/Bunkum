namespace Bunkum.HttpServer.Storage;

public class InMemoryDataStore : IDataStore
{
    private readonly Dictionary<string, byte[]> _data = new();

    /// <inheritdoc />
    public bool ExistsInStore(string key) => this._data.ContainsKey(key);
    /// <inheritdoc />
    public bool RemoveFromStore(string key) => this._data.Remove(key);
    /// <inheritdoc />
    public string[] GetKeysFromStore() => this._data.Keys.ToArray();

    /// <inheritdoc />
    public bool WriteToStore(string key, byte[] data)
    {
        try
        {
            this._data.Remove(key);
            this._data.Add(key, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public byte[] GetDataFromStore(string key)
    {
        return this._data[key];
    }

    /// <inheritdoc />
    public bool WriteToStoreFromStream(string key, Stream data)
    {
        using MemoryStream ms = new();
        data.CopyTo(data);
        return this.WriteToStore(key, ms.ToArray());
    }

    /// <inheritdoc />
    public Stream GetStreamFromStore(string key)
    {
        byte[] data = this.GetDataFromStore(key);
        return new MemoryStream(data);
    }

    /// <inheritdoc />
    public Stream OpenWriteStream(string key)
    {
        throw new NotImplementedException();
    }
}