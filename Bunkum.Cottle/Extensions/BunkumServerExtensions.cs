using Bunkum.Core;
using Bunkum.Cottle.Middlewares;
using Bunkum.Cottle.Services;
using Cottle;

namespace Bunkum.Cottle.Extensions;

public static class BunkumServerExtensions
{
    public static void AddCottle(this BunkumServer server, DocumentConfiguration configuration)
    {
        CottleService service = new(server.Logger, configuration);
        server.AddService(service);

        CottleMiddleware middleware = new(service);
        server.AddMiddleware(middleware);
    }
}