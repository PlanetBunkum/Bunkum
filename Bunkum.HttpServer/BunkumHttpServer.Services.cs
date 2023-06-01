using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Configuration;
using Bunkum.HttpServer.Health;
using Bunkum.HttpServer.Health.Endpoints;
using Bunkum.HttpServer.Health.Reports;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Services;
using Bunkum.HttpServer.Storage;
using Bunkum.HttpServer.Time;
using JetBrains.Annotations;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class BunkumHttpServer // Services
{
    #region Dependency Injection

    [Pure]
    private static TObject? InjectDependencies<TObject>(object?[] args, IEnumerable<Func<ParameterInfo, object?>> injectorLists) where TObject : class 
        => InjectDependencies<TObject>(typeof(TObject), args, injectorLists);

    [Pure]
    private static TObject? InjectDependencies<TObject>(Type type, object?[] args, IEnumerable<Func<ParameterInfo, object?>> injectorLists) where TObject : class
    {
        List<object?> fullArgs = args.ToList();
        
        // Find a suitable constructor for the given parameters. This accounts for null parameters.
        // Some additional logic is present to check for references to services and automatically inject them if present.
        ConstructorInfo? ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(info =>
            {
                List<object?> newArgs = new(args);
                ParameterInfo[] paramInfos = info.GetParameters();

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    bool foundArg = false;
                    foreach (Func<ParameterInfo, object?> injector in injectorLists)
                    {
                        object? result = injector.Invoke(paramInfos[i]);
                        if (result == null) continue;
                        
                        newArgs.Insert(i, result);
                        foundArg = true;
                        break;
                    }
                    if(foundArg) continue;

                    if (newArgs[i] == null)
                    {
                        if (paramInfos[i].ParameterType.IsValueType)
                            return false;
                    }
                    else if (!paramInfos[i].ParameterType.IsInstanceOfType(newArgs[i])) return false;
                }

                if (newArgs.Count != paramInfos.Length) return false;
                
                fullArgs = newArgs;
                return true;
            });

        if (ctor == null) return null;
        TObject? service = (TObject?)ctor.Invoke(fullArgs.ToArray());

        return service;
    }
    
    private static object? InjectFromPool<TObject, TInjected>(ParameterInfo info, IEnumerable<TInjected> pool)
    {
        // Attempt to find dependencies on other services and inject them.
        if (!info.ParameterType.IsAssignableTo(typeof(TInjected))) return null;
        
        TInjected? injected = pool.FirstOrDefault(s => s!.GetType() == info.ParameterType);
        if (injected == null) 
            throw new Exception($"Could not find {info.ParameterType}, which {typeof(TObject).Name} depends on.");

        return injected;
    }
    
    private static object? InjectFromObject<TInjected>(ParameterInfo info, TInjected obj)
    {
        // Attempt to find dependencies on other services and inject them.
        if (!info.ParameterType.IsAssignableTo(typeof(TInjected))) return null;

        if (obj!.GetType() == info.ParameterType) return obj;
        return null;
    }
    
    private static Func<ParameterInfo, object?> CreateInjectorFromPool<TObject, TInjected>(IEnumerable<TInjected> pool) 
        => info => InjectFromPool<TObject, TInjected>(info, pool);
    
    private static Func<ParameterInfo, object?> CreateInjectorFromObject<TInjected>(TInjected obj) 
        => info => InjectFromObject(info, obj);

    #endregion
    
    public void AddService<TService>(params object?[] args) where TService : Service
    {
        TService? service = InjectDependencies<TService>(new[] {this._logger}.Concat(args).ToArray(), new List<Func<ParameterInfo, object?>>
        {
            CreateInjectorFromPool<TService, Service>(this._services),
            CreateInjectorFromPool<TService, Config>(this._configs),
        });

        if (service == null) throw new InvalidOperationException("Service failed to initialize correctly.");
        
        this.AddService(service);
    }
    
    public void AddService(Service service)
    {
        this._services.Add(service);
    }

    public void AddAuthenticationService(IAuthenticationProvider<IUser, IToken>? provider = null,
        bool assumeAuthenticationRequired = false)
    {
        this.AddService<AuthenticationService>(provider, assumeAuthenticationRequired);
    }

    public void AddStorageService(IDataStore dataStore)
    {
        this.AddService<StorageService>(dataStore);
    }

    public void AddStorageService<TDataStore>() where TDataStore : IDataStore
    {
        this.AddService<StorageService>(Activator.CreateInstance<TDataStore>());
    }

    public void AddRateLimitService(RateLimitSettings? settings = null, ITimeProvider? timeProvider = null)
    {
        settings ??= RateLimitSettings.DefaultSettings;
        timeProvider ??= new RealTimeProvider();

        IRateLimiter rateLimiter = new RateLimiter(timeProvider, settings.Value);
        this.AddService<RateLimitService>(rateLimiter);
    }

    public void AddHealthCheckService(IEnumerable<Type>? checkTypes = null, bool addGeneralCheck = true)
    {
        List<IHealthCheck> checks = new();
        if (checkTypes != null)
        {
            IEnumerable<Type> checkTypesList = checkTypes.ToList();
            
            if (checkTypesList.Any(check => check.BaseType != typeof(IHealthCheck)))
                throw new InvalidOperationException($"Cannot use a health check that is not an {nameof(IHealthCheck)}");
            
            foreach (Type type in checkTypesList)
            {
                IHealthCheck? healthCheck = InjectDependencies<IHealthCheck>(type, Array.Empty<object>(), new[]
                {
                    CreateInjectorFromObject(this._databaseProvider),
                });
                
                if (healthCheck == null)
                {
                    this._logger.LogWarning(BunkumContext.Health, $"Health Check {type.Name} failed to initialize.");
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
        
        this.AddEndpointGroup<HealthCheckEndpoints>();
        this.AddService<HealthService>(checks);
    }

    #region Obsolete
    // ReSharper disable UnusedParameter.Global

    [Obsolete($"Instead of using UseAuthenticationProvider, the new method is adding a {nameof(AuthenticationService)}. See AddService.", true)]
    public void UseAuthenticationProvider(IAuthenticationProvider<IUser, IToken> provider)
    {
        throw new InvalidOperationException("UseAuthenticationProvider is obsolete.");
    }
    
    [Obsolete($"Instead of using UseAuthenticationProvider, the new method is adding a {nameof(StorageService)}. See AddService.", true)]
    public void UseDataStore(IDataStore dataStore)
    {
        throw new InvalidOperationException("UseDataStore is obsolete.");
    }
    #endregion
}