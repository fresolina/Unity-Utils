using System;
using System.Reflection;
using UnityEngine;

namespace Lotec.Utils.Attributes {
    /// <summary>
    /// Custom attribute to validate if a field is not null.
    /// Marks the field in red if it is null in the inspector.
    /// </summary>
    public class NotNullAttribute : PropertyAttribute { }

    public static class NotNullExtension {
        /// <summary>
        /// Helper to validate all NotNull fields on a MonoBehaviour. Call this in OnValidate().
        /// Example: void OnValidate() => this.AssertNotNullFields();
        /// </summary>
        public static void AssertNotNullFields(this object obj) {
            Type targetType = obj.GetType();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields) {
                NotNullAttribute[] attributes = (NotNullAttribute[])field.GetCustomAttributes(typeof(NotNullAttribute), inherit: true);

                if (attributes.Length > 0) {
                    var value = field.GetValue(obj);
                    // Assert.IsNotNull(value, message);
                    if (value == null) {
                        string message = $"{targetType.Name}: '{field.Name}' ({field.FieldType.Name}) missing.";
                        Debug.LogError(message, obj as UnityEngine.Object);
                    }
                }
            }
        }

        /// <summary>
        /// Automatically assigns components to any [NotNull] fields that are null by using GetComponent.
        /// Example: void OnValidate() => this.SetNotNullFields();
        /// </summary>
        /// <param name="obj">The MonoBehaviour-derived object to set fields on.</param>
        public static void SetNotNullFields(this MonoBehaviour obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj), "The MonoBehaviour to set fields on cannot be null.");
            }

            Type targetType = obj.GetType();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields) {
                NotNullAttribute[] attributes = (NotNullAttribute[])field.GetCustomAttributes(typeof(NotNullAttribute), inherit: true);
                if (attributes.Length == 0) continue; // Only process fields with the [NotNull] attribute
                if (field.GetValue(obj) != null) continue; // Only attempt to set if the field is currently null
                Type fieldType = field.FieldType;
                if (!typeof(Component).IsAssignableFrom(fieldType) && !fieldType.IsInterface) continue; // Skip Assets

                // Try get missing NotNull component from the GameObject
                Component component = obj.GetComponentInChildren(fieldType);
                if (component != null && component != obj) {
                    field.SetValue(obj, component);
                    Debug.Log($"Automatically assigned '{fieldType.Name}' to field '{field.Name}' on '{obj}'.", obj.gameObject);
                } else {
                    Debug.LogWarning($"[NotNull] field '{field.Name}' of type '{fieldType.Name}' requires a component that was not found on '{obj}'.", obj.gameObject);
                }
            }
        }
    }
}
