using System.Diagnostics.CodeAnalysis;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum BunkumCategory
{
    Startup,
    Authentication,
    Configuration,
    Filter,
    Digest,
    
    Server,
    Service,
    Middleware,
    Request,
    Health,
    Api,
    Game,
    Matching,
    Commands,
    
    UserContent,
    UserPhotos,
    UserLevels,
    LevelCategories,
    LevelScores,
}