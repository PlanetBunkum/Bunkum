using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Bunkum.Core.Configuration;
using Bunkum.Core.Database;
using Bunkum.Core.Extensions;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using Newtonsoft.Json;
using NotEnoughLogs;

namespace Bunkum.Core.Endpoints.Middlewares;

internal class MainMiddleware : IMiddleware
{
    private readonly Logger _logger;

    private readonly List<EndpointGroup> _endpoints;
    private readonly List<Service> _services;
    private readonly List<IBunkumSerializer> _serializers;
    private readonly List<Config> _configs;

    public MainMiddleware(List<EndpointGroup> endpoints,
        Logger logger,
        List<Service> services,
        List<Config> configs,
        List<IBunkumSerializer> serializers)
    {
        this._logger = logger;
        
        this._endpoints = endpoints;
        this._services = services;
        this._serializers = serializers;
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
            
            context.Write("Not found: "u8);
            context.Write(path);
        }
        else
        {
            context.ResponseType = resp.Value.ResponseType;
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
                IEnumerable<EndpointAttribute> attributes = method.GetCustomAttributes<EndpointAttribute>();
                foreach (EndpointAttribute attribute in attributes)
                {
                    if(attribute.Protocol.Name != context.Protocol.Name)
                        continue;
                        
                    if (!attribute.UriMatchesRoute(context.Uri, context.Method, out Dictionary<string, string> parameters))
                        continue;

                    this._logger.LogTrace(BunkumCategory.Request, $"Handling request with {group.GetType().Name}.{method.Name}");

                    foreach (Service service in this._services)
                    {
                        Response? response = service.OnRequestHandled(context, method, database);
                        if (response != null) return response;
                    }

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
                            Protocol = context.Protocol,
                            Method = context.Method,
                            RequestHeaders = context.RequestHeaders,
                            ResponseHeaders = context.ResponseHeaders,
                            RemoteCertificate = context.RemoteCertificate,
                        },
                    };

                    // Next, lets iterate through the method's arguments and add some based on what we find.
                    {
                        Response? response = this.InjectParametersIntoEndpointInvocation(context, method, attribute, body, parameters, database, invokeList);
                        if (response != null) return response;
                    }
                    
                    ClientCacheResponseAttribute? cacheAttribute = method.GetCustomAttribute<ClientCacheResponseAttribute>();
                    if (cacheAttribute?.Seconds != null)
                    {
                        context.ResponseHeaders.Add("Cache-Control", "max-age=" + cacheAttribute.Seconds);
                    }
                    
