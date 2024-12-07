using UnityEngine;

namespace Lotec.Utils.Attributes {
    /// <summary>
    /// Custom attribute to validate if a field is not null.
    /// Marks the field in red if it is null in the inspector.
    /// </summary>
    public class NotNullAttribute : PropertyAttribute { }
}
