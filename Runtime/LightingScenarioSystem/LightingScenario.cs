using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
TODO: Handle skybox reflection cubemap asset copying?
*/

namespace Lotec.Lighting {
    public class LightingScenario : MonoBehaviour {
        [Header("Lighting Environment Settings")]
        [SerializeField] RenderSettingsValues _renderSettingsValues;
        [SerializeField] GameObject _objectsParent;

        void Awake() {
            EnsureObjectsParent();
        }

        void EnsureObjectsParent() {
            if (_objectsParent == null) {
                _objectsParent = gameObject;
            }
        }

        public ReflectionProbe[] GetReflectionProbes() {
            return _objectsParent.GetComponentsInChildren<ReflectionProbe>(true);
        }

        public virtual void ApplyScenario() {
            Debug.Log($"LightingScenario: Applying scenario '{name}'");
            _objectsParent.SetActive(true);
            ApplyRenderSettings();
        }

        public virtual void DisableScenario() {
            _objectsParent.SetActive(false);
        }

        // Instance helper: copy current RenderSettings into this asset (can be called from context menu in Inspector)
        [ContextMenu("Copy Current Lighting Environment Settings")]
        public void CopyFromRenderSettings() {
            _renderSettingsValues.skybox = RenderSettings.skybox;
            _renderSettingsValues.ambientMode = RenderSettings.ambientMode;
            _renderSettingsValues.ambientSkyColor = RenderSettings.ambientSkyColor;
            _renderSettingsValues.ambientEquatorColor = RenderSettings.ambientEquatorColor;
            _renderSettingsValues.ambientGroundColor = RenderSettings.ambientGroundColor;
            _renderSettingsValues.ambientLight = RenderSettings.ambientLight;
            _renderSettingsValues.ambientIntensity = RenderSettings.ambientIntensity;

            _renderSettingsValues.ambientProbe = RenderSettings.ambientProbe;

            _renderSettingsValues.fog = RenderSettings.fog;
            _renderSettingsValues.fogMode = RenderSettings.fogMode;
            _renderSettingsValues.fogColor = RenderSettings.fogColor;
            _renderSettingsValues.fogDensity = RenderSettings.fogDensity;
            _renderSettingsValues.fogStartDistance = RenderSettings.fogStartDistance;
            _renderSettingsValues.fogEndDistance = RenderSettings.fogEndDistance;

            _renderSettingsValues.customReflectionTexture = RenderSettings.customReflectionTexture as Cubemap;
            _renderSettingsValues.reflectionIntensity = RenderSettings.reflectionIntensity;
            _renderSettingsValues.reflectionBounces = RenderSettings.reflectionBounces;
            _renderSettingsValues.defaultReflectionMode = RenderSettings.defaultReflectionMode;
            _renderSettingsValues.defaultReflectionResolution = RenderSettings.defaultReflectionResolution;

            _renderSettingsValues.sunSource = RenderSettings.sun;

            _renderSettingsValues.flareFadeSpeed = RenderSettings.flareFadeSpeed;
            _renderSettingsValues.flareStrength = RenderSettings.flareStrength;
            _renderSettingsValues.haloStrength = RenderSettings.haloStrength;

            _renderSettingsValues.subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
        }

        public void ApplyRenderSettings() {
            RenderSettings.skybox = _renderSettingsValues.skybox;
            RenderSettings.ambientMode = _renderSettingsValues.ambientMode;
            RenderSettings.ambientSkyColor = _renderSettingsValues.ambientSkyColor;
            RenderSettings.ambientEquatorColor = _renderSettingsValues.ambientEquatorColor;
            RenderSettings.ambientGroundColor = _renderSettingsValues.ambientGroundColor;
            RenderSettings.ambientLight = _renderSettingsValues.ambientLight;
            RenderSettings.ambientIntensity = _renderSettingsValues.ambientIntensity;

            RenderSettings.ambientProbe = _renderSettingsValues.ambientProbe;

            RenderSettings.fog = _renderSettingsValues.fog;
            RenderSettings.fogMode = _renderSettingsValues.fogMode;
            RenderSettings.fogColor = _renderSettingsValues.fogColor;
            RenderSettings.fogDensity = _renderSettingsValues.fogDensity;
            RenderSettings.fogStartDistance = _renderSettingsValues.fogStartDistance;
            RenderSettings.fogEndDistance = _renderSettingsValues.fogEndDistance;

            RenderSettings.customReflectionTexture = _renderSettingsValues.customReflectionTexture;
            RenderSettings.reflectionIntensity = _renderSettingsValues.reflectionIntensity;
            RenderSettings.reflectionBounces = _renderSettingsValues.reflectionBounces;
            RenderSettings.defaultReflectionMode = _renderSettingsValues.defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = _renderSettingsValues.defaultReflectionResolution;
            RenderSettings.sun = _renderSettingsValues.sunSource;

            RenderSettings.flareFadeSpeed = _renderSettingsValues.flareFadeSpeed;
            RenderSettings.flareStrength = _renderSettingsValues.flareStrength;
            RenderSettings.haloStrength = _renderSettingsValues.haloStrength;

            RenderSettings.subtractiveShadowColor = _renderSettingsValues.subtractiveShadowColor;
        }

        void Reset() {
            CopyFromRenderSettings();
        }

        void OnValidate() {
            EnsureObjectsParent();
        }

        public string GetScenarioFolder() {
            var activeScene = SceneManager.GetActiveScene();
            var sceneAssetPath = activeScene.path;
            string scenarioName = gameObject.name;
            string sceneScenariosFolderName = $"{activeScene.name}-LightingScenarios";
            string sceneAssetDirPath = Path.GetDirectoryName(sceneAssetPath);
            string scenarioFolder = Path.Combine(sceneAssetDirPath, sceneScenariosFolderName, scenarioName).Replace('\\', '/');
            return scenarioFolder;
        }
    }
}
