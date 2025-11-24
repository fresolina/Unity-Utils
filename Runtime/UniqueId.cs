using UnityEngine;

namespace Lotec.Utils {
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class UniqueId : MonoBehaviour {
        [SerializeField] string _id;
        public string Id => _id;

        void Awake() {
            EnsureRegistered();
        }

        void EnsureRegistered() {
            if (string.IsNullOrEmpty(_id)) {
                _id = System.Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
                Debug.Log($"Assigned new UniqueId {_id} to GameObject '{gameObject.name}' in scene '{gameObject.scene.name}'");
            }
            UniqueIdManager.RegisterObject(_id, gameObject);
        }

#if UNITY_EDITOR
        void Reset() {
            EnsureRegistered();
        }
#endif
    }
}
