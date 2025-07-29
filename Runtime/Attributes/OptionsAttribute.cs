using System;
using UnityEngine;

namespace Lotec.Utils.Attributes {
    /// <summary>
    /// Generic attribute that creates a dropdown in the inspector with options populated by calling a method at edit time.
    /// Works with any serializable type that can be displayed in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OptionsAttribute : PropertyAttribute {
        /// <summary>
        /// The name of the method to call to get the list of available options.
        /// The method should return T[], List&lt;T&gt;, or IEnumerable&lt;T&gt; and take no parameters,
        /// where T matches the field type.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Optional display format for complex types. Use {0} as placeholder for the object.
        /// For example: "{0.Name}" or "{0.ToString()}"
        /// </summary>
        public string DisplayFormat { get; set; }

        /// <summary>
        /// Creates an Options attribute.
        /// </summary>
        /// <param name="methodName">Name of the method that returns the available options</param>
        public OptionsAttribute(string methodName) {
            MethodName = methodName;
        }

        /// <summary>
        /// Creates an Options attribute with custom display format.
        /// </summary>
        /// <param name="methodName">Name of the method that returns the available options</param>
        /// <param name="displayFormat">Format string for displaying complex types</param>
        public OptionsAttribute(string methodName, string displayFormat) {
            MethodName = methodName;
            DisplayFormat = displayFormat;
        }
    }
}
