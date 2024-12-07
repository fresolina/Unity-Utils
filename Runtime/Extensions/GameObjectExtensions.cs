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
