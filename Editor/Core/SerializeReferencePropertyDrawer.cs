#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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

    [InitializeOnLoad]
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

    // T == interfaceType
    public abstract class SerializeReferenceDrawer<T> : PropertyDrawer {
        readonly TypeMap[] _typeInfos;

        public SerializeReferenceDrawer() {
            _typeInfos = TypeHelper.s_typesFromInterfaceType[typeof(T)]
                .Select(t => new TypeMap { Type = t, Name = t.Name, HumanizedName = t.ToHumanizedString(typeof(T)) })
                .ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            bool showCreateButton = false;
            try {
                showCreateButton = property.managedReferenceValue == null;
                if (!showCreateButton) {
                    label.text = property.managedReferenceValue.GetType().ToHumanizedString(typeof(T));
                }
            } catch (InvalidOperationException) {
                Debug.LogError("InvalidOperationException");
                // property.managedReferenceValue only exists on SerializeReference property.
            }

            if (!showCreateButton) {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Create a dropdown button
            if (EditorGUI.DropdownButton(position, new GUIContent("Create Type"), FocusType.Passive)) {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < _typeInfos.Length; i++) {
                    TypeMap typeInfo = _typeInfos[i];
                    menu.AddItem(new GUIContent(typeInfo.HumanizedName), false, () => {
                        Type currentType = property.managedReferenceValue?.GetType();
                        if (currentType == typeInfo.Type) return;

                        // Create a new instance of the selected type
                        object instance = Activator.CreateInstance(typeInfo.Type);
                        property.managedReferenceValue = instance;
                        property.isExpanded = true;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, true);
    }

    [InitializeOnLoad]
    public static class AttributeChecker {
        static AttributeChecker() {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            CheckForSerializeFieldAttribute(assemblies);
        }

        public static void CheckForSerializeFieldAttribute(Assembly[] assemblies) {
            var membersToCheck = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.GetCustomAttributes(typeof(SerializeField), false).Any())
                    .Select(field => new TypeMap { Name = field.Name, Type = field.FieldType })
                    .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(property => property.GetCustomAttributes(typeof(SerializeField), false).Any())
                        .Select(property => new TypeMap { Name = property.Name, Type = property.PropertyType })
                    )
                )
                .Where(member => TypeHelper.s_typesToCheckWithArrays.Contains(member.Type));

            foreach (var member in membersToCheck) {
                Debug.LogWarning($"'{member.Name}' in class '{member.Type.DeclaringType.Name}' is of type '{member.Type}' and uses [SerializeField].");
            }
        }
    }
    public struct TypeMap {
        public Type Type;
        public string Name;
        public string HumanizedName;
    }
}
#endif
