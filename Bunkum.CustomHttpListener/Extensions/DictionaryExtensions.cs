namespace Bunkum.CustomHttpListener.Extensions;

internal static class DictionaryExtensions
{
    internal static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) 
        where TKey : notnull
        where TValue : class
    {
        if (dict.TryGetValue(key, out TValue? value))
            return value;
        
        return null;
    }
}