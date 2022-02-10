using System.Collections.Generic;

/// extensions for dictionaries and related types
static class DictionaryExt {
    /// get the value for the key, or the default if none exists
    public static V Get<K, V>(this Dictionary<K, V> dict, K key) {
        _ = dict.TryGetValue(key, out var val);
        return val;
    }

    /// deconstruct a dictionary's key-value pair
    public static void Deconstruct<K, V>(this KeyValuePair<K, V> p, out K k, out V v) {
        k = p.Key;
        v = p.Value;
    }
}