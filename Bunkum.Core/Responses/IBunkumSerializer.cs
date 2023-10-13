namespace Bunkum.Core.Responses;

/// <summary>
/// Defines a serializer for a set of content types.
/// </summary>
public interface IBunkumSerializer
{
    /// <summary>
    /// The list of content types to be handled with this <see cref="IBunkumSerializer"/> 
    /// </summary>
    string[] ContentTypes { get; }

    byte[] Serialize(object data);
}