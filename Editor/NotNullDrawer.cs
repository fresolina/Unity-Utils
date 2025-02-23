using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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
        public static void DrawRedBorderIfNull(Rect position, Object obj) {
            if (obj != null) return;

            Rect borderRect = new Rect(
                position.x - BorderThickness,
                position.y - BorderThickness,
                position.width + (BorderThickness * 2),
                position.height + (BorderThickness * 2)
            );
            EditorGUI.DrawRect(borderRect, s_borderColor);
            EditorGUI.DrawRect(position, s_backgroundColor);
        }

        // Unity does not provide access to this color, so hard code it.
        static Color GetDefaultBackgroundColor() {
            float intensity = EditorGUIUtility.isProSkin ? 0.22f : 0.76f;
            return new Color(intensity, intensity, intensity, 1f);
        }
    }

    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public class NotNullDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            NotNullDrawerHelper.DrawRedBorderIfNull(position, property.objectReferenceValue);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }
    }
}
