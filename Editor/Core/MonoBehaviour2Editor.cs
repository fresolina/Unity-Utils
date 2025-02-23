#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Lotec.Utils.Attributes.Editors; // Ensure you include the namespace for ScriptTooltipAttribute

namespace Lotec.Utils {
    /// <summary>
    /// Custom Editor for MonoBehaviour2 to handle interface fields and tooltips while preserving default inspector behavior.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour2), true)]
    public class MonoBehaviour2Editor : Editor {
        // Dictionary to map interface field names to their corresponding FieldInfo
        private Dictionary<string, FieldInfo> _interfaceFieldMap;

        // SerializedProperty for the _serializedInterfaceFields list
        private SerializedProperty _interfaceFieldsProp;

        // List of all serialized fields sorted by their declaration order
        private List<FieldInfo> _sortedSerializedFields;

        // Tooltip text for the script field
        private string _scriptTooltip;

        private void OnEnable() {
            _interfaceFieldMap = new Dictionary<string, FieldInfo>();

            // Retrieve all fields (public and private) from the target's type
            var allFields = target.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Filter serialized fields
            var serializedFields = new List<FieldInfo>();
            foreach (var field in allFields) {
                if (!field.IsPublic && !System.Attribute.IsDefined(field, typeof(SerializeField))) continue;

                serializedFields.Add(field);

                // If the field is an interface, map it
                if (field.FieldType.IsInterface) {
                    _interfaceFieldMap[field.Name] = field;
                }
            }

            // Sort fields by MetadataToken to preserve declaration order
            _sortedSerializedFields = new List<FieldInfo>(serializedFields);
            _sortedSerializedFields.Sort((f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

            // Find the _serializedInterfaceFields property
            _interfaceFieldsProp = serializedObject.FindProperty("_serializedInterfaceFields");

            // Retrieve the ScriptTooltipAttribute from the target class
            ScriptTooltipAttribute scriptTooltipAttr = (ScriptTooltipAttribute)System.Attribute.GetCustomAttribute(target.GetType(), typeof(ScriptTooltipAttribute));
            if (scriptTooltipAttr != null) {
                _scriptTooltip = scriptTooltipAttr.Tooltip;
            } else {
                // Default tooltip if none is provided
                _scriptTooltip = "The script associated with this component.";
            }
        }

        public override void OnInspectorGUI() {
            // Update the serialized object
            serializedObject.Update();

            // Draw the script field with custom tooltip
            DrawScriptField();

            // Iterate through sorted fields
            foreach (var field in _sortedSerializedFields) {
                // If the field is an interface, draw it with custom logic
                if (_interfaceFieldMap.ContainsKey(field.Name)) {
                    DrawInterfaceField(field);
                } else {
                    // Draw the default field using SerializedProperty
                    DrawDefaultField(field);
                }
            }

            // Apply any modified properties
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the script field with a custom tooltip.
        /// </summary>
        private void DrawScriptField() {
            SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");

            EditorGUI.BeginDisabledGroup(true);

            // Create a GUIContent with the custom tooltip
            GUIContent scriptLabel = new GUIContent("Script", _scriptTooltip);

            // Draw the script field with the custom label
            EditorGUILayout.PropertyField(scriptProp, scriptLabel, true);

            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Draws a default serialized field using Unity's default property drawing.
        /// Preserves existing [Tooltip] attributes on the field.
        /// </summary>
        /// <param name="field">The FieldInfo of the field to draw.</param>
        private void DrawDefaultField(FieldInfo field) {
            SerializedProperty property = serializedObject.FindProperty(field.Name);
            if (property != null) {
                // Retrieve TooltipAttribute if it exists
                TooltipAttribute tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
                string tooltipText = tooltipAttr != null ? tooltipAttr.tooltip : "";

                // Create GUIContent with field name and tooltip
                GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(field.Name), tooltipText);

                EditorGUILayout.PropertyField(property, label, true);
            }
        }

        /// <summary>
        /// Draws an interface field with custom assignment logic and enhanced labeling.
        /// Preserves existing [Tooltip] attributes on the field.
        /// </summary>
        /// <param name="field">The FieldInfo of the interface field.</param>
        private void DrawInterfaceField(FieldInfo field) {
            // Find the corresponding entry in _serializedInterfaceFields list
            int index = FindInterfaceFieldIndex(field.Name);

            // Guard Clause: Skip if no corresponding entry found
            if (index == -1) return;

            SerializedProperty entry = _interfaceFieldsProp.GetArrayElementAtIndex(index);
            SerializedProperty fieldNameProp = entry.FindPropertyRelative("fieldName");
            SerializedProperty objectRefProp = entry.FindPropertyRelative("objectRef");

            // Create a custom label that includes the interface name
            string labelText = $"{ObjectNames.NicifyVariableName(fieldNameProp.stringValue)} ({field.FieldType.Name})";
            GUIContent label = new GUIContent(labelText, field.GetCustomAttribute<TooltipAttribute>()?.tooltip);

            EditorGUI.BeginChangeCheck();

            // Don't know how to merge layout of NotNull attribute creating a red border, so hack it and add check also here.
            Rect controlRect = EditorGUILayout.GetControlRect(true);
            bool hasNotNull = System.Attribute.IsDefined(field, typeof(Attributes.NotNullAttribute));
            if (hasNotNull) {
                NotNullDrawerHelper.DrawRedBorderIfNull(controlRect, objectRefProp.objectReferenceValue);
            }
            // Draw the Object field with the custom label and optionally red border from NotNull
            EditorGUI.PropertyField(controlRect, objectRefProp, label, true);

            // If the user has changed the object
            if (EditorGUI.EndChangeCheck()) {
                Object newValue = objectRefProp.objectReferenceValue;
                Object objectToAssign = GetObjectToAssign(newValue, field);
                objectRefProp.objectReferenceValue = objectToAssign;
                field.SetValue(target, objectToAssign);
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Finds the index of the interface field in the _serializedInterfaceFields list.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>The index of the field in the list or -1.</returns>
        private int FindInterfaceFieldIndex(string fieldName) {
            for (int i = 0; i < _interfaceFieldsProp.arraySize; i++) {
                SerializedProperty entry = _interfaceFieldsProp.GetArrayElementAtIndex(i);
                SerializedProperty fname = entry.FindPropertyRelative("fieldName");
                if (fname.stringValue == fieldName)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Determines the appropriate object to assign to the interface field.
        /// </summary>
        /// <param name="newValue">The new value assigned by the user.</param>
        /// <param name="field">The FieldInfo of the interface field.</param>
        /// <returns>The object to assign to the field.</returns>
        private Object GetObjectToAssign(Object newValue, FieldInfo field) {
            if (newValue == null) {
                return null;
            }

            // Directly assign if the newValue implements the interface
            if (field.FieldType.IsAssignableFrom(newValue.GetType())) {
                return newValue;
            }

            // Attempt to find a component that implements the interface
            Component componentToAssign = null;

            if (newValue is Component component) {
                componentToAssign = component.GetComponent(field.FieldType);
            } else if (newValue is GameObject gameObject) {
                componentToAssign = gameObject.GetComponent(field.FieldType);
            }

            if (componentToAssign == null) {
                // Show a warning dialog if no valid component is found
                EditorUtility.DisplayDialog(
                    "Invalid Assignment",
                    $"The assigned object must implement {field.FieldType.Name} directly or have a component that does.",
                    "OK"
                );
            }
            return componentToAssign;
        }
    }
}
#endif
