using Bunkum.Core;
using Bunkum.Core.Database.Dummy;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using BunkumSampleApplication.Configuration;
using BunkumSampleApplication.Models;

namespace BunkumSampleApplication.Endpoints;

public class WeatherEndpoints : EndpointGroup
{
    [HttpEndpoint("/api/v1/weather", HttpMethods.Get, ContentType.Json)]
    // This is a simple endpoint - the bare minimum.
    // The only requirement of an endpoint is that it is marked with an [HttpEndpoint] attribute and the first argument is a RequestContext.
    public WeatherStatistic GetWeather(RequestContext context)
    {
        // Since we've marked our ContentType as Json, Bunkum will automatically serialize this object with Newtonsoft.JSON if we return it.
        // Unlike ASP.NET, you can return anything for any content type.
        // If you would like to return a string here despite the JSON content type, you're more than welcome to do so.
        // Bunkum tries to let you do anything, even if it's not exactly standard compliant.
        return new WeatherStatistic
        {
            Time = DateTimeOffset.Now,
            Temperature = 72,
            Description = "It's hot today!",
        };
    }
    
    // Let's introduce some more Bunkum concepts.
    [HttpEndpoint("/api/v2/weather", HttpMethods.Get, ContentType.Json)]
    // Here, we inject the ExampleConfiguration we set up in Program.cs.
    // It can be named anything, just as long as the type matches.
    public WeatherStatistic GetWeatherV2(RequestContext context, ExampleConfiguration configuration)
    {
        return new WeatherStatistic
        {
            Time = DateTimeOffset.Now,
            Temperature = 72,
            Description = configuration.ExampleVariable,
        };
    }
    
    [HttpEndpoint("/api/v3/weather", HttpMethods.Get, ContentType.Json)]
    // We can do this for just about anything we set up in Program.cs. Let's get crazy...
    public WeatherStatistic GetWeatherV3(RequestContext context,
        ExampleConfiguration configuration,
        DummyDatabaseContext database,
        IDataStore dataStore, // For IDataStore, it's important to use the interface. Otherwise, you can't swap it out easily.
        DateTimeOffset timeFromTimeService)
    {
        dataStore.WriteToStore($"request-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}", "I'm a span with data"u8);
        return new WeatherStatistic
        {
            Time = timeFromTimeService,
            Temperature = database.GetDummyValue(),
            Description = configuration.ExampleVariable,
        };
    }
    
    // You may have noticed the "ContentType.Json" parameter in the endpoint attribute.
    // Bunkum allows for powerful control over serialization. You can create your own serializers via creating an IBunkumSerializer.
    // Let's specify the XML content type, which by default uses Bunkum's own BunkumXmlSerializer.
    // Similarly, specifying ContentType.Json would use BunkumJsonSerializer by default.
    // You can remove the default ones and add your own through the BunkumServer.
    [HttpEndpoint("/api/xml/weather", HttpMethods.Get, ContentType.Xml)]
    public WeatherStatistic GetWeatherXml(RequestContext context)
    {
        return new WeatherStatistic
        {
            Time = DateTimeOffset.Now,
            Temperature = 72,
            Description = "It's hot today!",
        };
    }
}