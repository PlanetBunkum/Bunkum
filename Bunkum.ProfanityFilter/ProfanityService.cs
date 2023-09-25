using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using ProfanityFilter.Interfaces;
using Filter = ProfanityFilter.ProfanityFilter;

namespace Bunkum.ProfanityFilter;

public class ProfanityService : EndpointService
{
    internal ProfanityService(Logger logger, string[] allowList, string[] extraDenyList) : base(logger)
    {
        this._profanityFilter = new Filter();
        
        foreach (string s in allowList) this._profanityFilter.AllowList.Add(s);
        this._profanityFilter.AddProfanity(extraDenyList);
    }

    private readonly IProfanityFilter _profanityFilter;

    public bool SentenceContainsProfanity(string input)
    {
        return this._profanityFilter.DetectAllProfanities(input, true).Count > 0;
    }

    public string CensorSentence(string input)
    {
        return this._profanityFilter.CensorString(input, '*');
    }
}