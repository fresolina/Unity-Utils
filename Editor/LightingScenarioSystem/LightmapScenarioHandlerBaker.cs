#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lotec.Lighting.Editor {
    /// <summary>
    /// TODO: Auto-bake skybox reflection probe and move into scenario folder.
    /// Can be manually set by copying the Unity-generated ReflectionProbe-0 into the scenario folder,
    /// then setting it as custom reflection probe for that scenario and mode from Skybox to Custom.
    /// </summary>
    public class LightmapScenarioHandlerBaker {
        readonly LightmapScenarioHandler _handler;
        LightmapScenario[] _scenarios;
        LightmapScenario _currentScenario;
        int _currentScenarioIndex = -1;
        int _bakedCount = 0;
        DateTime _sequenceStartUtc = DateTime.MinValue;
        float _totalProgress;
        float _lastScenarioRawProgress = 0f;

        public bool IsRunning { get; private set; } = false;
        // LastProgress represents total progress across all scenarios [0..1]
        public float LastProgress { get; private set; }
        // Current message (e.g. "Baking scenario 2/5")
        public string LastMessage { get; private set; }
        // Summary of the last completed bake sequence (shown in inspector)
        public string LastBakeSummary { get; private set; }

        public LightmapScenarioHandlerBaker(LightmapScenarioHandler handler) {
            _handler = handler;
        }

        public void Start() {
            if (IsRunning) {
                // already running; don't show a modal dialog in editor code per new requirements
                Debug.LogWarning("LightmapScenarioHandlerBaker: Start called while already running.");
                return;
            }

            var rawScenarios = _handler?.Scenarios ?? Array.Empty<LightmapScenario>();
            if (rawScenarios.Length == 0) {
                LastBakeSummary = "No scenarios available";
                Debug.LogWarning("LightmapScenarioHandlerBaker: No lightmap scenarios found on this handler.");
                return;
            }

            var validScenarios = new List<LightmapScenario>(rawScenarios.Length);
            foreach (var scenario in rawScenarios) {
                if (scenario == null) continue;
                validScenarios.Add(scenario);
            }

            if (validScenarios.Count == 0) {
                LastBakeSummary = "No valid scenarios available";
                Debug.LogWarning("LightmapScenarioHandlerBaker: All scenario references were null on this handler. Refresh the list in the inspector.");
                return;
            }

            _scenarios = validScenarios.ToArray();

            _currentScenarioIndex = -1;
            _bakedCount = 0;
            IsRunning = true;
            _sequenceStartUtc = DateTime.UtcNow;

            // Ensure all scenarios are disabled before starting
            for (int i = 0; i < _scenarios.Length; i++) {
                var scenario = _scenarios[i];
                if (scenario == null) continue;
                scenario.DisableScenario();
            }

            LastBakeSummary = string.Empty;
            LastProgress = 0f;
            LastMessage = "Starting...";
            EditorApplication.update += OnUpdate;
            StartNext();
        }

        void StartNext() {
            _currentScenarioIndex++;
            if (!IsRunning || _scenarios == null || _currentScenarioIndex >= _scenarios.Length) {
                Finish();
                return;
            }

            _currentScenario = _scenarios[_currentScenarioIndex];

            _lastScenarioRawProgress = 0f;
            LightmapScenarioEditorUtility.BakeAsync(_currentScenario, () => {
                _bakedCount++;
                StartNext();
            });
        }

        void OnUpdate() {
            if (!IsRunning) return;

            float scenarioProgress = Mathf.Clamp01(Lightmapping.buildProgress);
            // Lightmapping.buildProgress can start over, caused by Lightmapping internal multipass baking. Ignore that.
            scenarioProgress = Mathf.Max(scenarioProgress, _lastScenarioRawProgress);
            int totalScenarios = Mathf.Max(1, _scenarios.Length);
            _totalProgress = (_currentScenarioIndex + scenarioProgress) / totalScenarios;
            LastMessage = $"Baking scenario {_currentScenarioIndex + 1}/{_scenarios.Length}";
            LastProgress = Mathf.Clamp01(_totalProgress);
            // Lightmapping.buildProgress can start over, caused by Lightmapping internal multipass baking. Ignore that.
            _lastScenarioRawProgress = Mathf.Max(_lastScenarioRawProgress, scenarioProgress);
        }

        void Finish() {
            EditorApplication.update -= OnUpdate;
            IsRunning = false;
            Debug.Log($"LightmapScenarioHandlerBaker: Finish (baked {_bakedCount} scenarios)");
            var duration = DateTime.UtcNow - (_sequenceStartUtc == DateTime.MinValue ? DateTime.UtcNow : _sequenceStartUtc);
            string dur = string.Format("{0:D2}:{1:D2}", (int)duration.TotalMinutes, duration.Seconds);
            LastBakeSummary = $"Last bake: baked {_bakedCount} of {_scenarios.Length} scenarios in {dur}.";

            LastProgress = 1f;
            LastMessage = "Completed";
            _sequenceStartUtc = DateTime.MinValue;
        }

        public void Cancel() {
            EditorApplication.update -= OnUpdate;
            IsRunning = false;
            LightmapScenarioEditorUtility.Cleanup(_currentScenario);
            Debug.Log("LightmapScenarioHandlerBaker: Cancelled");
            var duration = DateTime.UtcNow - (_sequenceStartUtc == DateTime.MinValue ? DateTime.UtcNow : _sequenceStartUtc);
            string dur = string.Format("{0:D2}:{1:D2}", (int)duration.TotalMinutes, duration.Seconds);
            LastBakeSummary = $"Last bake: cancelled after {dur} (baked {_bakedCount} of {_scenarios?.Length ?? 0}).";
            _sequenceStartUtc = DateTime.MinValue;
        }
    }
}
#endif
