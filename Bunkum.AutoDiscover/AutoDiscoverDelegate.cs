using Bunkum.Core;

namespace Bunkum.AutoDiscover;

/// <summary>
/// The function to use to determine the URL.
/// </summary>
/// <param name="game">
/// The TitleID or identifier of the game to be patched.
/// </param>
public delegate string AutoDiscoverDelegate(RequestContext context, string? game);