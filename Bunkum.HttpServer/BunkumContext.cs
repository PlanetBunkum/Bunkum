using System.Diagnostics.CodeAnalysis;

namespace Bunkum.HttpServer;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum BunkumContext
{
    Startup,
    Authentication,
    Configuration,
    Filter,
    Digest,
    
    Service,
    Middleware,
    Request,
    Health,
    Api,
    Game,
    Matching,
    
    UserContent,
    UserPhotos,
    UserLevels,
    LevelCategories,
    LevelScores,
}