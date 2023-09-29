using Bunkum.Core;
using Bunkum.Core.Database;
using Realms;

namespace Bunkum.RealmDatabase;

// ReSharper disable once UnusedType.Global
public abstract class RealmDatabaseProvider<TContext> : IDatabaseProvider<TContext> where TContext : RealmDatabaseContext
{
    private RealmConfigurationBase _configuration = null!;

    /// <summary>
    /// The version of your database's schema. Increment this when you make changes to your database.
    /// </summary>
    protected abstract ulong SchemaVersion { get; }
    /// <summary>
    /// A list of types that appear in your database's schema.
    /// </summary>
    protected abstract List<Type> SchemaTypes { get; }
    
    /// <summary>
    /// The filename of your realm database. For example, <c>gameServer.realm</c>.
    /// </summary>
    protected abstract string Filename { get; }
    
    /// <summary>
    /// Allows the Realm to be persisted in memory instead of being attached to a file. Useful for integration tests.
    /// </summary>
    protected virtual bool InMemory => false;

    public void Initialize()
    {
        if (this.InMemory)
        {
            this._configuration = new InMemoryConfiguration(Path.Join(Path.GetTempPath(), this.Filename))
            {
                SchemaVersion = this.SchemaVersion,
                Schema = this.SchemaTypes,
                // no migrations for in-memory realms
            };
            return;
        }
        
        this._configuration = new RealmConfiguration(Path.Join(BunkumFileSystem.DataDirectory, this.Filename))
        {
            SchemaVersion = this.SchemaVersion,
            Schema = this.SchemaTypes,
            MigrationCallback = this.Migrate,
        };
    }
    
    public abstract void Warmup();

    protected abstract void Migrate(Migration migration, ulong oldVersion);
    
    private readonly ThreadLocal<Realm> _realmStorage = new(true);

    protected virtual TContext CreateContext() => Activator.CreateInstance<TContext>();

    public TContext GetContext()
    {
        this._realmStorage.Value ??= Realm.GetInstance(this._configuration);
        this._realmStorage.Value.Refresh();
        
        TContext context = this.CreateContext();
        context.InitializeContext(this._realmStorage.Value);

        return context;
    }
    
    public void Dispose()
    {
        foreach (Realm realmStorageValue in this._realmStorage.Values) 
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            realmStorageValue?.Dispose();
        }

        this._realmStorage.Dispose();
    }
}