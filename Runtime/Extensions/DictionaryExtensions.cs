using System.Collections.Generic;

namespace Lotec.Utils.Extensions {
    public static class DictionaryExtensions {
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> first, Dictionary<TKey, TValue> second) {
            if (second == null || first == null) return;
            foreach (KeyValuePair<TKey, TValue> item in second) {
                if (!first.ContainsKey(item.Key))
                    first.Add(item.Key, item.Value);
            }
        }
    }
}
