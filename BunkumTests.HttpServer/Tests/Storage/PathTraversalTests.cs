using Bunkum.HttpServer.Storage;

namespace BunkumTests.HttpServer.Tests.Storage;

public class PathTraversalTests
{
    [Test]
    public void CantTraverseUsingForwardSlash()
    {
        IDataStore dataStore = new FileSystemDataStore();
        bool result = dataStore.TryGetDataFromStore("../bunkum.json", out byte[]? _);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void CantTraverseUsingBackSlash()
    {
        IDataStore dataStore = new FileSystemDataStore();
        bool result = dataStore.TryGetDataFromStore("..\\bunkum.json", out byte[]? _);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void CantTraverseUsingHexCode()
    {
        IDataStore dataStore = new FileSystemDataStore();
        bool result = dataStore.TryGetDataFromStore("..%2Fbunkum.json", out byte[]? _);
        Assert.That(result, Is.False);
    }
}