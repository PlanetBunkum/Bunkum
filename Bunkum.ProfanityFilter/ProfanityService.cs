using Bunkum.Core.Services;
using NotEnoughLogs;
using ProfanityFilter.Interfaces;
using Filter = ProfanityFilter.ProfanityFilter;

namespace Bunkum.ProfanityFilter;

/// <summary>
/// A wrapper around Profanity.Detector for Bunkum
/// </summary>
[Obsolete(DeprecationWarning.Reason)]
public class ProfanityService : EndpointService
{
    [Obsolete(DeprecationWarning.Reason)]
    internal ProfanityService(Logger logger, string[] allowList, string[] extraDenyList) : base(logger)
    {
        this._profanityFilter = new Filter();
        
        foreach (string s in allowList) this._profanityFilter.AllowList.Add(s);
        this._profanityFilter.AddProfanity(extraDenyList);
    }

    private readonly IProfanityFilter _profanityFilter;

    /// <summary>
    /// Checks if the input contains any blacklisted terms
    /// </summary>
    /// <param name="input">The input to check against.</param>
    /// <returns>true if the input contains any detected profanity, false if not.</returns>
    [Obsolete(DeprecationWarning.Reason)]
    public bool SentenceContainsProfanity(string input)
    {
        return this._profanityFilter.DetectAllProfanities(input, true).Count > 0;
    }

    /// <summary>
    /// Replaces all profanity in the input with asterisks ('*').
    /// </summary>
    /// <param name="input">The input to replace.</param>
    /// <returns>The input, with profanity censored using asterisks ('*').</returns>
    [Obsolete(DeprecationWarning.Reason)]
    public string CensorSentence(string input)
    {
        return this._profanityFilter.CensorString(input, '*');
    }
}