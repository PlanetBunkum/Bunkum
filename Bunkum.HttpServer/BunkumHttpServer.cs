using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Authentication.Dummy;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Database.Dummy;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Extensions;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using JetBrains.Annotations;
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
    public EventHandler<ListenerContext>? NotFound;

    public bool AssumeAuthenticationRequired = false;
    public bool UseDigestSystem = false;
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore FieldCanBeMadeReadOnly.Global

    // Should be 19 characters (or less maybe?)
    // Length was taken from PS3 and PS4 digest keys
    public const string DigestKey = "CustomServerDigest";

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

    [Pure]
    private Response? InvokeEndpointByRequest(ListenerContext context, Lazy<IDatabaseContext> database, MemoryStream body)
    {
        foreach (EndpointGroup group in this._endpoints)
        {
            foreach (MethodInfo method in group.GetType().GetMethods())
            {
                ImmutableArray<EndpointAttribute> attributes = method.GetCustomAttributes<EndpointAttribute>().ToImmutableArray();
                if(attributes.Length == 0) continue;

                foreach (EndpointAttribute attribute in attributes)
                {
                    if (!attribute.UriMatchesRoute(
                            context.Uri,
                            context.Method,
                            out Dictionary<string, string> parameters))
                    {
                        continue;
                    }
                    
                    this._logger.LogTrace(BunkumContext.Request, $"Handling request with {group.GetType().Name}.{method.Name}");

                    IUser? user = null;
                    if (method.GetCustomAttribute<AuthenticationAttribute>()?.Required ?? this.AssumeAuthenticationRequired)
                    {
                        user = this._authenticationProvider.AuthenticateUser(context, database.Value);
                        if (user == null)
                            return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.Forbidden);
                    }

                    HttpStatusCode nullCode = method.GetCustomAttribute<NullStatusCodeAttribute>()?.StatusCode ??
                                              HttpStatusCode.NotFound;

                    // Build list to invoke endpoint method with
                    List<object?> invokeList = new() { 
                        new RequestContext // 1st argument is always the request context. This is fact, and is backed by an analyzer.
                        {
                            RequestStream = body,
                            QueryString = context.Query,
                            Url = context.Uri,
                            Logger = this._logger,
                            DataStore = this._dataStore,
                        },
                    };

                    // Next, lets iterate through the method's arguments and add some based on what we find.
                    foreach (ParameterInfo param in method.GetParameters().Skip(1))
                    {
                        Type paramType = param.ParameterType;

                        // Pass in the request body as a parameter
                        if (param.Name == "body")
                        {
                            // If the request has no body and we have a body parameter, then it's probably safe to assume it's required unless otherwise explicitly stated.
                            // Fire a bad request back if this is the case.
                            if (!context.HasBody && !method.HasCustomAttribute<AllowEmptyBodyAttribute>())
                            {
                                this._logger.LogWarning(BunkumContext.Request, "Rejecting request due to empty body");
                                return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                            }

                            if(paramType == typeof(Stream)) invokeList.Add(body);
                            else if(paramType == typeof(string)) invokeList.Add(Encoding.Default.GetString(body.GetBuffer()));
                            else if(paramType == typeof(byte[])) invokeList.Add(body.GetBuffer());
                            else if(attribute.ContentType == ContentType.Xml)
                            {
                                XmlSerializer serializer = new(paramType);
                                try
                                {
                                    object? obj = serializer.Deserialize(new StreamReader(body));
                                    if (obj == null) throw new Exception();
                                    invokeList.Add(obj);
                                }
                                catch (Exception e)
                                {
                                    this._logger.LogError(BunkumContext.UserContent, $"Failed to parse object data: {e}\n\nXML: {body}");
                                    return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                                }
                            }
                            // We can't find a valid type to send or deserialization failed
                            else
                            {
                                this._logger.LogWarning(BunkumContext.Request, "Rejecting request, couldn't find a valid type to send");
                                return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                            }

                            body.Seek(0, SeekOrigin.Begin);

                            continue;
                        }
                        
                        if (paramType.IsAssignableTo(typeof(IUser)))
                        {
                            // Users will always be non-null at this point. Once again, this is backed by an analyzer.
                            Debug.Assert(user != null);
                            invokeList.Add(user);
                        }
                        else if(paramType.IsAssignableTo(typeof(IDatabaseContext)))
                        {
                            // Pass in a database context if the endpoint needs one.
                            invokeList.Add(database.Value);
                        }
                        else if (paramType.IsAssignableTo(this._configType))
                        {
                            if (this._config == null)
                                throw new InvalidOperationException("A config was attempted to be passed into an endpoint, but there was no config set on startup!");
                            
                            invokeList.Add(this._config);
                        }
                        else if (paramType.IsAssignableTo(typeof(BunkumConfig)))
                        {
                            invokeList.Add(this._bunkumConfig);
                        }
                        else if (paramType == typeof(string))
                        {
                            // Attempt to pass in a route parameter based on the method parameter's name
                            invokeList.Add(parameters!.GetValueOrDefault(param.Name));
                        }
                        else
                        {
                            // We don't know what this param is or what to do with it, so pass in null.
                            // Better than not calling the endpoint and throwing an exception.
                            invokeList.Add(null);
                        }
                    }

                    object? val = method.Invoke(group, invokeList.ToArray());

                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (val)
                    {
                        case null:
                            return new Response(Array.Empty<byte>(), attribute.ContentType, nullCode);
                        case Response response:
                            return response;
                        default:
                            return new Response(val, attribute.ContentType);
                    }
                }
            }
        }

        return null;
    }

    private async Task HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database)
    {
        Stopwatch requestStopwatch = new();
        requestStopwatch.Start();

        try
        {
            string path = context.Uri.AbsolutePath;

            if (this.UseDigestSystem) this.VerifyDigestRequest(context, context.InputStream);
            Debug.Assert(context.InputStream.Position == 0); // should be at position 0 before we pass to the application's endpoints

            // Find a endpoint using the request context, pass in database and our MemoryStream
            Response? resp = this.InvokeEndpointByRequest(context, database, context.InputStream);

            if (resp == null)
            {
                context.ResponseType = ContentType.Plaintext;
                context.ResponseCode = HttpStatusCode.NotFound;
                context.Write("Not found: " + path);
                
                this.NotFound?.Invoke(this, context);
            }
            else
            {
                context.ResponseType = resp.Value.ContentType;
                context.ResponseCode = resp.Value.StatusCode;
                
                if(this.UseDigestSystem) this.SetDigestResponse(context, new MemoryStream(resp.Value.Data));
                context.Write(resp.Value.Data);
            }
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

    // Referenced from Project Lighthouse
    // https://github.com/LBPUnion/ProjectLighthouse/blob/d16132f67f82555ef636c0dabab5aabf36f57556/ProjectLighthouse.Servers.GameServer/Middlewares/DigestMiddleware.cs
    // https://github.com/LBPUnion/ProjectLighthouse/blob/19ea44e0e2ff5f2ebae8d9dfbaf0f979720bd7d9/ProjectLighthouse/Helpers/CryptoHelper.cs#L35
    // TODO: make this non-lbp specific, or implement middlewares and move to game server
    private bool VerifyDigestRequest(ListenerContext context, Stream body)
    {
        Debug.Assert(this.UseDigestSystem, "Tried to verify digest request when digest system is disabled");
        
        string url = context.Uri.AbsolutePath;
        string auth = $"{context.Cookies["MM_AUTH"] ?? string.Empty}";

        string digestResponse = this.CalculateDigest(url, body, auth);

        string digestHeader = !url.StartsWith("/lbp/upload/") ? "X-Digest-A" : "X-Digest-B";
        string clientDigest = context.RequestHeaders[digestHeader] ?? string.Empty;
        
        context.ResponseHeaders["X-Digest-B"] = digestResponse;
        if (clientDigest == digestResponse) return true;
        
        this._logger.LogWarning(BunkumContext.Digest, $"Digest failed: {clientDigest} != {digestResponse}");
        return false;
    }

    private void SetDigestResponse(ListenerContext context, Stream body)
    {
        Debug.Assert(this.UseDigestSystem, "Tried to set digest response when digest system is disabled");
        
        string url = context.Uri.AbsolutePath;
        string auth = $"{context.Cookies["MM_AUTH"] ?? string.Empty}";

        string digestResponse = this.CalculateDigest(url, body, auth);
        
        context.ResponseHeaders["X-Digest-A"] = digestResponse;
    }

    private string CalculateDigest(string url, Stream body, string auth)
    {
        using MemoryStream ms = new();

        // FIXME: Directly referencing LBP in Bunkum
        if (!url.StartsWith("/lbp/upload/"))
        {
            // get request body
            body.CopyTo(ms);
            body.Seek(0, SeekOrigin.Begin);
        }
        
        ms.WriteString(auth);
        ms.WriteString(url);
        ms.WriteString(DigestKey);
        
        ms.Position = 0;
        using SHA1 sha = SHA1.Create();
        string digestResponse = Convert.ToHexString(sha.ComputeHash(ms)).ToLower();

        return digestResponse;
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
}