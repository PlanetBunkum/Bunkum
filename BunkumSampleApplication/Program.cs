// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Database.Dummy;
using Bunkum.Core.Storage;
using BunkumSampleApplication.Configuration;
using BunkumSampleApplication.Endpoints;
using BunkumSampleApplication.Middlewares;
using BunkumSampleApplication.Services;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;

// Initialize a Bunkum server
BunkumHttpServer server = new(new LoggerConfiguration
{
    Behaviour = new QueueLoggingBehaviour(),
    #if DEBUG
    MaxLevel = LogLevel.Trace,
    #else
    MaxLevel = LogLevel.Info,
    #endif
});

// The initialize function describes what services, middlewares, and endpoints are used for this server.
// You can technically run Add/Use methods outside of this, but it's recommended to keep them inside Initialize
// since Initialize is run upon hot reload.
server.Initialize = s =>
{
    // Discover all 'endpoints' from the assembly we're currently running - BunkumSampleApplication
    // This method looks for methods in the given extending EndpointGroup, and makes them routable to Bunkum.
    // If you've used ASP.NET before, Endpoints are essentially 
    s.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    
    // If for whatever reason you would like to add an endpoint group manually, you can do it with AddEndpointGroup like so.
    s.AddEndpointGroup<ManualEndpoints>();
    // Although, because we've run DiscoverEndpointsFromAssembly above, we've technically added it twice.
    // This call is purely for demonstration.
    
    // Middlewares are run right before endpoints are executed.
    // They can stop a request, add data to it, remove data, modify, send their own response, etc.
    // Here, we add a simple middleware that adds a header to all responses.
    s.AddMiddleware<AddHeaderMiddleware>();
    
    // Next, we add Database support via a provider.
    // A DatabaseProvider lazily provides a DatabaseContext object to endpoints that request one.
    // This is a dummy database that doesn't actually reach out to anything, just returns a simple value for testing.
    // Bunkum officially provides Bunkum.RealmDatabase to use Realm, but as of writing there are no other officially supported databases.
    // It's quite trivial to write your own Provider/Context classes, though. They're simple interfaces.
    s.UseDatabaseProvider(new DummyDatabaseProvider());
    
    // Then, we add a StorageService. This is Bunkum's abstraction for managing files, e.g. uploaded server assets like images.
    // This storage service stores files in memory, but you can change it to a FileSystemDataStore to store locally.
    // You can also extend IDataStore and create your own handler for storage, for example if you wanted to upload to S3.
    s.AddStorageService<InMemoryDataStore>();
    
    // Let's add some configuration. This is a built in helper function that uses Newtonsoft.JSON to load a config.
    // You can also do this with Config.LoadFromFile<TConfig>(filename).
    // This configuration is then exposed to endpoints and services. 
    s.AddConfigFromJsonFile<ExampleConfiguration>("example.json");
    
    // Finally, let's add a service. Services can do basically anything, including adding things for dependency injection into endpoints,
    // running code before middlewares, etc.
    s.AddService<TimeService>();
};

// Start the server in multi-threaded mode, and let Bunkum manage the rest.
server.Start();
await Task.Delay(-1);