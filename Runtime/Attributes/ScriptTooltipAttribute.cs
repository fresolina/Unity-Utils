using System;

namespace Lotec.Utils {
    /// <summary>
    /// Custom attribute to specify tooltip text for MonoBehaviour classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ScriptTooltipAttribute : Attribute {
        public string Tooltip { get; }

        public ScriptTooltipAttribute(string tooltip) {
            Tooltip = tooltip;
        }
    }
}
