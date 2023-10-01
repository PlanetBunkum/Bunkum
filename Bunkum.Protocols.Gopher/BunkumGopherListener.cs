using Bunkum.Listener;
using NotEnoughLogs;

namespace Bunkum.Protocols.Gopher;

public abstract class BunkumGopherListener : BunkumListener
{
    protected BunkumGopherListener(Logger logger) : base(logger)
    {}
}