using UnityEngine;

namespace Lotec.Utils.Extensions {
    public static class GameObjectExtensions {
        /// <summary>
        /// Get component in GameObject, or add it if it does not exist.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
            if (gameObject.TryGetComponent(out T component)) {
                return component;
            }
            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Check if GameObject has component of type T.
        /// </summary>
        /// /// <param name="gameObject">The GameObject to check</param>
        /// <typeparam name="T">The component type to check for</typeparam>
        /// <returns>True if the GameObject has the component, false otherwise</returns>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component {
            if (gameObject == null) return false;
            return gameObject.TryGetComponent<T>(out _);
        }

        /// <summary>
        /// Explicitly change GameObject active flag on all children.
        /// (SetActiveRecursively already exists in GameObject, but deprecated)
        /// TODO: Use GetChild() to skip foreach?
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="active"></param>
        public static void SetActiveRecursively(GameObject gameObject, bool active) {
            foreach (Transform transform in gameObject.transform) {
                transform.gameObject.SetActive(active);
                SetActiveRecursively(transform.gameObject, active);
            }
        }

        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
    }
}
