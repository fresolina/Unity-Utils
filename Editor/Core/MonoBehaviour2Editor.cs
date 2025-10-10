#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lotec.Utils {
    /// <summary>
    /// Custom Editor for MonoBehaviour2 to handle interface fields and tooltips while preserving default inspector behavior.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour2), true)]
    public class MonoBehaviour2Editor : UnityEditor.Editor {
        // Dictionary to map interface field names to their corresponding FieldInfo
        private Dictionary<string, FieldInfo> _interfaceFieldMap;

        // SerializedProperty for the _serializedInterfaceFields list
        private SerializedProperty _interfaceFieldsProp;

        // List of all serialized fields sorted by their declaration order
        private List<FieldInfo> _sortedSerializedFields;

        // Tooltip text for the script field
        private string _scriptTooltip;

        private void OnEnable() {
            if (!EnsureTargetsReady()) return;

            InitializeInspectorData();
        }

        void OnDisable() {
            EditorApplication.delayCall -= RetryInitialize;
        }

        bool EnsureTargetsReady() {
            if (targets == null || targets.Length == 0 || targets[0] == null) {
                EditorApplication.delayCall += RetryInitialize;
                return false;
            }

            return true;
        }

        void RetryInitialize() {
            EditorApplication.delayCall -= RetryInitialize;
            if (this == null || !EnsureTargetsReady()) return;

            InitializeInspectorData();
        }

        void InitializeInspectorData() {
            _interfaceFieldMap = new Dictionary<string, FieldInfo>();

            // Retrieve all fields (public and private) from the target's type
            var allFields = target.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Filter serialized fields
            var serializedFields = new List<FieldInfo>();
            foreach (var field in allFields) {
                // Check if field is serialized (public, [SerializeField], or [SerializeReference])
                bool isSerializedField = field.IsPublic ||
                                       System.Attribute.IsDefined(field, typeof(SerializeField)) ||
                                       System.Attribute.IsDefined(field, typeof(SerializeReference));

                if (!isSerializedField) continue;

                serializedFields.Add(field);

                // If the field is an interface and NOT a SerializeReference, map it
                // SerializeReference fields have their own custom property drawers
                if (field.FieldType.IsInterface && !System.Attribute.IsDefined(field, typeof(SerializeReference))) {
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

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            // Draw the script field with custom tooltip
            CreateScriptField(root);

            // Create fields container
            var fieldsContainer = new VisualElement();
            root.Add(fieldsContainer);

            // Iterate through sorted fields
            foreach (var field in _sortedSerializedFields) {
                // Skip [SerializeReference] fields - they have their own custom property drawers and will be handled automatically
                if (System.Attribute.IsDefined(field, typeof(SerializeReference))) {
                    var property = serializedObject.FindProperty(field.Name);
                    if (property != null) {
                        var propertyField = new PropertyField(property);
                        fieldsContainer.Add(propertyField);
                    }
                    continue;
                }

                // If the field is an interface, draw it with custom logic
                if (_interfaceFieldMap.ContainsKey(field.Name)) {
                    CreateInterfaceField(fieldsContainer, field);
                } else {
                    // Draw the default field using PropertyField
                    CreateDefaultField(fieldsContainer, field);
                }
            }

            // Add debug buttons for all public methods
            CreateDebugButtons(root);

            return root;
        }

        /// <summary>
        /// Creates the script field with a custom tooltip.
        /// </summary>
        private void CreateScriptField(VisualElement container) {
            SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");

            var scriptField = new PropertyField(scriptProp, "Script");
            scriptField.tooltip = _scriptTooltip;
            scriptField.SetEnabled(false);
            container.Add(scriptField);
        }

        /// <summary>
        /// Creates a default serialized field using Unity's default property drawing.
        /// Preserves existing [Tooltip] attributes on the field.
        /// </summary>
        /// <param name="container">The container to add the field to.</param>
        /// <param name="field">The FieldInfo of the field to draw.</param>
        private void CreateDefaultField(VisualElement container, FieldInfo field) {
            SerializedProperty property = serializedObject.FindProperty(field.Name);
            if (property != null) {
                // Retrieve TooltipAttribute if it exists
                TooltipAttribute tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
                string tooltipText = tooltipAttr != null ? tooltipAttr.tooltip : "";

                var propertyField = new PropertyField(property);
                if (!string.IsNullOrEmpty(tooltipText)) {
                    propertyField.tooltip = tooltipText;
                }

                container.Add(propertyField);
            }
        }

        /// <summary>
        /// Creates an interface field with custom assignment logic and enhanced labeling.
        /// Preserves existing [Tooltip] attributes on the field.
        /// </summary>
        /// <param name="container">The container to add the field to.</param>
        /// <param name="field">The FieldInfo of the interface field.</param>
        private void CreateInterfaceField(VisualElement container, FieldInfo field) {
            // Find the corresponding entry in _serializedInterfaceFields list
            int index = FindInterfaceFieldIndex(field.Name);

            // Guard Clause: Skip if no corresponding entry found
            if (index == -1) return;

            SerializedProperty entry = _interfaceFieldsProp.GetArrayElementAtIndex(index);
            SerializedProperty fieldNameProp = entry.FindPropertyRelative("fieldName");
            SerializedProperty objectRefProp = entry.FindPropertyRelative("objectRef");

            // Create a custom label that includes the interface name
            string labelText = $"{ObjectNames.NicifyVariableName(fieldNameProp.stringValue)} ({field.FieldType.Name})";

            var propertyField = new PropertyField(objectRefProp, labelText);

            // Add tooltip if available
            TooltipAttribute tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null) {
                propertyField.tooltip = tooltipAttr.tooltip;
            }

            // Handle value changes for interface validation
            propertyField.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt) => {
                Object newValue = evt.newValue;
                Object objectToAssign = GetObjectToAssign(newValue, field);
                if (objectToAssign != newValue) {
                    objectRefProp.objectReferenceValue = objectToAssign;
                    field.SetValue(target, objectToAssign);
                    serializedObject.ApplyModifiedProperties();
                }
            });

            container.Add(propertyField);
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

        /// <summary>
        /// Creates debug buttons for all public methods in the target MonoBehaviour2 class.
        /// Shows buttons under a collapsible "Debug" foldout.
        /// </summary>
        private void CreateDebugButtons(VisualElement container) {
            // Get all public methods from the target's type
            var publicMethods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // Filter out inherited methods and methods with parameters
            List<MethodInfo> debugMethods = FilterMethods(publicMethods);

            // Create debug buttons section if there are any methods
            if (debugMethods.Count <= 0) return;

            // Add some space before debug section
            var spacer = new VisualElement();
            spacer.style.height = 10;
            container.Add(spacer);

            // Create a foldout for debug methods
            var debugFoldout = new Foldout();
            debugFoldout.text = "Debug";
            debugFoldout.value = false; // Collapsed by default
            debugFoldout.style.marginBottom = 5;

            // Create debug buttons container
            var debugContainer = new VisualElement();
            debugContainer.style.flexDirection = FlexDirection.Column;
            debugContainer.style.paddingLeft = 15; // Indent the buttons

            foreach (var method in debugMethods) {
                var button = new Button(() => {
                    try {
                        method.Invoke(target, null);
                        Debug.Log($"Called {method.Name}() on {target.name}");
                    } catch (System.Exception ex) {
                        Debug.LogError($"Error calling {method.Name}(): {ex.Message}");
                    }
                });

                button.text = $"{ObjectNames.NicifyVariableName(method.Name)}()";
                button.style.marginBottom = 2;
                debugContainer.Add(button);
            }

            debugFoldout.Add(debugContainer);
            container.Add(debugFoldout);
        }

        private static List<MethodInfo> FilterMethods(MethodInfo[] publicMethods) {
            var debugMethods = new List<MethodInfo>();
            foreach (var method in publicMethods) {
                // Skip methods inherited from base Unity classes
                if (method.DeclaringType == typeof(MonoBehaviour) ||
                    method.DeclaringType == typeof(MonoBehaviour2) ||
                    method.DeclaringType == typeof(Component) ||
                    method.DeclaringType == typeof(UnityEngine.Object) ||
                    method.DeclaringType == typeof(object)) {
                    continue;
                }

                // Skip property getters/setters and special methods
                if (method.IsSpecialName || method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) {
                    continue;
                }

                // Skip methods with parameters
                if (method.GetParameters().Length > 0) {
                    continue;
                }

                debugMethods.Add(method);
            }

            return debugMethods;
        }
    }
}
#endif
