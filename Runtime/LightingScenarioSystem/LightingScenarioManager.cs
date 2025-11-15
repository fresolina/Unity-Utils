using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Lighting {

    interface ILightingScenarioHandler {
        void ActivateScenario(LightingScenario scenario);
    }

    /// <summary>
    /// Manages lighting scenarios within the scene.
    /// </summary>
    public class LightingScenarioManager : MonoBehaviour2 {
        public static LightingScenarioManager Instance { get; private set; }

        [SerializeField, NotNull] ILightingScenarioHandler _handler;

        void Awake() {
            if (Instance != null) {
                Debug.LogError("Duplicate Singleton: LightingScenarioManager already exists.", Instance.gameObject);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ActivateScenario(LightingScenario scenario) => _handler.ActivateScenario(scenario);
    }
}
