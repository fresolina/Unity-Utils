#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotec.Utils.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lotec.Utils.Editor {
    [CustomPropertyDrawer(typeof(OptionsAttribute))]
    public class OptionsPropertyDrawer : PropertyDrawer {

        private static readonly HashSet<SerializedPropertyType> s_supportedTypes = new() {
            SerializedPropertyType.String,
            SerializedPropertyType.Integer,
            SerializedPropertyType.Float
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var optionsAttribute = (OptionsAttribute)attribute;
            var targetObject = property.serializedObject.targetObject;
            var propertyField = new PropertyField(property);

            // Validate property type
            if (!s_supportedTypes.Contains(property.propertyType)) {
                return CreateErrorField(property, "Options attribute only supports string, int, and float properties");
            }

            // Get options method and validate
            var method = GetMethod(targetObject, optionsAttribute.MethodName);
            if (method == null) {
                return CreateErrorField(property, $"Method '{optionsAttribute.MethodName}' not found");
            }

            var options = GetOptions(targetObject, method);
            if (options == null || options.Length == 0) {
                return CreateErrorField(property, "No options available from method");
            }

            // Create dropdown and schedule replacement
            var dropdown = CreateDropdown(property, options);
            propertyField.schedule.Execute(() => ReplaceInputField(propertyField, dropdown));

            return propertyField;
        }

        private VisualElement CreateErrorField(SerializedProperty property, string errorMessage) {
            // Create a container that mimics the PropertyField layout
            var container = new VisualElement();
            container.AddToClassList("unity-base-field");
            container.AddToClassList("unity-property-field");

            // Add the property label using the display name from SerializedProperty
            var label = new Label(property.displayName);
            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");
            container.Add(label);

            // Add the error message as the input area
            var errorLabel = new Label(errorMessage);
            errorLabel.AddToClassList("unity-base-field__input");
            errorLabel.style.color = Color.red;
            container.Add(errorLabel);

            return container;
        }

        private DropdownField CreateDropdown(SerializedProperty property, object[] options) {
            var dropdown = new DropdownField();
            var stringOptions = options.Select(opt => opt?.ToString() ?? "null").ToArray();

            // Get current value and find selection index
            var (currentValue, selectedIndex) = property.propertyType switch {
                SerializedPropertyType.String => GetStringSelection(property.stringValue, options, stringOptions),
                SerializedPropertyType.Integer => GetIntSelection(property.intValue, options, stringOptions),
                SerializedPropertyType.Float => GetFloatSelection(property.floatValue, options, stringOptions),
                _ => throw new InvalidOperationException($"Unsupported property type: {property.propertyType}")
            };

            // Setup dropdown choices and selection
            SetupDropdownChoices(dropdown, stringOptions, currentValue, selectedIndex);

            // Register value change callback
            dropdown.RegisterValueChangedCallback(evt => HandleValueChange(property, options, evt.newValue, currentValue));

            return dropdown;
        }

        private (string currentValue, int selectedIndex) GetStringSelection(string value, object[] options, string[] stringOptions) {
            var selectedIndex = Array.FindIndex(stringOptions, opt => opt == value);
            return (value, selectedIndex);
        }

        private (string currentValue, int selectedIndex) GetIntSelection(int value, object[] options, string[] stringOptions) {
            var selectedIndex = Array.FindIndex(options, opt =>
                opt != null && int.TryParse(opt.ToString(), out int val) && val == value);
            return (value.ToString(), selectedIndex);
        }

        private (string currentValue, int selectedIndex) GetFloatSelection(float value, object[] options, string[] stringOptions) {
            var selectedIndex = Array.FindIndex(options, opt =>
                opt != null && float.TryParse(opt.ToString(), out float val) && Mathf.Approximately(val, value));
            return (value.ToString(), selectedIndex);
        }

        private void SetupDropdownChoices(DropdownField dropdown, string[] stringOptions, string currentValue, int selectedIndex) {
            var choices = stringOptions.ToList();

            // Handle current value not in options
            if (selectedIndex < 0 && !string.IsNullOrEmpty(currentValue)) {
                choices.Insert(0, $"{currentValue} (current)");
                selectedIndex = 0;
            } else if (selectedIndex < 0) {
                selectedIndex = 0;
            }

            dropdown.choices = choices;

            // Set selected index safely
            if (selectedIndex < dropdown.choices.Count) {
                dropdown.index = selectedIndex;
            }
        }

        private void HandleValueChange(SerializedProperty property, object[] options, string newValue, string currentValue) {
            // Handle "(current)" option
            if (newValue.EndsWith(" (current)")) {
                SetPropertyValue(property, currentValue);
                return;
            }

            // Find the option index
            var newIndex = Array.FindIndex(options, opt => opt?.ToString() == newValue);
            if (newIndex >= 0) {
                SetPropertyValue(property, options[newIndex].ToString());
            }
        }

        private void SetPropertyValue(SerializedProperty property, string value) {
            switch (property.propertyType) {
                case SerializedPropertyType.String:
                    property.stringValue = value;
                    break;
                case SerializedPropertyType.Integer:
                    if (int.TryParse(value, out int intValue)) {
                        property.intValue = intValue;
                    }
                    break;
                case SerializedPropertyType.Float:
                    if (float.TryParse(value, out float floatValue)) {
                        property.floatValue = floatValue;
                    }
                    break;
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        private void ReplaceInputField(PropertyField propertyField, DropdownField dropdown) {
            // Find the input field within the PropertyField and replace it with our dropdown
            var inputField = propertyField.Q(className: "unity-base-field__input");
            if (inputField != null) {
                inputField.Clear();
                inputField.Add(dropdown);
                return;
            }

            // Fallback: find text field input and replace it
            var textInput = propertyField.Q(className: "unity-text-field__input");
            if (textInput != null) {
                var parent = textInput.parent;
                if (parent != null) {
                    parent.Clear();
                    parent.Add(dropdown);
                    return;
                }
            }

            // Last resort: add the dropdown directly to the PropertyField
            propertyField.Add(dropdown);
        }

        private object[] GetOptions(object target, MethodInfo method) {
            try {
                var result = method.Invoke(target, null);
                if (result is Array array) {
                    var options = new object[array.Length];
                    for (int i = 0; i < array.Length; i++) {
                        options[i] = array.GetValue(i);
                    }
                    return options;
                }
                return null;
            } catch (Exception ex) {
                Debug.LogError($"Error calling method '{method.Name}': {ex.Message}");
                return null;
            }
        }

        private MethodInfo GetMethod(object target, string methodName) {
            if (target == null || string.IsNullOrEmpty(methodName))
                return null;

            var type = target.GetType();
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
    }
}
#endif
