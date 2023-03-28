namespace Bunkum.HttpServer.Serialization;

/// <summary>
/// This interface exposes a method to Bunkum, which is is called just before serialization takes place.
/// This is helpful if there are extra properties that need to be set that exist outside your database.
/// </summary>
public interface INeedsPreparationBeforeSerialization
{
    public void PrepareForSerialization();
}