using System.Reflection;

namespace Bunkum.HttpServer.Extensions;

public static class MethodInfoExtensions
{
    public static bool HasCustomAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
    {
        return method.GetCustomAttribute<TAttribute>() != null;
    }
}