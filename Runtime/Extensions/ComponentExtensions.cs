using UnityEngine;

namespace Lotec.Utils.Extensions {
    public static class ComponentExtensions {
        /// <summary>
        /// Clone this component and add it to destination GameObject.
        /// </summary>
        /// <param name="destination"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The cloned component</returns>
        public static T CopyComponent<T>(this T original, GameObject destination) where T : Component {
            System.Type type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields) {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props) {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst;
        }
    }
}
