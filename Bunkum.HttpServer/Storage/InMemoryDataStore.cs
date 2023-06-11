namespace Bunkum.HttpServer.Storage;

public class InMemoryDataStore : IDataStore
{
    private readonly Dictionary<string, byte[]> _data = new();

    public bool ExistsInStore(string key) => this._data.ContainsKey(key);
    public bool RemoveFromStore(string key) => this._data.Remove(key);
    public string[] GetKeysFromStore() => this._data.Keys.ToArray();

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

    public byte[] GetDataFromStore(string key)
    {
        return this._data[key];
    }

    public bool WriteToStoreFromStream(string key, Stream data)
    {
        using MemoryStream ms = new();
        data.CopyTo(data);
        return this.WriteToStore(key, ms.ToArray());
    }

    public Stream GetStreamFromStore(string key)
    {
        byte[] data = this.GetDataFromStore(key);
        return new MemoryStream(data);
    }

    public Stream OpenWriteStream(string key)
    {
        throw new NotImplementedException();
    }
}