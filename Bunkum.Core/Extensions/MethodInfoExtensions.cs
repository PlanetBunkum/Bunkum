using System.Reflection;

namespace Bunkum.Core.Extensions;

public static class MethodInfoExtensions
{
    public static bool HasCustomAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
    {
        return method.GetCustomAttribute<TAttribute>() != null;
    }
}