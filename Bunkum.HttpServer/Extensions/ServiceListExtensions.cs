using Bunkum.HttpServer.Services;
using JetBrains.Annotations;

namespace Bunkum.HttpServer.Extensions;

public static class ServiceListExtensions
{
    public static TService? TryGetService<TService>(this IEnumerable<Service> services) where TService : Service
    {
        Service? service = services.FirstOrDefault(s => s.GetType() == typeof(TService));
        return service as TService;
    }
    
    public static bool TryGetService<TService>(this IEnumerable<Service> services, out TService? service) where TService : Service
    {
        service = services.TryGetService<TService>();
        return service != null;
    }
}