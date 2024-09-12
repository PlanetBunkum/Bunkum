using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Bunkum.Core.Authentication;
using Bunkum.Core.Configuration;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using JetBrains.Annotations;

namespace Bunkum.Core;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class BunkumServer // Services
{
    #region Dependency Injection

    [Pure]
    public static TObject? InjectDependencies<TObject>(object?[] args, IEnumerable<Func<ParameterInfo, object?>> injectorLists) where TObject : class 
        => InjectDependencies<TObject>(typeof(TObject), args, injectorLists);

    [Pure]
    public static TObject? InjectDependencies<TObject>(Type type, object?[] args, IEnumerable<Func<ParameterInfo, object?>> injectorLists) where TObject : class
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

                    if (newArgs.Count <= i)
                        return false;

                    if (newArgs[i] == null)
                    {
                        if (paramInfos[i].ParameterType.IsValueType)
                            return false;
                    }
                    else if (!paramInfos[i].ParameterType.IsInstanceOfType(newArgs[i]))
                        return false;
                }

                if (newArgs.Count != paramInfos.Length)
                    return false;
                
                fullArgs = newArgs;
                return true;
            });

        if (ctor == null) return null;
        TObject? service = (TObject?)ctor.Invoke(fullArgs.ToArray());

        return service;
    }
    
    public static object? InjectFromPool<TObject, TInjected>(ParameterInfo info, IEnumerable<TInjected> pool)
    {
        // Attempt to find dependencies on other services and inject them.
        if (!info.ParameterType.IsAssignableTo(typeof(TInjected))) return null;
        
        TInjected? injected = pool.FirstOrDefault(s => s!.GetType() == info.ParameterType);
        if (injected == null)
        {
            // NullabilityInfoContext isn't thread-safe, so it cant be re-used
            // https://stackoverflow.com/a/72586919
            // TODO: do benchmarks of this call to see if we should optimize
            bool isNullable = new NullabilityInfoContext().Create(info).WriteState == NullabilityState.Nullable;
            
            if(!isNullable)
                throw new Exception($"Could not find {info.ParameterType}, which {typeof(TObject).Name} depends on.");
        }

        return injected;
    }
    
    public static object? InjectFromObject<TInjected>(ParameterInfo info, TInjected obj)
    {
        // Attempt to find dependencies on other services and inject them.
        if (!info.ParameterType.IsAssignableTo(typeof(TInjected))) return null;
        return obj;
    }
    
    public static Func<ParameterInfo, object?> CreateInjectorFromPool<TObject, TInjected>(IEnumerable<TInjected> pool) 
        => info => InjectFromPool<TObject, TInjected>(info, pool);
    
    public static Func<ParameterInfo, object?> CreateInjectorFromObject<TInjected>(TInjected obj) 
        => info => InjectFromObject(info, obj);

    #endregion
    
    public void AddService<TService>(params object?[] args) where TService : Service
    {
        TService? service = InjectDependencies<TService>(new[] {this.Logger}.Concat(args).ToArray(), new List<Func<ParameterInfo, object?>>
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

    public void AddAuthenticationService(IAuthenticationProvider<IToken<IUser>>? provider = null,
        bool assumeAuthenticationRequired = false,
        HttpStatusCode failureStatusCode = HttpStatusCode.Forbidden)
    {
        this.AddService<AuthenticationService>(provider, assumeAuthenticationRequired, failureStatusCode);
    }

    public void AddStorageService(IDataStore dataStore)
    {
        this.AddService<StorageService>(dataStore);
    }

    public void AddStorageService<TDataStore>() where TDataStore : IDataStore
    {
        this.AddService<StorageService>(Activator.CreateInstance<TDataStore>());
    }

    public void AddRateLimitService(RateLimitSettings? settings = null, TimeProvider? timeProvider = null)
    {
        settings ??= RateLimitSettings.DefaultSettings;
        timeProvider ??= TimeProvider.System;

        IRateLimiter rateLimiter = new RateLimiter(timeProvider, settings.Value);
        this.AddService<RateLimitService>(rateLimiter);
    }
}