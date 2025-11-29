namespace WeaponSkins;

internal static class DictionaryExtensions
{
    public static bool UpdateOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
        where TValue : class, IEquatable<TValue>
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            if (existingValue.Equals(value))
            {
                return false;
            }
        }

        dictionary[key] = value;
        return true;
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> valueFactory)
        where TValue : class
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        value = valueFactory();
        dictionary[key] = value;
        return value;
    }
}

