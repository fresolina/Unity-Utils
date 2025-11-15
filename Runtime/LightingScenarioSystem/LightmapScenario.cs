using System;
using UnityEngine;

namespace Lotec.Lighting {
    public class LightmapScenario : LightingScenario {
        public static event Action<LightmapScenario> ScenarioAppliedInEditor;
        public static event Action<LightmapScenario> ScenarioDisabledInEditor;

        [Header("Lightmap Data")]
        [Tooltip("Array of lightmap textures")]
        [SerializeField] LightmapInfo[] _lightmaps;
        [Tooltip("Lightmap indices and scale offsets for each renderer.")]
        [SerializeField] RendererInfo[] _rendererData = Array.Empty<RendererInfo>();
        [SerializeField] UnityEngine.Object _lightingDataAsset;

        public UnityEngine.Object LightingDataAsset => _lightingDataAsset;
        public void SetLightingDataAsset(UnityEngine.Object asset) => _lightingDataAsset = asset;
        public LightmapInfo[] Lightmaps => _lightmaps;
        public void SetLightmaps(LightmapInfo[] lightmaps) => _lightmaps = lightmaps ?? Array.Empty<LightmapInfo>();
        public RendererInfo[] RendererData => _rendererData;
        public void SetRendererData(RendererInfo[] rendererData) => _rendererData = rendererData ?? Array.Empty<RendererInfo>();

        public override void ApplyScenario() {
            base.ApplyScenario();

            ScenarioAppliedInEditor?.Invoke(this);
            ApplyLightmapData();
        }

        void ApplyLightmapData() {
            if (_lightmaps == null || _lightmaps.Length == 0) return;

            // Build LightmapData[] from the stored lightmap textures and apply it
            var newLightmaps = new LightmapData[_lightmaps.Length];
            for (int i = 0; i < _lightmaps.Length; i++) {
                var lightmapInfo = _lightmaps.Length > i ? _lightmaps[i] : null;
                var lightmapData = new LightmapData();
                if (lightmapInfo != null) {
                    lightmapData.lightmapColor = lightmapInfo.lightmapColor;
                    lightmapData.lightmapDir = lightmapInfo.lightmapDir;
                    lightmapData.shadowMask = lightmapInfo.shadowMask;
                }
                newLightmaps[i] = lightmapData;
            }
            // Assign new lightmaps array
            LightmapSettings.lightmaps = newLightmaps;

            // Update renderer indices/scale offsets using the stored rendererData
            foreach (var info in _rendererData) {
                if (info == null || info.renderer == null) continue;

                info.renderer.lightmapIndex = info.lightmapIndex;
                info.renderer.lightmapScaleOffset = info.lightmapScaleOffset;
            }
        }

        void OnDisable() {
            ScenarioDisabledInEditor?.Invoke(this);
        }

        [System.Serializable]
        public class RendererInfo {
            public Renderer renderer;
            public int lightmapIndex;
            public Vector4 lightmapScaleOffset;
        }

        [System.Serializable]
        public class LightmapInfo {
            public Texture2D lightmapColor;
            public Texture2D lightmapDir;
            public Texture2D shadowMask;
        }
    }
}
