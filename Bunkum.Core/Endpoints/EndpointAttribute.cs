using System.Diagnostics.CodeAnalysis;
using System.Web;
using Bunkum.Core.Listener.Protocol;
using Bunkum.Core.Listener.Parsing;
using JetBrains.Annotations;

namespace Bunkum.Core.Endpoints;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : Attribute
{
    public string FullRoute { get; private set; }
    private readonly Dictionary<int, RouteParam> _parameterIndexes = new();

    public readonly Method Method;
    public readonly ContentType ContentType;

    private struct RouteParam
    {
        internal readonly int Skip;
        internal readonly string Name;

        internal RouteParam(int skip, string s)
        {
            this.Skip = skip;
            this.Name = s;
        }
    }

    private void GetRouteParameters(string route)
    {
        // Skip scanning for parameters if the route obviously doesn't contain one
        // Maybe can optimize further in the future with source generators?
        // Only runs once per endpoint either way, so whatever
        if (route.IndexOf('{') == -1)
        {
            this.FullRoute = route;
            return;
        }
        
        // Scan for route parameters

        string fullRoute = string.Empty;
        
        string[] routeSplit = route.Split('/');
        for (int i = 0; i < routeSplit.Length; i++)
        {
            string s = routeSplit[i];
            if (i != 0) fullRoute += '/';

            bool parsingParam = false;
            string param = string.Empty;

            int j = 0; // easier flow, this foreach uses lots of continues
            foreach (char c in s)
            {
                if (parsingParam)
                {
                    if (c == '}')
                    {
                        this._parameterIndexes.Add(i, new RouteParam(j, param));
                        fullRoute += '_';
                        parsingParam = false;
                        continue;
                    }
                    
                    param += c;
                    continue;
                }
                
                if (c == '{')
                {
                    parsingParam = true;
                    continue;
                }

                j++;
                fullRoute += c;
            }

            if (parsingParam) throw new ArgumentException("Route parameter was not closed");
        }

        this.FullRoute = fullRoute;
    }

    public EndpointAttribute(string route, HttpMethods method = HttpMethods.Get, ContentType contentType = ContentType.Plaintext)
    {
        this.Method = MethodUtils.FromEnum(typeof(HttpProtocolMethods), method);
        this.ContentType = contentType;

        this.GetRouteParameters(route);
    }
    
    public EndpointAttribute(string route, ContentType contentType)
        : this(route, HttpMethods.Get, contentType)
    {
        
    }
    
    public EndpointAttribute(string route, ContentType contentType, HttpMethods method)
        : this(route, method, contentType)
    {
        
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public bool UriMatchesRoute(Uri? uri, Method method, out Dictionary<string, string> parameters)
    {
        parameters = new Dictionary<string, string>();
        if (uri == null) return false;
        
        string path = uri.AbsolutePath;

        if (method != this.Method) return false;
        if (this.FullRoute == path) return true;
        if (this._parameterIndexes.Count != 0)
        {
            string fullRoute = string.Empty;
            string[] routeSplit = path.Split('/');
            for (int i = 0; i < routeSplit.Length; i++)
            {
                string s = routeSplit[i];
                if (i != 0) fullRoute += '/';
                
                if (!this._parameterIndexes.TryGetValue(i, out RouteParam param))
                {
                    fullRoute += s;
                }
                else
                {
                    try
                    {
                        string paramValue = s.Substring(param.Skip);
                        string paramStart = s.Substring(0, param.Skip);
                        
                        parameters.Add(param.Name, HttpUtility.UrlDecode(paramValue));
                        fullRoute += paramStart + '_';
                    }
                    catch
                    {
                        fullRoute += '_';
                    }
                }
            }

            return fullRoute == this.FullRoute;
        }
        
        return false;
    }
}