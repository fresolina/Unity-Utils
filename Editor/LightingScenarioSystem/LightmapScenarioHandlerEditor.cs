#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lotec.Lighting.Editor {
    [CustomEditor(typeof(LightmapScenarioHandler))]
    public class LightmapScenarioHandlerEditor : UnityEditor.Editor {
        static System.Collections.Generic.Dictionary<LightmapScenarioHandler, LightmapScenarioHandlerBaker> s_bakers = new();

        LightmapScenarioHandlerBaker BakerFor(LightmapScenarioHandler handler) {
            if (handler == null) return null;
            if (!s_bakers.TryGetValue(handler, out var baker) || baker == null) {
                baker = new LightmapScenarioHandlerBaker(handler);
                s_bakers[handler] = baker;
            }
            return baker;
        }

        public override void OnInspectorGUI() {
            // Draw default inspector
            DrawDefaultInspector();

            // Then add simple buttons for each scenario (merged from LightmapScenarioHandlerButtons)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenarios", EditorStyles.boldLabel);

            var handler = target as LightmapScenarioHandler;
            if (handler == null) return;

            serializedObject.Update();
            var scenariosProp = serializedObject.FindProperty("_scenarios");
            if (scenariosProp == null || scenariosProp.arraySize == 0) {
                EditorGUILayout.LabelField("(no scenarios)");
                return;
            }

            for (int i = 0; i < scenariosProp.arraySize; i++) {
                var el = scenariosProp.GetArrayElementAtIndex(i);
                var sc = el.objectReferenceValue as Lotec.Lighting.LightmapScenario;
                if (sc == null) continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(sc.name);
                if (GUILayout.Button("Activate", GUILayout.Width(90))) {
                    handler.ActivateScenario(sc);
                    if (!Application.isPlaying) EditorUtility.SetDirty(handler);
                }
                if (GUILayout.Button("Bake", GUILayout.Width(90))) {
                    LightmapScenarioEditorUtility.BakeAsync(sc, () => {
                        if (!Application.isPlaying) EditorUtility.SetDirty(handler);
                    });
                }
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();

            // Add spacing and bottom-aligned Bake All button (full-width)
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var baker = BakerFor(handler);
            if (baker.IsRunning) {
                // show inline progress, percentage label, and cancel button
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(baker.LastMessage);
                Rect r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                EditorGUI.ProgressBar(r, baker.LastProgress, string.Empty);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
                    baker.Cancel();
                }
            } else {
                if (GUILayout.Button("Bake All Scenarios", GUILayout.Height(28), GUILayout.ExpandWidth(true))) {
                    baker?.Start();
                }
            }
            EditorGUILayout.EndHorizontal();
            // Show last bake summary (if present)
            if (!string.IsNullOrEmpty(baker.LastBakeSummary)) {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(baker.LastBakeSummary, MessageType.Info);
            }
        }

        void OnDisable() {
            if (target is LightmapScenarioHandler handler) {
                s_bakers.Remove(handler);
            }
        }
    }
}
#endif
