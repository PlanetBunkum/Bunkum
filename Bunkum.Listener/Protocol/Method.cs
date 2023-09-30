using System.Reflection;

namespace Bunkum.Listener.Protocol;

public record Method(string Value)
{
    internal static readonly Method Invalid = new("_");

    public override string ToString()
    {
        return this.Value;
    }
}

public static class MethodUtils
{
    public static Method FromString(Type type, ReadOnlySpan<char> str)
    {
        foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if(field.GetValue(null) is not Method method) continue;
            if (str.SequenceEqual(method.Value)) return method;
        }

        return Method.Invalid;
    }
    
    public static Method FromEnum<TEnum>(Type type, TEnum @enum) where TEnum : Enum
    {
        foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if(field.GetValue(null) is not Method method) continue;
            if (@enum.ToString().Equals(method.Value, StringComparison.InvariantCultureIgnoreCase)) return method;
        }

        return Method.Invalid;
    }
}