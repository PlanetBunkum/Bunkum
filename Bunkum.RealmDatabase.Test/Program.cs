using System.Diagnostics;
using Bunkum.RealmDatabase.Test;

using TestDatabaseProvider provider = new();
provider.Initialize();

using TestDatabaseContext context = provider.GetContext();

context.CreateTest();
context.CreateTest();

Debug.Assert(context.GetTests().Count() == 2);