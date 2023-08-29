using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Extensions;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Services;
using Newtonsoft.Json;
using NotEnoughLogs;

namespace Bunkum.HttpServer.Endpoints.Middlewares;

internal class MainMiddleware : IMiddleware
{
    private readonly List<EndpointGroup> _endpoints;
    private readonly LoggerContainer<BunkumContext> _logger;

    private readonly List<Service> _services;

    private readonly List<Config> _configs;

    public MainMiddleware(List<EndpointGroup> endpoints, LoggerContainer<BunkumContext> logger, List<Service> services, List<Config> configs)
    {
        this._endpoints = endpoints;
        this._logger = logger;
        this._services = services;

        this._configs = configs;
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

                    foreach (Service service in this._services)
                    {
                        Response? response = service.OnRequestHandled(context, method, database);
                        if (response != null) return response;
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
                            Cookies = context.Cookies,
                            RemoteEndpoint = context.RemoteEndpoint,
                            Method = context.Method,
                            RequestHeaders = context.RequestHeaders,
                            ResponseHeaders = context.ResponseHeaders,
                        },
                    };

                    // Next, lets iterate through the method's arguments and add some based on what we find.
                    foreach (ParameterInfo param in method.GetParameters().Skip(1)) // Skip the request context.
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

                        if(paramType.IsAssignableTo(typeof(IDatabaseContext)))
                        {
                            // Pass in a database context if the endpoint needs one.
                            invokeList.Add(database.Value);
                        }
                        else if (paramType.IsAssignableTo(typeof(Config)))
                        {
                            Config? configToPass = this._configs.FirstOrDefault(config => paramType == config.GetType());

                            if (configToPass == null)
                            {
                                throw new ArgumentNullException(
                                    $"Could not find a valid config for the {paramType.Name} parameter '{param.Name}'");
                            }

                            invokeList.Add(configToPass);
                        }
                        else if (paramType == typeof(string))
                        {
                            // Attempt to pass in a route parameter based on the method parameter's name
                            invokeList.Add(parameters!.GetValueOrDefault(param.Name));
                        }
                        else if (paramType == typeof(int) || paramType == typeof(int?))
                        {
                            // Also try to pass in a route parameter for integers
                            string? strParam = parameters!.GetValueOrDefault(param.Name);
                            bool intParsed = int.TryParse(strParam, out int intParam);
                            invokeList.Add(intParsed ? intParam : null); // check result, intParam will be null
                        }
                        else
                        {
                            // Ask all services to try to provide an argument for this parameter.
                            object? arg = null;
                            
                            List<Service>.Enumerator services = this._services.GetEnumerator();
                            while (arg == null)
                            {
                                if (!services.MoveNext()) break;
                                arg = services.Current.AddParameterToEndpoint(context, param, database);
                            }
                            
                            services.Dispose();
                            
                            // NullabilityInfoContext isn't thread-safe, so it cant be re-used
                            // https://stackoverflow.com/a/72586919
                            // TODO: do benchmarks of this call to see if we should optimize
                            bool isNullable = new NullabilityInfoContext().Create(param).WriteState == NullabilityState.Nullable;

                            // If our argument is still null, log a warning as a precaution.
                            // Don't consider nullable arguments for this warning
                            if (arg == null && !isNullable)
                            {
                                this._logger.LogWarning(BunkumContext.Request, 
                                    $"Could not find a valid argument for the {paramType.Name} parameter '{param.Name}'. " +
                                    $"Null will be used instead.");
                            }

                            // Add the arg even if null, as even if we don't know what this param is or what to do with it,
                            // it's probably better than not calling the endpoint and throwing an exception causing things to explode.
                            invokeList.Add(arg);
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
                        case byte[] data:
                            return new Response(data, attribute.ContentType, okCode);
                        default:
                            return new Response(val, attribute.ContentType, okCode);
                    }
                }
            }
        }

        return null;
    }
}