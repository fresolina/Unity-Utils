#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lotec.Utils.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lotec.Utils.Editor {
    [CustomPropertyDrawer(typeof(OptionsAttribute))]
    public class OptionsPropertyDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var optionsAttribute = (OptionsAttribute)attribute;
            var targetObject = property.serializedObject.targetObject;

            // Create the root container
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            // Create the label
            var label = new Label(property.displayName);
            label.style.minWidth = EditorGUIUtility.labelWidth;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            container.Add(label);

            // Get the method that provides the options
            var method = GetMethod(targetObject, optionsAttribute.MethodName);
            if (method == null) {
                var errorLabel = new Label($"Method '{optionsAttribute.MethodName}' not found.");
                errorLabel.style.color = Color.red;
                errorLabel.style.flexGrow = 1;
                container.Add(errorLabel);
                return container;
            }

            // Get the options from the method
            var optionsData = GetOptionsData(targetObject, method, property);
            if (optionsData == null || optionsData.options.Length == 0) {
                var errorLabel = new Label("No options available from method.");
                errorLabel.style.color = Color.red;
                errorLabel.style.flexGrow = 1;
                container.Add(errorLabel);
                return container;
            }

            // Create the appropriate UI element based on property type
            VisualElement fieldElement = property.propertyType switch {
                SerializedPropertyType.String => CreateStringDropdown(property, optionsData),
                SerializedPropertyType.Integer => CreateIntDropdown(property, optionsData),
                SerializedPropertyType.Float => CreateFloatDropdown(property, optionsData),
                SerializedPropertyType.Enum => CreateEnumDropdown(property, optionsData),
                SerializedPropertyType.ObjectReference => CreateObjectDropdown(property, optionsData),
                SerializedPropertyType.Generic => CreateGenericDropdown(property, optionsData),
                _ => CreateUnsupportedField(property)
            };

            fieldElement.style.flexGrow = 1;
            container.Add(fieldElement);

            return container;
        }

        private DropdownField CreateStringDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            // Set up choices
            var choices = optionsData.displayNames.ToList();
            dropdown.choices = choices;

            // Find current selection
            string currentValue = property.stringValue;
            int selectedIndex = Array.FindIndex(optionsData.options, opt => opt?.ToString() == currentValue);

            // Handle current value not in options
            if (selectedIndex < 0 && !string.IsNullOrEmpty(currentValue)) {
                choices.Insert(0, $"{currentValue} (current)");
                dropdown.choices = choices;
                selectedIndex = 0;
            } else if (selectedIndex < 0) {
                selectedIndex = 0;
            }

            // Set current value
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            // Handle value changes
            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    property.stringValue = optionsData.options[newIndex]?.ToString() ?? "";
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            return dropdown;
        }

        private DropdownField CreateIntDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            dropdown.choices = optionsData.displayNames.ToList();

            // Find current selection
            int currentValue = property.intValue;
            int selectedIndex = Array.FindIndex(optionsData.options, opt =>
                opt != null && int.TryParse(opt.ToString(), out int val) && val == currentValue);

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    if (int.TryParse(optionsData.options[newIndex]?.ToString(), out int newValue)) {
                        property.intValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            return dropdown;
        }

        private DropdownField CreateFloatDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            dropdown.choices = optionsData.displayNames.ToList();

            // Find current selection
            float currentValue = property.floatValue;
            int selectedIndex = Array.FindIndex(optionsData.options, opt =>
                opt != null && float.TryParse(opt.ToString(), out float val) && Mathf.Approximately(val, currentValue));

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    if (float.TryParse(optionsData.options[newIndex]?.ToString(), out float newValue)) {
                        property.floatValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            return dropdown;
        }

        private DropdownField CreateEnumDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            dropdown.choices = optionsData.displayNames.ToList();

            // Find current selection
            var enumType = fieldInfo.FieldType;
            var currentValue = Enum.ToObject(enumType, property.enumValueIndex);
            int selectedIndex = Array.FindIndex(optionsData.options, opt =>
                opt != null && opt.Equals(currentValue));

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    var newValue = optionsData.options[newIndex];
                    if (newValue != null && enumType.IsInstanceOfType(newValue)) {
                        property.enumValueIndex = Convert.ToInt32(newValue);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            return dropdown;
        }

        private DropdownField CreateObjectDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            dropdown.choices = optionsData.displayNames.ToList();

            // Find current selection
            var currentValue = property.objectReferenceValue;
            int selectedIndex = Array.FindIndex(optionsData.options, opt =>
                ReferenceEquals(opt, currentValue));

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    var newValue = optionsData.options[newIndex];
                    if (newValue is UnityEngine.Object unityObj) {
                        property.objectReferenceValue = unityObj;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            return dropdown;
        }

        private DropdownField CreateGenericDropdown(SerializedProperty property, OptionsData optionsData) {
            var dropdown = new DropdownField();

            dropdown.choices = optionsData.displayNames.ToList();

            // For generic properties, we need to handle them based on the field type
            var fieldType = fieldInfo.FieldType;

            // Find current selection by comparing serialized values
            int selectedIndex = FindCurrentGenericSelection(property, optionsData, fieldType);

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }

            dropdown.RegisterValueChangedCallback(evt => {
                int newIndex = dropdown.choices.IndexOf(evt.newValue);
                if (newIndex >= 0 && newIndex < optionsData.options.Length) {
                    var newValue = optionsData.options[newIndex];
                    SetGenericPropertyValue(property, newValue, fieldType);
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            return dropdown;
        }

        private Label CreateUnsupportedField(SerializedProperty property) {
            var errorLabel = new Label($"Options attribute not supported for {property.propertyType}");
            errorLabel.style.color = Color.red;
            return errorLabel;
        }

        private int FindCurrentGenericSelection(SerializedProperty property, OptionsData optionsData, Type fieldType) {
            try {
                // Get the current value from the serialized property
                var currentValue = GetCurrentGenericValue(property, fieldType);

                // Compare with each option
                for (int i = 0; i < optionsData.options.Length; i++) {
                    var option = optionsData.options[i];

                    // Handle null comparisons
                    if (currentValue == null && option == null) {
                        return i;
                    }
                    if (currentValue == null || option == null) {
                        continue;
                    }

                    // For value types and simple comparisons
                    if (fieldType.IsValueType || fieldType == typeof(string)) {
                        if (option.Equals(currentValue)) {
                            return i;
                        }
                    }
                    // For complex types, try multiple comparison strategies
                    else {
                        // First try direct equality
                        if (option.Equals(currentValue)) {
                            return i;
                        }

                        // Then try reference equality
                        if (ReferenceEquals(option, currentValue)) {
                            return i;
                        }

                        // For serializable objects, try JSON comparison
                        try {
                            var currentJson = JsonUtility.ToJson(currentValue);
                            var optionJson = JsonUtility.ToJson(option);
                            if (!string.IsNullOrEmpty(currentJson) && !string.IsNullOrEmpty(optionJson) && currentJson.Equals(optionJson)) {
                                return i;
                            }
                        } catch {
                            // JSON comparison failed, continue to next option
                        }
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning($"Error finding current generic selection: {ex.Message}");
            }

            return -1;
        }

        private object GetCurrentGenericValue(SerializedProperty property, Type fieldType) {
            // For generic properties, we need to reconstruct the object from the serialized data
            try {
                // Try to get the managed reference value if it's a managed reference
                if (property.propertyType == SerializedPropertyType.ManagedReference) {
                    return property.managedReferenceValue;
                }

                // Get the field value directly from the target object using reflection
                var targetObject = property.serializedObject.targetObject;
                var field = targetObject.GetType().GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) {
                    return field.GetValue(targetObject);
                }

                // Fallback: try to create a default instance only if the type has a parameterless constructor
                if (HasParameterlessConstructor(fieldType)) {
                    return Activator.CreateInstance(fieldType);
                }

                // If no parameterless constructor, return null
                return null;
            } catch (Exception ex) {
                Debug.LogWarning($"Error getting current generic value: {ex.Message}");
                return null;
            }
        }

        private bool HasParameterlessConstructor(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        private void SetGenericPropertyValue(SerializedProperty property, object value, Type fieldType) {
            try {
                // Handle managed reference types
                if (property.propertyType == SerializedPropertyType.ManagedReference) {
                    property.managedReferenceValue = value;
                    return;
                }

                // For other generic types, we need to set the value through reflection
                var targetObject = property.serializedObject.targetObject;
                var field = targetObject.GetType().GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null) {
                    // Convert value to the correct type if needed
                    object convertedValue = value;
                    if (value != null && !fieldType.IsInstanceOfType(value)) {
                        try {
                            convertedValue = Convert.ChangeType(value, fieldType);
                        } catch {
                            // If conversion fails, try to copy the value using JSON serialization
                            try {
                                var json = JsonUtility.ToJson(value);
                                convertedValue = JsonUtility.FromJson(json, fieldType);
                            } catch {
                                Debug.LogWarning($"Cannot convert value of type {value.GetType()} to {fieldType}");
                                return;
                            }
                        }
                    }

                    field.SetValue(targetObject, convertedValue);
                    EditorUtility.SetDirty(targetObject);
                }
            } catch (Exception ex) {
                Debug.LogError($"Error setting generic property value: {ex.Message}");
            }
        }

        private MethodInfo GetMethod(object target, string methodName) {
            if (target == null || string.IsNullOrEmpty(methodName))
                return null;

            Type type = target.GetType();
            MethodInfo method = null;

            while (type != null && method == null) {
                method = type.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    Type.EmptyTypes,
                    null);

                type = type.BaseType;
            }

            return method;
        }

        private OptionsData GetOptionsData(object target, MethodInfo method, SerializedProperty property) {
            try {
                object result = method.Invoke(target, null);
                if (result == null) return null;

                var options = ExtractOptionsArray(result);
                if (options == null || options.Length == 0) return null;

                var optionsAttribute = (OptionsAttribute)attribute;
                var displayNames = CreateDisplayNames(options, optionsAttribute.DisplayFormat);

                return new OptionsData { options = options, displayNames = displayNames };
            } catch (Exception ex) {
                Debug.LogError($"Error calling method '{method.Name}': {ex.Message}");
                return null;
            }
        }

        private object[] ExtractOptionsArray(object result) {
            if (result is Array array) {
                var objects = new object[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    objects[i] = array.GetValue(i);
                }
                return objects;
            } else if (result is IEnumerable enumerable) {
                var list = new List<object>();
                foreach (var item in enumerable) {
                    list.Add(item);
                }
                return list.ToArray();
            } else {
                return new[] { result };
            }
        }

        private string[] CreateDisplayNames(object[] options, string displayFormat) {
            var displayNames = new string[options.Length];

            for (int i = 0; i < options.Length; i++) {
                var option = options[i];
                if (option == null) {
                    displayNames[i] = "null";
                    continue;
                }

                if (!string.IsNullOrEmpty(displayFormat)) {
                    try {
                        // Simple format string replacement
                        displayNames[i] = displayFormat.Replace("{0}", option.ToString());

                        // Handle property access like {0.Name}
                        if (displayFormat.Contains("{0.")) {
                            displayNames[i] = FormatWithPropertyAccess(option, displayFormat);
                        }
                    } catch {
                        displayNames[i] = option.ToString();
                    }
                } else {
                    displayNames[i] = option.ToString();
                }
            }

            return displayNames;
        }

        private string FormatWithPropertyAccess(object obj, string format) {
            string result = format;

            // Handle multiple property access placeholders like {0.Name}, {0.intensity}, etc.
            var regex = new Regex(@"\{0\.([^}]+)\}");
            var matches = regex.Matches(format);

            foreach (Match match in matches) {
                var propertyPath = match.Groups[1].Value;
                var propertyValue = GetPropertyValue(obj, propertyPath);
                var placeholder = match.Value; // The full match like {0.name}
                result = result.Replace(placeholder, propertyValue?.ToString() ?? "null");
            }

            // Also handle the basic {0} placeholder
            result = result.Replace("{0}", obj.ToString());

            return result;
        }

        private object GetPropertyValue(object obj, string propertyName) {
            if (obj == null || string.IsNullOrEmpty(propertyName)) {
                return null;
            }

            try {
                var type = obj.GetType();

                // First try to get as a property
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && property.CanRead) {
                    return property.GetValue(obj);
                }

                // Then try to get as a field
                var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) {
                    return field.GetValue(obj);
                }

                Debug.LogWarning($"Property or field '{propertyName}' not found on type {type.Name}");
            } catch (Exception ex) {
                Debug.LogWarning($"Error accessing property '{propertyName}': {ex.Message}");
            }

            return null;
        }

        private class OptionsData {
            public object[] options;
            public string[] displayNames;
        }
    }
}
#endif
