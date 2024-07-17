using Bunkum.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Bunkum.EntityFrameworkDatabase;

public abstract class EntityFrameworkDatabaseProvider<TDatabaseContext> : IDatabaseProvider<TDatabaseContext>
    where TDatabaseContext : DbContext, IDatabaseContext
{
    protected abstract EntityFrameworkInitializationStyle InitializationStyle { get; }

    public virtual void Initialize()
    {
        switch (this.InitializationStyle)
        {
            case EntityFrameworkInitializationStyle.Migrate:
                this.GetContext().Database.Migrate();
                break;
            case EntityFrameworkInitializationStyle.EnsureCreated:
                this.GetContext().Database.EnsureCreated();
                break;
            case EntityFrameworkInitializationStyle.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Warmup()
    {}

    public virtual TDatabaseContext GetContext() => Activator.CreateInstance<TDatabaseContext>();
    
    public void Dispose()
    {
        
    }
}