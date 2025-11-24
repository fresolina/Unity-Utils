using System.Collections.Generic;
using System.Linq;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Utils.Examples {
    /// <summary>
    /// Example demonstrating the generic Options attribute with various types
    /// </summary>
    public class GenericOptionsExample : MonoBehaviour2 {
        [System.Serializable]
        public class LightingScenario {
            public string name;
            public Color ambientColor;
            public float intensity;

            public LightingScenario(string name, Color color, float intensity) {
                this.name = name;
                this.ambientColor = color;
                this.intensity = intensity;
            }

            public override string ToString() => name;
        }

        [Header("Basic types")]
        [SerializeField, Options(nameof(GetStringOptions))]
        private string selectedString = "";
        [SerializeField, Options(nameof(GetIntOptions))]
        private int selectedInt = 0;
        [SerializeField, Options(nameof(GetFloatOptions))]
        private float selectedFloat = 0f;

        [Header("GameObject Options")]
        [SerializeField, Options(nameof(GetGameObjectOptions))]
        private GameObject selectedGameObject;

        [Header("Custom Object Options with Display Format")]
        [SerializeField, Options(nameof(GetLightingScenarios), "{0.name} (Intensity: {0.intensity})")]
        private LightingScenario selectedScenario;

        [Header("Component Options")]
        [SerializeField, Options(nameof(GetAudioSourceOptions))]
        private AudioSource selectedAudioSource;

        [Header("Material Options")]
        [SerializeField, Options(nameof(GetMaterialOptions))]
        private Material selectedMaterial;

        // Configuration for dynamic options
        [SerializeField] private bool includeAdvancedOptions = false;
        [SerializeField] private int maxIntValue = 10;

        // String options method
        private string[] GetStringOptions() {
            var options = new List<string> { "Morning", "Afternoon", "Evening", "Night" };

            if (includeAdvancedOptions) {
                options.AddRange(new[] { "Night_Fireplace", "Debug_Mode", "Custom_Lighting" });
            }

            return options.ToArray();
        }

        // Integer options method
        private int[] GetIntOptions() {
            var options = new List<int>();

            for (int i = 1; i <= maxIntValue; i++) {
                if (i % 2 == 1 || includeAdvancedOptions) // Odd numbers, or all if advanced
                {
                    options.Add(i);
                }
            }

            return options.ToArray();
        }

        // Float options method
        private float[] GetFloatOptions() {
            return new float[] { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
        }

        // GameObject options method
        private GameObject[] GetGameObjectOptions() {
            // Find all GameObjects with specific components
            var options = new List<GameObject> { null }; // Include null option

            // Add all GameObjects with Light components in the scene
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            options.AddRange(lights.Select(light => light.gameObject));

            return options.ToArray();
        }

        // Custom object options method
        private LightingScenario[] GetLightingScenarios() {
            var scenarios = new List<LightingScenario>
            {
                new LightingScenario("Morning", new Color(1f, 0.95f, 0.8f), 1.2f),
                new LightingScenario("Afternoon", new Color(1f, 1f, 1f), 1.0f),
                new LightingScenario("Evening", new Color(1f, 0.8f, 0.6f), 0.7f),
                new LightingScenario("Night", new Color(0.2f, 0.2f, 0.4f), 0.3f)
            };

            if (includeAdvancedOptions) {
                scenarios.Add(new LightingScenario("Night_Fireplace", new Color(1f, 0.5f, 0.2f), 0.8f));
                scenarios.Add(new LightingScenario("Storm", new Color(0.3f, 0.3f, 0.3f), 0.4f));
            }

            return scenarios.ToArray();
        }

        // AudioSource options method
        private AudioSource[] GetAudioSourceOptions() {
            var options = new List<AudioSource> { null }; // Include null option

            // Find all AudioSources in the scene
            AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            options.AddRange(audioSources);

            return options.ToArray();
        }

        // Material options method  
        private Material[] GetMaterialOptions() {
            var options = new List<Material> { null }; // Include null option

            // Find all unique materials from renderers in the scene
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var materials = new HashSet<Material>();

            foreach (var renderer in renderers) {
                foreach (var material in renderer.sharedMaterials) {
                    if (material != null) {
                        materials.Add(material);
                    }
                }
            }

            options.AddRange(materials);
            return options.ToArray();
        }

        [ContextMenu("Apply Current Selection")]
        public void ApplyCurrentSelection() {
            Debug.Log("=== Current Options Selection ===");
            Debug.Log($"String: {selectedString}");
            Debug.Log($"Int: {selectedInt}");
            Debug.Log($"Float: {selectedFloat}");
            Debug.Log($"GameObject: {(selectedGameObject ? selectedGameObject.name : "null")}");
            Debug.Log($"Scenario: {(selectedScenario != null ? $"{selectedScenario.name} ({selectedScenario.intensity})" : "null")}");
            Debug.Log($"AudioSource: {(selectedAudioSource ? selectedAudioSource.name : "null")}");
            Debug.Log($"Material: {(selectedMaterial ? selectedMaterial.name : "null")}");
            Debug.Log("=== End Selection ===");

            // Apply the lighting scenario if one is selected
            if (selectedScenario != null) {
                RenderSettings.ambientLight = selectedScenario.ambientColor;
                RenderSettings.ambientIntensity = selectedScenario.intensity;
                Debug.Log($"Applied lighting scenario: {selectedScenario.name}");
            }
        }

        [ContextMenu("Test Dynamic Updates")]
        public void TestDynamicUpdates() {
            includeAdvancedOptions = !includeAdvancedOptions;
            maxIntValue = maxIntValue == 10 ? 20 : 10;

            Debug.Log($"Updated options: Advanced={includeAdvancedOptions}, MaxInt={maxIntValue}");
            Debug.Log("Check inspector to see updated dropdown options!");
        }

        void Start() {
            ApplyCurrentSelection();
        }

        protected override void OnValidate() {
            base.OnValidate();

            // Example validation: ensure selected values are still valid
            var stringOptions = GetStringOptions();
            if (!stringOptions.Contains(selectedString) && stringOptions.Length > 0) {
                selectedString = stringOptions[0];
            }

            var intOptions = GetIntOptions();
            if (!intOptions.Contains(selectedInt) && intOptions.Length > 0) {
                selectedInt = intOptions[0];
            }
        }
    }
}
