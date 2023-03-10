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
    
    Request,
    Api,
    Game,
    
    UserContent,
    UserPhotos,
    UserLevels,
    LevelCategories,
    LevelScores,
}