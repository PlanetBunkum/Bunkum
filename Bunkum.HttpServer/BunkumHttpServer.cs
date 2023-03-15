using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Bunkum.CustomHttpListener;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Database.Dummy;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Endpoints.Middlewares;
using Bunkum.HttpServer.Extensions;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace Bunkum.HttpServer;

public class BunkumHttpServer
{
    private readonly BunkumHttpListener _listener;
    private readonly List<EndpointGroup> _endpoints = new();
    private readonly LoggerContainer<BunkumContext> _logger;

    private IAuthenticationProvider<IUser> _authenticationProvider = new DummyAuthenticationProvider();
    private IDatabaseProvider<IDatabaseContext> _databaseProvider = new DummyDatabaseProvider();
    private IDataStore _dataStore = new NullDataStore();
    
    private Config? _config;
    private Type? _configType;
    private readonly BunkumConfig _bunkumConfig;

    // ReSharper disable UnassignedField.Global
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public bool AssumeAuthenticationRequired = false;
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore FieldCanBeMadeReadOnly.Global

    private readonly List<IMiddleware> _middlewares = new();

    public BunkumHttpServer(Uri? listenEndpoint = null)
    {
        this._logger = new LoggerContainer<BunkumContext>();
        this._logger.RegisterLogger(new ConsoleLogger());
        
        this._logger.LogInfo(BunkumContext.Startup, $"Bunkum is storing its data at {BunkumFileSystem.DataDirectory}.");
        if (!BunkumFileSystem.UsingCustomDirectory)
        {
            this._logger.LogInfo(BunkumContext.Startup, "You can override where data is stored using the BUNKUM_DATA_FOLDER environment variable.");
        }

        this._bunkumConfig = Config.LoadFromFile<BunkumConfig>("bunkum.json", this._logger);

        if (listenEndpoint != null)
        {
            this._logger.LogDebug(BunkumContext.Startup, $"Using hardcoded listen endpoint {listenEndpoint}");
        }
        else
        {
            listenEndpoint = new Uri($"http://{this._bunkumConfig.ListenHost}:{this._bunkumConfig.ListenPort}");
        }
        
        this._listener = new BunkumHttpListener(listenEndpoint);
    }

    public void Start()
    {
        this.RunStartupTasks();
        Task.Factory.StartNew(async () => await this.Block());
    }
    
    public async Task StartAndBlockAsync()
    {
        this.RunStartupTasks();
        await this.Block();
    }

    private void RunStartupTasks()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        this._logger.LogInfo(BunkumContext.Startup, "Starting up...");
        if (this._authenticationProvider is DummyAuthenticationProvider && this.AssumeAuthenticationRequired)
        {
            this._logger.LogWarning(BunkumContext.Startup, "The server was started with a dummy authentication provider. " +
                                                            "If your application relies on authentication, users will always have full access.");
        }
        
        this._logger.LogDebug(BunkumContext.Startup, "Initializing database provider...");
        this._databaseProvider.Initialize();
        
        this._logger.LogDebug(BunkumContext.Startup, "Starting listener...");
        try
        {
            this._listener.StartListening();
        }
        catch(Exception e)
        {
            this._logger.LogCritical(BunkumContext.Startup, $"An exception occured while trying to start the listener: \n{e}");
            this._logger.LogCritical(BunkumContext.Startup, "Visit this page to view troubleshooting steps: " +
                                                             "https://littlebigrefresh.github.io/Docs/refresh-troubleshooting");
            
            this._logger.Dispose();
            BunkumConsole.WaitForInputAndExit(1);
        }

