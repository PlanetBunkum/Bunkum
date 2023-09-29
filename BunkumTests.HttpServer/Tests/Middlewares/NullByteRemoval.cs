using System.Xml.Serialization;
using Bunkum.Core;
using BunkumTests.HttpServer.Endpoints;
using Newtonsoft.Json;

namespace BunkumTests.HttpServer.Tests.Middlewares;

public class NullByteRemoval : ServerDependentTest
{
    [Test]
    public async Task TestNullByteRemovalString()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();

        HttpRequestMessage request = new(HttpMethod.Post, "/body/string");
        request.Content = new StringContent("BeforeNull\0AfterNull\0\0");
        
        HttpResponseMessage msg = await client.SendAsync(request);
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("BeforeNull"));
        });
    }
    
    [Test]
    public async Task TestNullByteRemovalJson()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();

        BodyEndpoints.Serializable obj = new()
        {
            Field = "Test",
        };

        HttpRequestMessage request = new(HttpMethod.Post, "/body/json");
        request.Content = new StringContent(JsonConvert.SerializeObject(obj) + "\0TEST");
        
        HttpResponseMessage msg = await client.SendAsync(request);
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            Assert.That(JsonConvert.DeserializeObject<BodyEndpoints.Serializable>(await msg.Content.ReadAsStringAsync())!.Field, Is.EqualTo("Test"));
        });
    }
    
    [Test]
    public async Task TestNullByteRemovalXml()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();

        BodyEndpoints.Serializable obj = new()
        {
            Field = "Test",
        };

        XmlSerializer serializer = new(typeof(BodyEndpoints.Serializable));
        StringWriter stringWriter = new();
        serializer.Serialize(stringWriter, obj);
        
        HttpRequestMessage request = new(HttpMethod.Post, "/body/xml");
        request.Content = new StringContent(stringWriter + "\0TEST");
        
        HttpResponseMessage msg = await client.SendAsync(request);
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            Assert.That(((BodyEndpoints.Serializable)serializer.Deserialize(new StringReader(await msg.Content.ReadAsStringAsync()))!).Field, Is.EqualTo("Test"));
        });
    }
    
    [Test]
    public async Task TestNullByteRemovalByteArray()
    {
        (BunkumServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();

        const string str = "BeforeNull\0AfterNull\0\0";
        
        HttpRequestMessage request = new(HttpMethod.Post, "/body/byteArray");
        request.Content = new StringContent(str);
        
        HttpResponseMessage msg = await client.SendAsync(request);
        Assert.Multiple(async () =>
        {
            Assert.That(msg.IsSuccessStatusCode, Is.True);
            //Byte arrays shouldn't have null bytes stripped
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(str));
        });
    }
}