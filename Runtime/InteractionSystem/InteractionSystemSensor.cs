
using System;
using Lotec.Utils;
using UnityEngine;

namespace Lotec.Interactions {
    public interface IObjectSensor<T> where T : class {
        event Action<T> SensorUpdated;
        T SensorObject { get; }
    }

    /// <summary>
    /// Find Interactable in world, by shooting a ray from player head camera.
    /// </summary>
    [Serializable]
    public class InteractionSystemSensor : MonoBehaviour2, IObjectSensor<Interactable> {
        [SerializeField] Aimer<Interactable> _aimer = new();

        public event Action<Interactable> SensorUpdated;
        public Interactable SensorObject => _aimer.ItemInWorld;

        void Start() {
            if (_aimer.Anchor == null) {
                _aimer.Anchor = Camera.main.transform;
            }
        }

        void Update() {
            if (_aimer.Check()) {
                SensorUpdated?.Invoke(_aimer.ItemInWorld);
            }
        }
    }
}
