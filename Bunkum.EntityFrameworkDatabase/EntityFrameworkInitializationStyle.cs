namespace Bunkum.EntityFrameworkDatabase;

/// <summary>
/// The method used for initializing Entity Framework databases.
/// </summary>
/// <remarks>
/// You can override <see cref="EntityFrameworkDatabaseProvider{TDatabaseContext}.Initialize"/> to define your own behavior.
/// </remarks>
public enum EntityFrameworkInitializationStyle
{
    /// <summary>
    /// Automatically perform database migrations on startup. Will create the database if it does not already exist.
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.relationaldatabasefacadeextensions.migrate
    /// </summary>
    Migrate = 0,
    /// <summary>
    /// Ensures that the database for the context exists, creating it if it does not exist.
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade.ensurecreated
    /// </summary>
    EnsureCreated = 1,
    /// <summary>
    /// No migrations/creations will occur; the database is assumed to already exist and be on the latest version.
    /// </summary>
    None = 2,
}