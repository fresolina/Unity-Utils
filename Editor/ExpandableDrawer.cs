#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lotec.Utils.Attributes {
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableDrawer : PropertyDrawer {
        UnityEditor.Editor _editor = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect foldoutRect = new Rect(position.x, position.y, 15, position.height);
            // Adjust the main position rect to make space for the foldout
            Rect propertyRect = new Rect(position.x + 15, position.y, position.width - 15, position.height);

            // Draw the foldout arrow
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

            // Draw the property field with label
            EditorGUI.PropertyField(propertyRect, property, label, true);

            if (property.objectReferenceValue == null || !property.isExpanded)
                return;

            if (!_editor)
                UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);

            // Adjust layout for the expanded content
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}
#endif
