namespace Bunkum.HttpServer.Storage;

public interface IDataStore
{
    /// <summary>
    /// Checks if the key exists in the data store.
    /// </summary>
    /// <param name="key">The key to be checked.</param>
    /// <returns>true when the key is stored in the data store, false if it does not exist.</returns>
    bool ExistsInStore(string key);
    /// <summary>
    /// Writes the given byte array to the data store.
    /// </summary>
    /// <param name="key">The key to be written to.</param>
    /// <param name="data">The data to be written.</param>
    /// <returns>true if the write operation was successful, false if not.</returns>
    bool WriteToStore(string key, byte[] data);
    /// <summary>
    /// Writes the given span to the data store.
    /// </summary>
    /// <param name="key">The key to be written to.</param>
    /// <param name="data">The data to be written.</param>
    void WriteToStore(string key, ReadOnlySpan<byte> data) => this.WriteToStore(key, data.ToArray());
    /// <summary>
    /// Retrieves a byte array from the data store using a key. 
    /// </summary>
    /// <param name="key">The key to be read from.</param>
    /// <returns>The data from the data store.</returns>
    byte[] GetDataFromStore(string key);
    /// <summary>
    /// Removes a key and its data from the data store.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>true if the removal was successful, false if not.</returns>
    bool RemoveFromStore(string key);
    /// <summary>
    /// Gets a list of all keys from the data store.
    /// </summary>
    /// <returns>A string array containing every key in the store.</returns>
    string[] GetKeysFromStore();
    
    /// <summary>
    /// Writes the given Stream to the data store.
    /// </summary>
    /// <param name="key">The key to be written to.</param>
    /// <param name="data">The data to be written.</param>
    /// <returns>true if the write operation was successful, false if not.</returns>
    bool WriteToStoreFromStream(string key, Stream data);
    /// <summary>
    /// Retrieves a Stream from the data store using a key. You are responsible for closing this stream. 
    /// </summary>
    /// <param name="key">The key to be read from.</param>
    /// <returns>The data from the data store.</returns>
    Stream GetStreamFromStore(string key);
    /// <summary>
    /// Opens a Stream from the data store using a key. You are responsible for closing this stream. 
    /// </summary>
    /// <param name="key">The key to write to.</param>
    /// <returns>A stream in the data store from the key.</returns>
    Stream OpenWriteStream(string key);

    /// <summary>
    /// Attempts to retrieve data from the data store.
    /// </summary>
    /// <param name="key">The key to be read from.</param>
    /// <param name="data">The data from the data store. If null, the read operation was unsuccessful.</param>
    /// <returns>true if the data was read, false if not.</returns>
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