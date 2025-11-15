using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lotec.Lighting {
    /// <summary>
    /// Manages lighting scenarios within the scene.
    /// </summary>
    public class LightmapScenarioHandler : MonoBehaviour, ILightingScenarioHandler {
        [SerializeField] LightmapScenario[] _scenarios = new LightmapScenario[0];
        [SerializeField] LightingScenario _activeScenario;

        public LightmapScenario[] Scenarios => _scenarios;
        public LightingScenario ActiveScenario => _activeScenario;

        public void ActivateScenario(LightingScenario scenario) {
            if (scenario == null) return;

            if (_activeScenario != null)
                _activeScenario.DisableScenario();
            _activeScenario = scenario;
            _activeScenario.ApplyScenario();
        }
    }
}