        stopwatch.Stop();
        this._logger.LogInfo(BunkumContext.Startup, $"Ready to go! Startup tasks took {stopwatch.ElapsedMilliseconds}ms.");
    }

    [DoesNotReturn]
    private async Task Block()
    {
        while (true)
        {
            await this._listener.WaitForConnectionAsync(async context => await Task.Factory.StartNew(async () =>
            {
                // Create a new lazy to get a database context, if the value is never accessed, a database instance is never passed
                Lazy<IDatabaseContext> database = new(this._databaseProvider.GetContext());
                
                // Handle the request
                await this.HandleRequest(context, database);
                
                if(database.IsValueCreated)
                    database.Value.Dispose();
            }));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database)
    {
        Stopwatch requestStopwatch = new();
        requestStopwatch.Start();

        try
        {
            // if (this.UseDigestSystem) this.VerifyDigestRequest(context, context.InputStream);
            Debug.Assert(context.InputStream.Position == 0); // should be at position 0 before we pass to the middleware chain
            
            // Setup a base middleware that calls Endpoints.
            // Passing in these parameters is a little janky in my opinion, but it gets the job done. 
            MainMiddleware mainMiddleware = new(this._endpoints,
                this._logger,
                this._authenticationProvider,
                this._dataStore,
                this._bunkumConfig,
                this._config,
                this._configType,
                this.AssumeAuthenticationRequired);

            Action next = () => { mainMiddleware.HandleRequest(context, database, null!); };
            
            foreach (IMiddleware middleware in this._middlewares)
            {
                // Without this copy, next won't be the same when we invoke next() in the middleware
                // By creating the copy, we ensure the pipeline stays in order.
                // https://www.jetbrains.com/help/rider/AccessToModifiedClosure.html
                Action nextCopy = next;
                
                // Every middleware triggers the last when next() is called.
                // For example:
                //   server.AddMiddleware<MiddlewareA>();
                //   server.AddMiddleware<MiddlewareB>();
                // results in:
                //   MiddlewareB -> MiddlewareA -> mainMiddleware
                // since adding MiddlewareB encapsulates the previous middleware, MiddlewareA.
                //
                // It's important to note that middlewares can "halt" this chain by simply not calling next().
                // This is by design.
                next = () => { middleware.HandleRequest(context, database, nextCopy); };
            }

            next(); // Trigger the pipeline
        }
        catch (Exception e)
        {
            this._logger.LogError(BunkumContext.Request, e.ToString());

            try
            {
                context.ResponseType = ContentType.Plaintext;
                context.ResponseCode = HttpStatusCode.InternalServerError;

#if DEBUG
                context.Write(e.ToString());
#else
                context.Write("Internal Server Error");
#endif
            }
            catch
            {
                // ignored
            }
        }
        finally
        {
            try
            {
                requestStopwatch.Stop();

                this._logger.LogInfo(BunkumContext.Request, $"Served request to {context.RemoteEndpoint}: " +
                                                          $"{(int)context.ResponseCode} {context.ResponseCode} on " +
                                                          $"{context.Method.ToString().ToUpper()} '{context.Uri.PathAndQuery}' " +
                                                          $"({requestStopwatch.ElapsedMilliseconds}ms)");

                await context.FlushResponseAndClose();
            }
            catch
            {
                // ignored
            }
        }
    }

    private void AddEndpointGroup(Type type)
    {
        EndpointGroup? doc = (EndpointGroup?)Activator.CreateInstance(type);
        Debug.Assert(doc != null);
        
        this._endpoints.Add(doc);
    }

    public void AddEndpointGroup<TDoc>() where TDoc : EndpointGroup => this.AddEndpointGroup(typeof(TDoc));

    public void DiscoverEndpointsFromAssembly(Assembly assembly)
    {
        List<Type> types = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EndpointGroup)))
            .ToList();

        foreach (Type type in types) this.AddEndpointGroup(type);
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    public void UseAuthenticationProvider(IAuthenticationProvider<IUser> provider)
    {
        this._authenticationProvider = provider;
    }

    public void UseDatabaseProvider(IDatabaseProvider<IDatabaseContext> provider)
    {
        this._databaseProvider = provider;
    }

    public void UseDataStore(IDataStore dataStore)
    {
        this._dataStore = dataStore;
    }

    // TODO: Configuration hot reload
    // TODO: .ini? would be helpful as it supports comments and we can document in the file itself
    public void UseJsonConfig<TConfig>(string filename) where TConfig : Config, new()
    {
        this._config = Config.LoadFromFile<TConfig>(filename, this._logger);
        this._configType = typeof(TConfig);
    }

    public void AddMiddleware<TMiddleware>() where TMiddleware : IMiddleware, new() => this.AddMiddleware(new TMiddleware());
    public void AddMiddleware<TMiddleware>(TMiddleware middleware) where TMiddleware : IMiddleware => this._middlewares.Add(middleware);
}