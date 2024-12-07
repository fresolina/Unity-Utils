using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Lotec.Utils.Attributes.Editors {
    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public class NotNullDrawer : PropertyDrawer {
        const float BorderThickness = 2f;
        Color _borderColor = Color.red;
        Color _backgroundColor = GetDefaultBackgroundColor();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Check if the property is null
            if (property.objectReferenceValue == null) {
                Rect borderRect = new(position.x - BorderThickness, position.y - BorderThickness,
                    position.width + (BorderThickness * 2), position.height + (BorderThickness * 2));

                EditorGUI.DrawRect(borderRect, _borderColor);

                Rect backgroundRect = new(position.x, position.y, position.width, position.height);
                EditorGUI.DrawRect(backgroundRect, _backgroundColor);
            }

            EditorGUI.PropertyField(position, property, label);

            EditorGUI.EndProperty();
        }

        // Unity does not provide access to this color, so hard code it.
        static Color GetDefaultBackgroundColor() {
            float kViewBackgroundIntensity = EditorGUIUtility.isProSkin ? 0.22f : 0.76f;
            return new Color(kViewBackgroundIntensity, kViewBackgroundIntensity, kViewBackgroundIntensity, 1f);
        }
    }
    public static class NotNullExtension {
        public static void AssertNotNullFields(this object obj) {
            System.Type targetType = obj.GetType();
            System.Reflection.FieldInfo[] fields = targetType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (System.Reflection.FieldInfo field in fields) {
                NotNullAttribute[] attributes = (NotNullAttribute[])field.GetCustomAttributes(typeof(NotNullAttribute), inherit: true);

                if (attributes.Length > 0) {
                    Assert.IsNotNull(field.GetValue(obj), "Required reference missing");
                }
            }
        }
    }
}
