
using System;
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Get object of type T in world, by shooting a ray.
    /// </summary>
    /// <typeparam name="T">Object type, for example IInteractable</typeparam>
    [Serializable]
    public class Aimer<T> where T : class {
        [SerializeField] protected float _interactionLength = 100f;
        [field: SerializeField, Tooltip("Defaults to Camera.main")]
        public Transform Anchor { get; set; }
        T _itemInWorld;
        T _previousItemInWorld;

        public T ItemInWorld => _itemInWorld;
        public T PreviousItemInWorld => _previousItemInWorld;

        /// <summary>
        /// Fire Ray cast from center of screen, and compare with previous result.
        /// Returns: Is the hit item different from the previous hit?
        /// </summary>
        public bool Check() {
            _previousItemInWorld = _itemInWorld;
            _itemInWorld = GetInteractable();

            return !Equals(_itemInWorld, _previousItemInWorld);
        }

        // Shoot a ray from anchor into world, and return the first matching object hit.
        T GetInteractable() {
            Ray ray = new(Anchor.position, Anchor.forward);
            Debug.DrawRay(ray.origin, ray.direction * _interactionLength, Color.yellow);
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionLength)) {
                return hit.collider.GetComponentInParent<T>();
            }
            return default;
        }
    }
}
