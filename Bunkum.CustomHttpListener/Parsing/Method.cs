namespace Bunkum.CustomHttpListener.Parsing;

public enum Method
{
    Invalid,
    Get,
    Put,
    Post,
    Delete,
    Head,
    Options,
    Trace,
    Patch,
    Connect,
}

public static class MethodUtils
{
    public static Method FromString(string str)
    {
        return Enum.GetValues<Method>().FirstOrDefault(m => m.ToString().ToUpperInvariant() == str);
    }
}