using System;
using System.Collections.Generic;
using System.Reflection;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Utils {
    /// <summary>
    /// Enhances MonoBehaviour with support for:
    /// * serializing interface fields in Unity
    /// * tooltip on the class with [ScriptTooltip("description")] attribute
    /// </summary>
    public class MonoBehaviour2 : MonoBehaviour, ISerializationCallbackReceiver {
        [Serializable]
        class InterfaceFieldData {
            public string fieldName;
            public UnityEngine.Object objectRef;
        }

        [SerializeField]
        List<InterfaceFieldData> _serializedInterfaceFields = new List<InterfaceFieldData>();

        // Serialize Dictionaries
        List<InterfaceFieldData> _serializedDictionaryKeyFields = new List<InterfaceFieldData>();
        List<InterfaceFieldData> _serializedDictionaryValueFields = new List<InterfaceFieldData>();

#if UNITY_EDITOR
        protected virtual void OnValidate() {
            this.AssertNotNullFields();
        }

        protected virtual void Reset() {
            this.SetNotNullFields();
            OnValidate();
        }
#endif

        void SerializeInterface(FieldInfo f) {
            // Ensure field is an interface
            if (!f.FieldType.IsInterface) return;

            // Get the value and cast to Object if it's a MonoBehaviour/ScriptableObject
            var value = f.GetValue(this) as UnityEngine.Object;
            if (value != null && !f.FieldType.IsAssignableFrom(value.GetType())) return;

            _serializedInterfaceFields.Add(new InterfaceFieldData {
                fieldName = f.Name,
                objectRef = value
            });
        }

        void SerializeDictionary(FieldInfo f) {
            // Ensure field is a dictionary
            if (!f.FieldType.IsGenericType || f.FieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return;

            // Get the value and cast to Object if it's a MonoBehaviour/ScriptableObject
            // var value = f.GetValue(this) as UnityEngine.Object;
        }

        public void OnBeforeSerialize() {
            _serializedInterfaceFields.Clear();
            _serializedDictionaryKeyFields.Clear();
            _serializedDictionaryValueFields.Clear();

            // Get all fields (public, private, instance) in the derived type(s).
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var f in fields) {
                // Ensure field is supposed to be serialized.
                if (!Attribute.IsDefined(f, typeof(SerializeField))) continue;

                SerializeInterface(f);
                // SerializeDictionary(f);
            }
        }

        public void OnAfterDeserialize() {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // For each stored interface field, assign it back to the original field
            foreach (var entry in _serializedInterfaceFields) {
                FieldInfo fi = Array.Find(fields, f => f.Name == entry.fieldName && f.FieldType.IsAssignableFrom(entry.objectRef.GetType()));
                fi?.SetValue(this, entry.objectRef);
            }
        }
    }
}
