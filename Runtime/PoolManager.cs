using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Lotec.Utils.Pools {
    /// <summary>
    /// Simple wrapper for pooling GameObjects. Automatically creates a pool per prefab.
    /// Add this script to a manager, then replace calls to Instantiate()
    ///   with PoolManager.Instantiate() and Destroy() with Poolmanager.Destroy().
    /// Do not use for prefabs that will break up into smaller pieces, etc.
    /// Ensure your pooled objects OnEnable() method resets needed values.
    /// </summary>
    public class PoolManager : MonoBehaviour {
        private static PoolManager s_instance;
        // Get the pool, handling objects instantiated from a prefab
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsFromPrefab = new();
        // Get the pool, handling the instantiated object
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsFromObject = new();

        public static GameObject Instantiate(GameObject prefab) {
            return s_instance.Get(prefab);
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
            GameObject obj = s_instance.Get(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public static void Destroy(GameObject obj) {
            s_instance.Release(obj);
        }

        public static void Destroy(GameObject obj, float time) {
            s_instance.StartCoroutine(s_instance.Release(obj, time));
        }

        void Awake() {
            s_instance = this;
        }

        public GameObject Get(GameObject prefab) {
            GameObject obj;
            if (!_poolsFromPrefab.TryGetValue(prefab, out ObjectPool<GameObject> pool)) {
                // This prefab requires a new pool for these object types.
                pool = new ObjectPool<GameObject>(
                    createFunc: () => Object.Instantiate(prefab),
                    actionOnGet: (GameObject obj) => obj.SetActive(true),
                    actionOnRelease: (GameObject obj) => obj.SetActive(false),
                    actionOnDestroy: (GameObject obj) => Object.Destroy(obj)
                );
                _poolsFromPrefab.Add(prefab, pool);
            }
            obj = pool.Get();
            _poolsFromObject.TryAdd(obj, pool);

            return obj;
        }

        public IEnumerator Release(GameObject obj, float time) {
            yield return new WaitForSeconds(time);
            Release(obj);
        }

        public void Release(GameObject obj) {
            if (_poolsFromObject.TryGetValue(obj, out ObjectPool<GameObject> pool)) {
                pool.Release(obj);
            } else {
                Debug.LogError($"Tried to release unknown object '{obj.name}' into pool. Destroying instead", obj);
                Object.Destroy(obj);
            }
        }
    }
}
