using Bunkum.HttpServer.Database;
using Realms;

namespace Bunkum.RealmDatabase.Test;

public class TestDatabaseContext : RealmDatabaseContext
{
    public Realm Realm => this._realm; // DON'T DO THIS, ITS JUST FOR TESTING

    public TestModel CreateTest()
    {
        TestModel model = new();
        this._realm.Write(() =>
        {
            this._realm.Add(model);
        });

        return model;
    }

    public IEnumerable<TestModel> GetTests()
    {
        return this._realm.All<TestModel>();
    }
}