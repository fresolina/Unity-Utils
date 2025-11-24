using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Utils {
    /// <summary>
    /// A serializable dictionary that can be used in Unity.
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField] TKey[] _keys;
        [SerializeField] TValue[] _values;

        public void OnAfterDeserialize() {
            Clear();
            if (_keys != null && _values != null) {
                for (int i = 0; i < _keys.Length; i++) {
                    this[_keys[i]] = _values[i];
                }
            }
        }

        public void OnBeforeSerialize() {
            _keys = new TKey[Count];
            _values = new TValue[Count];

            int index = 0;
            foreach (var kvp in this) {
                _keys[index] = kvp.Key;
                _values[index] = kvp.Value;
                index++;
            }
        }
    }
}
