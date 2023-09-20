using MongoDB.Bson;
using Realms;

namespace Bunkum.RealmDatabase.Test;

public partial class TestModel : IRealmObject
{
    public ObjectId TestId { get; set; } = ObjectId.GenerateNewId();
    public int Value { get; set; }
}