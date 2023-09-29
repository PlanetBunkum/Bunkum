namespace Bunkum.Core.Authentication.Dummy;

public class DummyToken : IToken<DummyUser>
{
    public DummyUser User { get; } = new();
}