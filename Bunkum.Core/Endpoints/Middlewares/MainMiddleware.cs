using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Bunkum.Core.Configuration;
using Bunkum.Core.Database;
using Bunkum.Core.Extensions;
using Bunkum.Core.Listener.Parsing;
using Bunkum.Core.Listener.Request;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Newtonsoft.Json;
using NotEnoughLogs;

namespace Bunkum.Core.Endpoints.Middlewares;

internal class MainMiddleware : IMiddleware
{
    private readonly List<EndpointGroup> _endpoints;
    private readonly Logger _logger;

    private readonly List<Service> _services;

    private readonly List<Config> _configs;

    public MainMiddleware(List<EndpointGroup> endpoints, Logger logger, List<Service> services, List<Config> configs)
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
                            Version = context.Version,
                            Method = context.Method,
                            RequestHeaders = context.RequestHeaders,
                            ResponseHeaders = context.ResponseHeaders,
                        },
                    };

                    // Next, lets iterate through the method's arguments and add some based on what we find.
                    {
                        Response? response = this.InjectParametersIntoEndpointInvocation(context, method, attribute, body, parameters, database, invokeList);
                        if (response != null) return response;
                    }

                    long? cacheSeconds;
                    if ((cacheSeconds = method.GetCustomAttribute<ClientCacheResponseAttribute>()?.Seconds) != null)
                    {
                        context.ResponseHeaders.Add("Cache-Control", "max-age=" + cacheSeconds.Value);
                    }

                    object? val = method.Invoke(group, invokeList.ToArray());
                    Response returnedResponse = GenerateResponseFromEndpoint(val, attribute, method);
                    
                    foreach (Service service in this._services)
                    {
                        service.AfterRequestHandled(context, returnedResponse, method, database);
                    }

                    return returnedResponse;
                }
            }
        }

        return null;
    }

    private static Response GenerateResponseFromEndpoint(object? val, EndpointAttribute attribute, MethodInfo method)
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
                HttpStatusCode okCode = method.GetCustomAttribute<SuccessStatusCodeAttribute>()?.StatusCode ??
                                        HttpStatusCode.OK;
                return new Response(val, attribute.ContentType, okCode);
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

            // Pass in the request body as a parameter
            if (param.Name == "body")
            {
                // If the request has no body and we have a body parameter,
                // then it's probably safe to assume it's required unless otherwise explicitly stated.
                // Fire a bad request back if this is the case.
                if (!context.HasBody && !method.HasCustomAttribute<AllowEmptyBodyAttribute>())
                {
                    this._logger.LogWarning(BunkumCategory.Request, "Rejecting request due to empty body");
                    return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                }

                if (!this.TryAddBodyToInvocation(attribute, paramType, body, invokeList))
                {
                    return new Response(Array.Empty<byte>(), ContentType.Plaintext, HttpStatusCode.BadRequest);
                }
            }
            else if(paramType.IsAssignableTo(typeof(IDatabaseContext)))
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
                    this._logger.LogWarning(BunkumCategory.Request, 
                        $"Could not find a valid argument for the {paramType.Name} parameter '{param.Name}'. " +
                        $"Null will be used instead.");
                }

                // Add the arg even if null, as even if we don't know what this param is or what to do with it,
                // it's probably better than not calling the endpoint and throwing an exception causing things to explode.
                invokeList.Add(arg);
            }
        }

        return null;
    }

    private bool TryAddBodyToInvocation(EndpointAttribute attribute, Type paramType, MemoryStream body, ICollection<object?> invokeList)
    {
        if (paramType == typeof(Stream)) invokeList.Add(body);
        else if (paramType == typeof(string))
        {
            TrimToToFirstNullByte(body);
            invokeList.Add(Encoding.Default.GetString(body.ToArray()));
        }
        else if (paramType == typeof(byte[])) invokeList.Add(body.GetBuffer());
        else if (attribute.ContentType == ContentType.Xml)
        {
            TrimToToFirstNullByte(body);

            XmlSerializer serializer = new(paramType);
            try
            {
                object? obj = serializer.Deserialize(new StreamReader(body));
                if (obj == null) throw new Exception();
                invokeList.Add(obj);
            }
            catch (Exception e)
            {
                this._logger.LogError(BunkumCategory.UserContent, $"Failed to parse object data: {e}\n\nXML: {body}");
                return false;
            }
        }
        else if (attribute.ContentType == ContentType.Json)
        {
            TrimToToFirstNullByte(body);

            try
            {
                JsonSerializer serializer = new();
                using JsonReader reader = new JsonTextReader(new StreamReader(body, null, false, -1, true));
                object? obj = serializer.Deserialize(reader, paramType);
                if (obj == null) throw new Exception();
                invokeList.Add(obj);
            }
            catch (Exception e)
            {
                this._logger.LogError(BunkumCategory.UserContent, $"Failed to parse object data: {e}\n\nJSON: {body}");
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

        body.Seek(0, SeekOrigin.Begin);
        return true;
    }

    private static void TrimToToFirstNullByte(Stream body)
    {
        long i = 0;
        body.Seek(0, SeekOrigin.Begin);
        int b;
        while ((b = body.ReadByte()) != -1)
        {
            if (b == 0) break;

            i += 1;
        }

        //Only call SetLength when necessary 
        if(i != body.Length)
            body.SetLength(i);
        
        body.Seek(0, SeekOrigin.Begin);
    }
}