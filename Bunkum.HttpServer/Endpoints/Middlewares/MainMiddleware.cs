using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Extensions;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Newtonsoft.Json;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Endpoints.Middlewares;

internal class MainMiddleware : IMiddleware
{
    private readonly List<EndpointGroup> _endpoints;
    private readonly LoggerContainer<BunkumContext> _logger;
    
    private readonly IAuthenticationProvider<IUser, IToken> _authenticationProvider;
    private readonly bool _assumeAuthenticationRequired;
    
    private readonly IDataStore _dataStore;
    private readonly IRateLimiter _rateLimiter;
    
    private readonly Config? _config;
    private readonly Type? _configType;
    
    private readonly BunkumConfig _bunkumConfig;
    
    public MainMiddleware(List<EndpointGroup> endpoints, LoggerContainer<BunkumContext> logger,
        IAuthenticationProvider<IUser, IToken> authenticationProvider, IDataStore dataStore, BunkumConfig bunkumConfig,
        Config? config, Type? configType, bool assumeAuthenticationRequired, IRateLimiter rateLimiter)
    {
        this._endpoints = endpoints;
        this._logger = logger;
        
        this._authenticationProvider = authenticationProvider;
        this._dataStore = dataStore;
        this._rateLimiter = rateLimiter;
        
        this._config = config;
        this._configType = configType;
        
        this._assumeAuthenticationRequired = assumeAuthenticationRequired;

        this._bunkumConfig = bunkumConfig;
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string path = context.Uri.AbsolutePath;
        
        // Next is null. This should always be the last middleware in the chain.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Debug.Assert(next == null);
        
        // Find a endpoint using the request context, pass in database
        Response? resp = this.InvokeEndpointByRequest(context, database);

        if (resp == null)
        {
            context.ResponseType = ContentType.Plaintext;
            context.ResponseCode = HttpStatusCode.NotFound;
            context.Write("Not found: " + path);
        }
        else
        {
            context.ResponseType = resp.Value.ContentType;
            context.ResponseCode = resp.Value.StatusCode;
                
            // if(this.UseDigestSystem) this.SetDigestResponse(context, new MemoryStream(resp.Value.Data));
            context.Write(resp.Value.Data);
        }
    }

    private Response? InvokeEndpointByRequest(ListenerContext context, Lazy<IDatabaseContext> database)
    {
        MemoryStream body = context.InputStream;
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

                    Lazy<IUser?> user = new(() => this._authenticationProvider.AuthenticateUser(context, database));
                    Lazy<IToken?> token = new(() => this._authenticationProvider.AuthenticateToken(context, database));
                    
                    if (method.GetCustomAttribute<AuthenticationAttribute>()?.Required ?? this._assumeAuthenticationRequired)
                    {
                        if (user.Value == null)
                            return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.Forbidden);
                    }

                    HttpStatusCode nullCode = method.GetCustomAttribute<NullStatusCodeAttribute>()?.StatusCode ??
                                              HttpStatusCode.NotFound;
                    
                    HttpStatusCode okCode = method.GetCustomAttribute<SuccessStatusCodeAttribute>()?.StatusCode ??
                                              HttpStatusCode.OK;

                    // Build list to invoke endpoint method with
                    List<object?> invokeList = new() { 
                        new RequestContext // 1st argument is always the request context. This is fact, and is backed by an analyzer.
                        {
                            RequestStream = body,
                            QueryString = context.Query,
                            Url = context.Uri,
                            Logger = this._logger,
                            DataStore = this._dataStore,
                            Cookies = context.Cookies,
                            RemoteEndpoint = context.RemoteEndpoint,
                            Method = context.Method,
                            RequestHeaders = context.RequestHeaders,
                            ResponseHeaders = context.ResponseHeaders,
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
                            else if (attribute.ContentType == ContentType.Json)
                            {
                                try
                                {
                                    string bodyStr = Encoding.Default.GetString(body.GetBuffer());
                                    object? obj = JsonConvert.DeserializeObject(bodyStr, paramType);
                                    if (obj == null) throw new Exception();
                                    invokeList.Add(obj);
                                }
                                catch (Exception e)
                                {
                                    this._logger.LogError(BunkumContext.UserContent, $"Failed to parse object data: {e}\n\nJSON: {body}");
                                    return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                                }
                            }
                            // We can't find a valid type to send or deserialization failed
                            else
                            {
                                this._logger.LogWarning(BunkumContext.Request, "Rejecting request, couldn't find a valid type to deserialize with");
                                return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                            }

                            body.Seek(0, SeekOrigin.Begin);

                            continue;
                        }
                        
                        if (paramType.IsAssignableTo(typeof(IUser)))
                        {
                            invokeList.Add(user.Value);
                        }
                        else if (paramType.IsAssignableTo(typeof(IToken)))
                        {
                            invokeList.Add(token.Value);
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

                    long? cacheSeconds;
                    if ((cacheSeconds = method.GetCustomAttribute<ClientCacheResponseAttribute>()?.Seconds) != null)
                    {
                        context.ResponseHeaders.Add("Cache-Control", "max-age=" + cacheSeconds.Value);
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
                            return new Response(val, attribute.ContentType, okCode);
                    }
                }
            }
        }

        return null;
    }
}