using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Database;
using Realms;

namespace Bunkum.RealmDatabase;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public abstract class RealmDatabaseContext : IDatabaseContext
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once InconsistentNaming
    protected Realm _realm { get; private set; } = null!;

    internal void InitializeContext(Realm realm)
    {
        this._realm = realm;
    }

    public void Refresh()
    {
        this._realm.Refresh();
    }
    
    public void Dispose()
    {
        //NOTE: we dont dispose the realm here, because the same thread may use it again, so we just `Refresh()` it
        this.Refresh();
    }
}