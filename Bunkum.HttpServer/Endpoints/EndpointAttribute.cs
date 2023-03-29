using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Responses;
using JetBrains.Annotations;

namespace Bunkum.HttpServer.Endpoints;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : Attribute
{
    public readonly string FullRoute;
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

    public EndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Plaintext)
    {
        this.Method = method;
        this.ContentType = contentType;

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

    public EndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
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
                    string paramValue = s.Substring(param.Skip);
                    string paramStart = s.Substring(0, param.Skip);
                    
                    parameters.Add(param.Name, HttpUtility.UrlDecode(paramValue));
                    
                    fullRoute += paramStart + '_';
                }
            }

            return fullRoute == this.FullRoute;
        }
        
        return false;
    }
}