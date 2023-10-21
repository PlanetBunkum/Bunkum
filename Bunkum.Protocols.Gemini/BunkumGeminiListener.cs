using Bunkum.Listener;
using Bunkum.Listener.Request;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gemini;

public abstract class BunkumGeminiListener : BunkumListener
{
    protected BunkumGeminiListener(Logger logger) : base(logger)
    {}
}