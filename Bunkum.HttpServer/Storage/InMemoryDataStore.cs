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
}