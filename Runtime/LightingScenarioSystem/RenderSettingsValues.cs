using UnityEngine;
using UnityEngine.Rendering;

namespace Lotec.Lighting {
    [System.Serializable]
    public struct RenderSettingsValues {
        [Header("Sky & Ambient")]
        public Material skybox;
        public AmbientMode ambientMode;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public float ambientIntensity;
        public Color ambientLight; // legacy

        // ambient probe (advanced)
        public SphericalHarmonicsL2 ambientProbe;

        [Header("Fog")]
        public bool fog;
        public FogMode fogMode;
        public Color fogColor;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;

        [Header("Reflections")]
        public Cubemap customReflectionTexture;
        public float reflectionIntensity;
        public int reflectionBounces;
        public DefaultReflectionMode defaultReflectionMode;
        public int defaultReflectionResolution;

        public Light sunSource;

        [Header("Flares/Halos")]
        public float flareFadeSpeed;
        public float flareStrength;
        public float haloStrength;

        [Header("Misc")]
        public Color subtractiveShadowColor;
    }
}
