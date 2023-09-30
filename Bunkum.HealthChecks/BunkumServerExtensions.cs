using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.HealthChecks.Endpoints;
using Bunkum.HealthChecks.Reports;

namespace Bunkum.HealthChecks;

public static class BunkumServerExtensions
{
    public static void AddHealthCheckService(this BunkumServer server, IDatabaseProvider<IDatabaseContext>? databaseProvider = null, IEnumerable<Type>? checkTypes = null, bool addGeneralCheck = true)
    {
        List<IHealthCheck> checks = new();
        if (checkTypes != null)
        {
            IEnumerable<Type> checkTypesList = checkTypes.ToList();

            if (checkTypesList.Any(check => !check.IsAssignableTo(typeof(IHealthCheck))))
                throw new InvalidOperationException($"Cannot use a health check that is not an {nameof(IHealthCheck)}");
            
            foreach (Type type in checkTypesList)
            {
                IHealthCheck? healthCheck = BunkumServer.InjectDependencies<IHealthCheck>(type, Array.Empty<object>(), new[]
                {
                    BunkumServer.CreateInjectorFromObject(databaseProvider),
                });
                
                if (healthCheck == null)
                {
                    server.Logger.LogWarning(BunkumCategory.Health, $"Health Check {type.Name} failed to initialize.");
                    continue;
                }
                
                checks.Add(healthCheck);
            }
        }
        else
        {
            checks = new List<IHealthCheck>(1);
        }
        
        if(addGeneralCheck) checks.Add(new GeneralHealthCheck());
        
        server.AddEndpointGroup<HealthCheckEndpoints>();
        server.AddService<HealthService>(checks);
    }
}