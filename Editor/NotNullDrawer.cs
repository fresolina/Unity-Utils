#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lotec.Utils.Attributes.Editors {
    public static class NotNullDrawerHelper {
        const float BorderThickness = 2f;
        static readonly Color s_borderColor = Color.red;
        static readonly Color s_backgroundColor = GetDefaultBackgroundColor();

        /// <summary>
        /// Draw red border around a field in the inspector if it is null.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="obj"></param>
        public static void DrawRedBorder(Rect position) {
            Rect borderRect = new Rect(
                position.x - BorderThickness,
                position.y - BorderThickness,
                position.width + (BorderThickness * 2),
                position.height + (BorderThickness * 2)
            );
            EditorGUI.DrawRect(borderRect, s_borderColor);
            EditorGUI.DrawRect(position, s_backgroundColor);
        }

        /// <summary>
        /// Apply red border styling to a UI Toolkit VisualElement if the value is null.
        /// </summary>
        /// <param name="element">The VisualElement to style</param>
        /// <param name="value">The object value to check for null</param>
        public static void ApplyNotNullBorder(VisualElement element, bool isNull) {
            if (isNull) {
                element.style.borderLeftColor = s_borderColor;
                element.style.borderRightColor = s_borderColor;
                element.style.borderTopColor = s_borderColor;
                element.style.borderBottomColor = s_borderColor;
                element.style.borderLeftWidth = BorderThickness;
                element.style.borderRightWidth = BorderThickness;
                element.style.borderTopWidth = BorderThickness;
                element.style.borderBottomWidth = BorderThickness;
            } else {
                element.style.borderLeftWidth = 0;
                element.style.borderRightWidth = 0;
                element.style.borderTopWidth = 0;
                element.style.borderBottomWidth = 0;
            }
        }

        // Unity does not provide access to this color, so hard code it.
        static Color GetDefaultBackgroundColor() {
            float intensity = EditorGUIUtility.isProSkin ? 0.22f : 0.76f;
            return new Color(intensity, intensity, intensity, 1f);
        }
    }

    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public class NotNullDrawer : PropertyDrawer {
        // UI Toolkit implementation
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var propertyField = new PropertyField(property);

            // Apply initial border state
            NotNullDrawerHelper.ApplyNotNullBorder(propertyField, IsNull(property));

            // Register callback for value changes
            propertyField.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt) => {
                NotNullDrawerHelper.ApplyNotNullBorder(propertyField, evt.newValue == null);
            });

            return propertyField;
        }

        // IMGUI implementation
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (IsNull(property)) {
                NotNullDrawerHelper.DrawRedBorder(position);
            }

            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }

        // Check if the property value is "null"
        bool IsNull(SerializedProperty property) {
            return property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null
                || property.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(property.stringValue)
                || property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null;
        }
    }
}
#endif
