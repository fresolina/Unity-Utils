using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Utils {
    public static class UniqueIdManager {
        private static readonly Dictionary<string, GameObject> s_gameObjects = new Dictionary<string, GameObject>();
        private static bool s_initialized = false;

        private static void EnsureInitialized() {
            if (s_initialized) return;
            s_initialized = true;

            RegisterSceneObjects();
        }

        private static void RegisterSceneObjects() {
            // Find all UniqueId components in loaded scenes (includes inactive objects)
            var uniqueIds = Object.FindObjectsByType<UniqueId>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var uniqueId in uniqueIds) {
                if (!string.IsNullOrEmpty(uniqueId.Id) && uniqueId.gameObject != null) {
                    RegisterObject(uniqueId.Id, uniqueId.gameObject);
                }
            }
        }

        // Register a GameObject for an id (call when creating/spawning objects at runtime)
        public static void RegisterObject(string id, GameObject obj) {
            EnsureInitialized();
            Debug.Log($"UniqueIdManager: Registering GameObject '{obj.name}' for id '{id}'");
            s_gameObjects[id] = obj;
        }

        public static GameObject GetGameObject(string id) {
            EnsureInitialized();
            Debug.Log($"UniqueIdManager: Getting GameObject for id '{id}'");
            s_gameObjects.TryGetValue(id, out var go);
            return go;
        }

        public static bool TryGetComponent<T>(string id, out T result) where T : Component {
            EnsureInitialized();
            if (s_gameObjects.TryGetValue(id, out var go) && go != null) {
                return go.TryGetComponent(out result);
            }

            result = null;
            return false;
        }

        // Force a full re-scan of loaded scenes for UniqueId components and rebuild the index.
        // Call this after dynamically loading scenes or when you need to ensure the index is up-to-date.
        public static void RebuildIndex() {
            s_gameObjects.Clear();
            // Re-scan loaded scenes
            var uniqueIds = Object.FindObjectsByType<UniqueId>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var uniqueId in uniqueIds) {
                if (!string.IsNullOrEmpty(uniqueId.Id) && uniqueId.gameObject != null) {
                    s_gameObjects[uniqueId.Id] = uniqueId.gameObject;
                }
            }
            s_initialized = true;
            Debug.Log($"UniqueIdManager: Rebuilt index with {s_gameObjects.Count} entries.");
        }
    }
}
