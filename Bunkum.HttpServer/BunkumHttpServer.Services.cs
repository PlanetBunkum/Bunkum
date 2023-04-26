using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Services;
using Bunkum.HttpServer.Storage;
using Bunkum.HttpServer.Time;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class BunkumHttpServer // Services
{
    public void AddService<TService>(params object?[] args) where TService : Service
    {
        List<object?> fullArgs = new object?[] { this._logger }.Concat(args).ToList();
        
        // Find a suitable constructor for the given parameters. This accounts for null parameters.
        // Some additional logic is present to check for references to services and automatically inject them if present.
        ConstructorInfo? ctor = typeof(TService).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(info =>
            {
                List<object?> newArgs = new(fullArgs);
                ParameterInfo[] paramInfos = info.GetParameters();

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    // Attempt to find dependencies on other services and inject them.
                    if (paramInfos[i].ParameterType.IsAssignableTo(typeof(Service)))
                    {
                        Service? service = this._services.FirstOrDefault(s => s.GetType() == paramInfos[i].ParameterType);
                        if (service == null) 
                            throw new Exception($"Could not find {paramInfos[i].ParameterType}, which {typeof(TService).Name} depends on.");
                        
                        newArgs.Insert(i, service);
                        continue;
                    }
                    
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
        
        TService? service = (TService?)ctor?.Invoke(fullArgs.ToArray());

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