                    Response? returnedResponse = null;
                    try
                    {
                        object? val = method.Invoke(group, invokeList.ToArray());
                        returnedResponse = this.GenerateResponseFromEndpoint(val, attribute, method);
                        
                        // ReSharper disable once ConstantConditionalAccessQualifier
                        int statusCode = (int)returnedResponse?.StatusCode!;
                        if (cacheAttribute is { OnlyCacheSuccess: true } && statusCode is < 200 or >= 299)
                        {
                            context.ResponseHeaders.Remove("Cache-Control");
                        }
                        
                        return returnedResponse;
                    }
                    finally
                    {
                        foreach (Service service in this._services)
                        {
                            service.AfterRequestHandled(context, returnedResponse, method, database);
                        }
                    }
                }
            }
        }

        return null;
    }

    private Response GenerateResponseFromEndpoint(object? val, EndpointAttribute attribute, MethodInfo method)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (val)
        {
            case null:
            {
                HttpStatusCode nullCode = method.GetCustomAttribute<NullStatusCodeAttribute>()?.StatusCode ??
                                          HttpStatusCode.NotFound;
                return new Response(Array.Empty<byte>(), attribute.ContentType, nullCode);
            }
            case Response response:
            {
                return response;
            }
            case HttpStatusCode statusCode:
            {
                return statusCode;
            }
            case byte[] data:
            {
                HttpStatusCode okCode = method.GetCustomAttribute<SuccessStatusCodeAttribute>()?.StatusCode ??
                                        HttpStatusCode.OK;
                return new Response(data, attribute.ContentType, okCode);
            }
            default:
            {
                IBunkumSerializer? serializer = this.GetSerializerOrDefault(attribute.ContentType);
                
                HttpStatusCode okCode = method.GetCustomAttribute<SuccessStatusCodeAttribute>()?.StatusCode ??
                                        HttpStatusCode.OK;
                return new Response(val, attribute.ContentType, okCode, serializer);
            }
        }
    }

    private Response? InjectParametersIntoEndpointInvocation(ListenerContext context,
        MethodInfo method,
        EndpointAttribute attribute,
        MemoryStream body,
        IReadOnlyDictionary<string, string> parameters,
        Lazy<IDatabaseContext> database,
        ICollection<object?> invokeList)
    {
        foreach (ParameterInfo param in method.GetParameters().Skip(1)) // Skip the request context.
        {
            Type paramType = param.ParameterType;

            // Ask all services to try to provide an argument for this parameter.
            object? arg = null;
            
            // Pass in the request body as a parameter
            if (param.Name == "body")
            {
                // If the request has no body and we have a body parameter,
                // then it's probably safe to assume it's required unless otherwise explicitly stated.
                // Fire a bad request back if this is the case.
                if (!context.HasBody && !method.HasCustomAttribute<AllowEmptyBodyAttribute>())
                {
                    this._logger.LogWarning(BunkumCategory.Request, "Rejecting request due to empty body");
                    return new Response([], ContentType.Plaintext, HttpStatusCode.BadRequest);
                }

                if (!this.TryGetBodyParameter(attribute, paramType, body, out arg))
                {
                    return new Response([], ContentType.Plaintext, HttpStatusCode.BadRequest);
                }
            }
            else if(paramType.IsAssignableTo(typeof(IDatabaseContext)))
            {
                // Pass in a database context if the endpoint needs one.
                arg = database.Value;
            }
            else if (paramType.IsAssignableTo(typeof(Config)))
            {
                Config? configToPass = this._configs.FirstOrDefault(config => paramType == config.GetType());

                if (configToPass == null)
                    throw new ArgumentNullException(
                        $"Could not find a valid config for the {paramType.Name} parameter '{param.Name}'");

                arg = configToPass;
            }
            else if (paramType == typeof(string))
            {
                // Attempt to pass in a route parameter based on the method parameter's name
                arg = parameters!.GetValueOrDefault(param.Name);
            }
            else if (paramType == typeof(int) || paramType == typeof(int?))
            {
                // Also try to pass in a route parameter for integers
                string? strParam = parameters!.GetValueOrDefault(param.Name);
                bool intParsed = int.TryParse(strParam, out int intParam);
                arg = intParsed ? intParam : null;
            }

            if (arg != null)
            {
                invokeList.Add(arg);
                continue;
            }
            
            List<Service>.Enumerator services = this._services.GetEnumerator();
            while (arg == null)
            {
                if (!services.MoveNext()) break;
                arg = services.Current.AddParameterToEndpoint(context, new BunkumParameterInfo(param), database);
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
                this._logger.LogWarning(BunkumCategory.Request, 
                    $"Could not find a valid argument for the {paramType.Name} parameter '{param.Name}'. " +
                    $"Null will be used instead.");
            }

            // Add the arg even if null, as even if we don't know what this param is or what to do with it,
            // it's probably better than not calling the endpoint and throwing an exception causing things to explode.
            invokeList.Add(arg);
        }

        return null;
    }

    private bool TryGetBodyParameter(EndpointAttribute attribute, Type paramType, MemoryStream bodyStream, out object? bodyParam)
    {
        byte[] body = bodyStream.ToArray();

        bodyParam = null;
        
        if (paramType == typeof(Stream)) bodyParam = new MemoryStream(body);
        else if (paramType == typeof(string)) bodyParam = Encoding.UTF8.GetString(TrimToFirstNullByte(body));
        else if (paramType == typeof(byte[])) bodyParam = body;
        else if (attribute.ContentType == ContentType.Xml)
        {
            XmlSerializer serializer = new(paramType);
            try
            {
                object? obj = serializer.Deserialize(new StringReader(Encoding.UTF8.GetString(TrimToFirstNullByte(body))));
                if (obj == null) throw new Exception();
                bodyParam = obj;
            }
            catch (Exception e)
            {
                this._logger.LogError(BunkumCategory.UserContent, $"Failed to parse object data: {e}\n\nXML: {Encoding.UTF8.GetString(body)}");
                return false;
            }
        }
        else if (attribute.ContentType == ContentType.Json)
        {
            try
            {
                JsonSerializer serializer = new();
                using JsonReader reader = new JsonTextReader(new StringReader(Encoding.UTF8.GetString(TrimToFirstNullByte(body))));
                object? obj = serializer.Deserialize(reader, paramType);
                if (obj == null) throw new Exception();
                bodyParam = obj;
            }
            catch (Exception e)
            {
                this._logger.LogError(BunkumCategory.UserContent, $"Failed to parse object data: {e}\n\nJSON: {Encoding.UTF8.GetString(body)}");
                return false;
            }
        }
        // We can't find a valid type to send or deserialization failed
        else
        {
            this._logger.LogWarning(BunkumCategory.Request,
                "Rejecting request, couldn't find a valid type to deserialize with");
            return false;
        }

        return true;
    }

    private static ReadOnlySpan<byte> TrimToFirstNullByte(ReadOnlySpan<byte> arr)
    {
        //Find the first null byte
        int idx = arr.IndexOf((byte)0);

        //If theres no null byte, do not trim
        if (idx == -1)
        {
            idx = arr.Length;
        }

        return arr[..idx];
    }
    
    private IBunkumSerializer? GetSerializerOrDefault(string contentType) 
        => this._serializers.FirstOrDefault(s => s.ContentTypes.Contains(contentType));
}