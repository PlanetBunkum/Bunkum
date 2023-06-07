namespace Bunkum.HttpServer.Storage;

public interface IDataStore
{
    bool ExistsInStore(string key);
    bool WriteToStore(string key, byte[] data);
    void WriteToStore(string key, ReadOnlySpan<byte> data) => this.WriteToStore(key, data.ToArray());
    byte[] GetDataFromStore(string key);
    bool RemoveFromStore(string key);
    string[] GetKeysFromStore();

    bool TryGetDataFromStore(string key, out byte[]? data)
    {
        try
        {
            if (this.ExistsInStore(key))
            {
                data = this.GetDataFromStore(key);
                return true;
            }
        }
        catch
        {
            data = null;
            return false;
        }

        data = null;
        return false;
    }
}