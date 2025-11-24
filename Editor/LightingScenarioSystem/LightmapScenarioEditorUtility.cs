#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Lotec.Utils.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Lotec.Lighting.Editor {
    [InitializeOnLoad]
    public static class LightmapScenarioEditorUtility {
        class ScenarioBakeState {
            public bool IsBaking;
            public Action OnComplete;
            public Action BakeCompletedHandler;
        }

        static readonly Dictionary<LightmapScenario, ScenarioBakeState> s_bakeStates = new();

        static LightmapScenarioEditorUtility() {
            LightmapScenario.ScenarioAppliedInEditor += AssignLightingDataAsset;
            LightmapScenario.ScenarioDisabledInEditor += Cleanup;
        }

        static ScenarioBakeState GetState(LightmapScenario scenario) {
            if (scenario == null) return null;
            if (!s_bakeStates.TryGetValue(scenario, out var state)) {
                state = new ScenarioBakeState();
                s_bakeStates[scenario] = state;
            }
            return state;
        }

        static void AssignLightingDataAsset(LightmapScenario scenario) {
            if (scenario == null) return;
            var lightingAsset = scenario.LightingDataAsset;
            if (lightingAsset == null) return;

            try {
                var lmType = typeof(UnityEditor.Lightmapping);
                var prop = lmType.GetProperty("lightingDataAsset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null) {
                    string assetPath = AssetDatabase.GetAssetPath(lightingAsset);
                    if (!string.IsNullOrEmpty(assetPath)) {
                        var expectedType = prop.PropertyType;
                        var typedAsset = AssetDatabase.LoadAssetAtPath(assetPath, expectedType);
                        prop.SetValue(null, typedAsset != null ? typedAsset : lightingAsset);
                    } else {
                        prop.SetValue(null, lightingAsset);
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning($"LightmapScenario: Failed to set Lightmapping.lightingDataAsset via reflection: {ex.Message}");
            }

            try {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(activeScene.path);
                if (sceneAsset != null) {
                    var so = new SerializedObject(sceneAsset);
                    var sceneProp = so.FindProperty("m_LightingDataAsset");
                    if (sceneProp != null) {
                        sceneProp.objectReferenceValue = lightingAsset;
                        so.ApplyModifiedProperties();
                        EditorSceneManager.MarkSceneDirty(activeScene);
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning($"LightmapScenario: Failed to assign scene LightingDataAsset: {ex.Message}");
            }
        }

        public static void CaptureCurrentLightmapData(LightmapScenario scenario) {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));

            LightmapData[] lightmaps = SaveLightmapTextures(scenario);

            var scenarioRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(scenario.gameObject);
            var rendererInfos = new List<LightmapScenario.RendererInfo>();
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers) {
                var staticFlags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if ((staticFlags & StaticEditorFlags.ContributeGI) == 0) continue;
                if (renderer.lightmapIndex < 0 || renderer.lightmapIndex >= lightmaps.Length) continue;

                var rendererRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(renderer.gameObject);
                if (scenarioRoot != null && rendererRoot == scenarioRoot) {
                    Debug.LogError($"LightmapScenario[{scenario.name}]: Scenario must not be in the same prefab as renderer objects '{scenarioRoot.name}'", scenario);
                    return;
                }

                rendererInfos.Add(new LightmapScenario.RendererInfo {
                    renderer = renderer,
                    lightmapIndex = renderer.lightmapIndex,
                    lightmapScaleOffset = renderer.lightmapScaleOffset
                });
            }

            scenario.SetRendererData(rendererInfos.ToArray());

            EditorUtility.SetDirty(scenario);
            AssetDatabase.SaveAssetIfDirty(scenario);
            TryAssignLightingDataAssetFromReflection(scenario);
        }

        static LightmapData[] SaveLightmapTextures(LightmapScenario scenario) {
            var lightmaps = LightmapSettings.lightmaps;
            string scenarioFolder = scenario.GetScenarioFolder();

            var lightmapInfos = new LightmapScenario.LightmapInfo[lightmaps.Length];
            for (int i = 0; i < lightmaps.Length; i++) {
                var newColorAsset = AssetHelper.CopyAssetToFolder(lightmaps[i].lightmapColor, scenarioFolder);
                var newDirAsset = AssetHelper.CopyAssetToFolder(lightmaps[i].lightmapDir, scenarioFolder);
                var newMaskAsset = AssetHelper.CopyAssetToFolder(lightmaps[i].shadowMask, scenarioFolder);

                lightmapInfos[i] = new LightmapScenario.LightmapInfo {
                    lightmapColor = newColorAsset,
                    lightmapDir = newDirAsset,
                    shadowMask = newMaskAsset,
                };
            }

            scenario.SetLightmaps(lightmapInfos);
            return lightmaps;
        }

        public static void SaveReflectionProbeTextures(LightmapScenario scenario) {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));

            var probes = scenario.GetReflectionProbes();
            string destFolder = scenario.GetScenarioFolder();

            var pending = new List<KeyValuePair<ReflectionProbe, Texture>>();
            var scenesToMark = new HashSet<UnityEngine.SceneManagement.Scene>();
            var rebakeRequests = new List<KeyValuePair<ReflectionProbe, string>>();

            AssetDatabase.StartAssetEditing();
            try {
                foreach (var probe in probes) {
                    if (probe == null || probe.mode == ReflectionProbeMode.Realtime) continue;

                    if (probe.mode == ReflectionProbeMode.Custom && probe.customBakedTexture == null) {
                        Debug.LogWarning($"LightmapScenario: Probe '{probe.name}' had no baked texture. Mode reset to Baked. Rebake scene to generate textures.", probe);
                        continue;
                    }

                    if (probe.mode == ReflectionProbeMode.Baked) {
                        if (probe.bakedTexture == null) {
                            Debug.LogError($"LightmapScenario: Probe '{probe.name}' had no baked texture to save. Unity error? Try again, or restart Unity if this keeps happening.", probe);
                            continue;
                        }

                        var asset = AssetHelper.CopyAssetToFolder(probe.bakedTexture, destFolder, newName: probe.name);
                        if (asset == null) continue;

                        pending.Add(new KeyValuePair<ReflectionProbe, Texture>(probe, asset));
                        scenesToMark.Add(probe.gameObject.scene);
                    } else {
                        var path = AssetDatabase.GetAssetPath(probe.customBakedTexture);
                        if (string.IsNullOrEmpty(path)) {
                            Debug.LogWarning($"LightmapScenario: Probe '{probe.name}' custom baked texture has no asset path. Skipping re-bake.", probe);
                            continue;
                        }
                        rebakeRequests.Add(new KeyValuePair<ReflectionProbe, string>(probe, path));
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }

            foreach (var request in rebakeRequests) {
                Lightmapping.BakeReflectionProbe(request.Key, request.Value);
            }

            if (pending.Count == 0) return;

            // Mark changes to probes Undoable.
            var probesArray = new ReflectionProbe[pending.Count];
            for (int i = 0; i < pending.Count; i++)
                probesArray[i] = pending[i].Key;
            Undo.RecordObjects(probesArray, "Set Custom Reflection Probe Textures");

            foreach (var kv in pending) {
                var probe = kv.Key;
                var asset = kv.Value;
                probe.customBakedTexture = asset;
                probe.mode = ReflectionProbeMode.Custom;
                EditorUtility.SetDirty(probe);
            }

            foreach (var scene in scenesToMark) {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        public static void BakeAsync(LightmapScenario scenario, Action onComplete) {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            var state = GetState(scenario);
            if (state == null) return;

            if (state.IsBaking) {
                Debug.LogWarning($"LightmapScenario({scenario.name}): BakeAsync called while already baking. Skipping.");
                return;
            }

            state.OnComplete = onComplete;
            state.IsBaking = true;

            Debug.Log($"LightmapScenario({scenario.name}): Starting Bake");
            scenario.ApplyScenario();

            EditorApplication.delayCall += () => {
                state.BakeCompletedHandler ??= () => OnBakeCompleted(scenario);
                Lightmapping.bakeCompleted += state.BakeCompletedHandler;
                bool started = Lightmapping.BakeAsync();
                if (!started) {
                    Debug.LogWarning($"LightmapScenario: BakeAsync failed to start for scenario '{scenario.name}'");
                    Lightmapping.bakeCompleted -= state.BakeCompletedHandler;
                    Cleanup(scenario);
                }
            };
        }

        public static void Cleanup(LightmapScenario scenario) {
            if (scenario == null) return;
            if (!s_bakeStates.TryGetValue(scenario, out var state)) return;

            if (state.BakeCompletedHandler != null) {
                Lightmapping.bakeCompleted -= state.BakeCompletedHandler;
            }

            var callback = state.OnComplete;
            state.OnComplete = null;
            state.BakeCompletedHandler = null;
            state.IsBaking = false;

            callback?.Invoke();
            s_bakeStates.Remove(scenario);
        }

        static void OnBakeCompleted(LightmapScenario scenario) {
            try {
                Debug.Log($"LightmapScenario: OnBakeCompleted invoked for scenario '{scenario.name}' - starting CaptureCurrentLightmapData and probe save");
                AssetDatabase.Refresh();
                CaptureCurrentLightmapData(scenario);
                SaveReflectionProbeTextures(scenario);
                scenario.DisableScenario();
            } finally {
                Cleanup(scenario);
            }
        }

        static void TryAssignLightingDataAssetFromReflection(LightmapScenario scenario) {
            try {
                Debug.Log("LightmapScenario: attempting reflection to UnityEditor.Lightmapping.lightingDataAsset");
                var lmType = typeof(UnityEditor.Lightmapping);
                var prop = lmType.GetProperty("lightingDataAsset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                Object lmAsset = null;
                if (prop != null) {
                    lmAsset = prop.GetValue(null) as Object;
                } else {
                    var field = lmType.GetField("lightingDataAsset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null) lmAsset = field.GetValue(null) as Object;
                }

                if (lmAsset != null) {
                    Debug.Log($"LightmapScenario: Reflection found LightingDataAsset '{lmAsset.name}' at path '{AssetDatabase.GetAssetPath(lmAsset)}'. Attempting CopyAssetToFolder...");
                    var folder = scenario.GetScenarioFolder();
                    var dest = AssetHelper.CopyAssetToFolder(lmAsset, folder);
                    if (dest != null) {
                        Debug.Log($"LightmapScenario: Reflection CopyAssetToFolder returned asset '{dest.name}'");
                        scenario.SetLightingDataAsset(dest);
                        EditorUtility.SetDirty(scenario);
                        AssetDatabase.SaveAssetIfDirty(scenario);
                    } else {
                        Debug.LogWarning("LightmapScenario: Reflection CopyAssetToFolder returned null when copying LightingDataAsset.");
                    }
                } else {
                    Debug.LogWarning("LightmapScenario: No LightingDataAsset found in scene or via Lightmapping API. The Lighting Data Asset must be assigned in the Lighting window to capture it.");
                }
            } catch (Exception ex) {
                Debug.LogWarning($"LightmapScenario: Reflection fallback failed when trying to get LightingDataAsset: {ex.Message}");
            }
        }
    }
}
#endif
