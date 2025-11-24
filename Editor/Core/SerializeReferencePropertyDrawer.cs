#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lotec.Utils.Interfaces.Editor {
    public static class TypeExtensions {
        // If type name ends with base type name, remove it.
        // Excludes prefixed 'I', commonly used in interface names.
        // Adds space before capital letters.
        public static string ToHumanizedString(this Type type, Type baseType) {
            string typeName = type.Name;
            string baseName = baseType.Name;

            if (baseType != null && baseType.IsAssignableFrom(type)) {
                // Remove 'I' from the interface name
                if (baseType.IsInterface)
                    baseName = Regex.Replace(baseName, @"^I(.*)", "$1");

                // Remove the base class name from end of the type name
                typeName = Regex.Replace(typeName, $"(.+)({baseName})$", "$1");
            }

            // Add spaces between words
            return Regex.Replace(typeName, @"(?<!^)([A-Z])", " $1");
        }
    }

    public static class TypeHelper {
        public static Dictionary<Type, Type[]> s_typesFromInterfaceType;
        public static readonly Type[] s_typesToCheckWithArrays;
        public static readonly Type[] s_typesToCheck;

        static TypeHelper() {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            s_typesToCheck = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttributes(typeof(SerializeInterfaceAttribute), false).Length > 0)
                .ToArray();

            s_typesToCheckWithArrays = s_typesToCheck
                .SelectMany(type => new Type[] { type, type.MakeArrayType() })
                .ToArray();

            s_typesFromInterfaceType = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(type => type.GetCustomAttributes(typeof(SerializeInterfaceAttribute), false).Any())
                .ToDictionary(
                    interfaceType => interfaceType,
                    interfaceType => assemblies
                        .SelectMany(s => s.GetTypes())
                        .Where(type => !type.IsInterface &&
                            !type.IsGenericType &&
                            type.GetInterfaces().Any(i => i.Namespace == interfaceType.Namespace && i.Name == interfaceType.Name) &&
                            !type.GetCustomAttributes(typeof(NoSerializeInterfaceAttribute), false).Any())
                        .OrderBy(type => type.Name)
                        .ToArray()
                );

            // foreach (var item in s_typesFromInterfaceType) {
            //     Debug.Log($"{item.Key.Name}: {item.Value.Length} - {string.Join(", ", item.Value.Select(t => t.Name))}");
            // }
        }
    }

    public class SerializeReferenceElement<T> : VisualElement {
        public SerializeReferenceElement(SerializedProperty property) {
            SerializedObject serializedObject = property.serializedObject;
            style.flexDirection = FlexDirection.Row;
            string typeName = property.managedReferenceValue?.GetType().ToHumanizedString(typeof(T)) ?? typeof(T).Name;
            var propField = new PropertyField(property, $"{property.displayName} ({typeName})");
            propField.style.flexGrow = 1;
            Add(propField);

            if (property.managedReferenceValue == null) {
                // "Create Type" dropdown menu
                var createMenu = new ToolbarMenu { text = "Create" };
                foreach (var typeInfo in TypeHelper.s_typesFromInterfaceType[typeof(T)]) {
                    string humanName = typeInfo.ToHumanizedString(typeof(T));
                    createMenu.menu.AppendAction(humanName, _ => {
                        property.managedReferenceValue = Activator.CreateInstance(typeInfo);
                        property.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                }
                Add(createMenu);
            } else {
                // "Clear" button to remove the managed reference
                var clearBtn = new Button(() => {
                    property.managedReferenceValue = null;
                    property.isExpanded = false;
                    serializedObject.ApplyModifiedProperties();
                    Clear();
                }) { text = "X" };
                Add(clearBtn);
            }
        }
    }

    [InitializeOnLoad]
    public static class AttributeChecker {
        static AttributeChecker() {
            CheckForSerializeFieldAttribute(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static void CheckForSerializeFieldAttribute(Assembly[] assemblies) {
            var membersToCheck = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.GetCustomAttributes(typeof(SerializeField), false).Any())
                    .Select(field => new TypeMap { Name = field.Name, Type = field.FieldType, DeclaringType = field.DeclaringType })
                    .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(property => property.GetCustomAttributes(typeof(SerializeField), false).Any())
                        .Select(property => new TypeMap { Name = property.Name, Type = property.PropertyType, DeclaringType = property.DeclaringType })
                    )
                )
                .Where(member => TypeHelper.s_typesToCheckWithArrays.Contains(member.Type));

            foreach (var member in membersToCheck) {
                string declaringTypeName = member.DeclaringType?.Name ?? "Unknown";
                Debug.LogWarning($"'{member.Name}' in class '{declaringTypeName}' uses [SerializeField] but is of type '{member.Type}' which is defined as [SerializeInterface]. You should probably use [SerializeReference] on the field.");
            }
        }
    }
    public struct TypeMap {
        public Type Type;
        public string Name;
        public string HumanizedName;
        public Type DeclaringType;
    }

    /// <summary>
    /// Generic property drawer for SerializeReference fields that supports both UI Toolkit and IMGUI.
    /// To create a property drawer for a specific interface, simply inherit from this class:
    /// [CustomPropertyDrawer(typeof(IYourInterface), true)]
    /// public class IYourInterfaceDrawer : SerializeReferenceDrawer<IYourInterface> { }
    /// </summary>
    /// <typeparam name="T">The interface type to draw</typeparam>
    public abstract class SerializeReferenceDrawer<T> : PropertyDrawer {
        // UI Toolkit implementation
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return new SerializeReferenceElement<T>(property);
        }

        // IMGUI implementation
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            DrawSerializeReferenceField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private void DrawSerializeReferenceField(Rect position, SerializedProperty property, GUIContent label) {
            var buttonWidth = 80f;
            var propertyRect = new Rect(position.x, position.y, position.width - buttonWidth - 5f, position.height);
            var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Draw the property field
            string typeName = property.managedReferenceValue?.GetType().ToHumanizedString(typeof(T)) ?? typeof(T).Name;
            var labelWithType = new GUIContent($"{label.text} ({typeName})", label.tooltip);

            EditorGUI.PropertyField(propertyRect, property, labelWithType, true);

            // Draw create/clear button
            if (property.managedReferenceValue == null) {
                if (GUI.Button(buttonRect, "Create")) {
                    ShowCreateMenu(property);
                }
            } else {
                if (GUI.Button(buttonRect, "Clear")) {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void ShowCreateMenu(SerializedProperty property) {
            var menu = new GenericMenu();

            if (TypeHelper.s_typesFromInterfaceType.ContainsKey(typeof(T))) {
                foreach (var typeInfo in TypeHelper.s_typesFromInterfaceType[typeof(T)]) {
                    string humanName = typeInfo.ToHumanizedString(typeof(T));
                    menu.AddItem(new GUIContent(humanName), false, () => {
                        property.managedReferenceValue = System.Activator.CreateInstance(typeInfo);
                        property.isExpanded = true;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            if (menu.GetItemCount() > 0) {
                menu.ShowAsContext();
            }
        }
    }
}
#endif
