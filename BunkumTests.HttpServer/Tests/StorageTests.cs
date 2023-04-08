using System.Text;
using Bunkum.HttpServer.Storage;

namespace BunkumTests.HttpServer.Tests;

#pragma warning disable NUnit2045

[NonParallelizable]
[TestFixture(typeof(InMemoryDataStore))]
[TestFixture(typeof(FileSystemDataStore))]
public class StorageTests
{
    private readonly IDataStore _dataStore;
    private readonly byte[] _value;
    
    public StorageTests(Type dataStoreType)
    {
        this._dataStore = (IDataStore)Activator.CreateInstance(dataStoreType)!;
        this._value = Encoding.Default.GetBytes("value");
    }
    
    
    [Test]
    public void DataStoreSystemWorks()
    {
        Assert.That(this._dataStore.ExistsInStore("key"), Is.False);
        Assert.That(() => this._dataStore.GetDataFromStore("key"), Throws.Exception);
        
        Assert.That(this._dataStore.WriteToStore("key", this._value), Is.True);
        
        Assert.That(this._dataStore.ExistsInStore("key"), Is.True);
        Assert.That(this._dataStore.GetDataFromStore("key"), Is.EqualTo(this._value));
        
        Assert.That(this._dataStore.RemoveFromStore("key"), Is.True);
        Assert.That(this._dataStore.ExistsInStore("key"), Is.False);
    }

    [Test]
    [TestCase("dir/")]
    [TestCase("dir/sub/")]
    public void CanCreateAndReadDirectories(string dir)
    {
        string key = dir + "a";
        
        Assert.That(this._dataStore.ExistsInStore(key), Is.False);
        Assert.That(() => this._dataStore.GetDataFromStore(key), Throws.Exception);
        
        Assert.That(this._dataStore.WriteToStore(key, this._value), Is.True);
        
        Assert.That(this._dataStore.ExistsInStore(key), Is.True);
        Assert.That(this._dataStore.ExistsInStore(dir), Is.False);
        
        Assert.That(this._dataStore.GetDataFromStore(key), Is.EqualTo(this._value));
        
        Assert.That(this._dataStore.RemoveFromStore(key), Is.True);
        Assert.That(this._dataStore.ExistsInStore(key), Is.False);
    }
}