using System.Collections.Generic;

public static class DictionaryExt {
    public static V Get<K, V>(this Dictionary<K, V> dictionary, K key) {
        dictionary.TryGetValue(key, out var value);
        return value;
    }
}
