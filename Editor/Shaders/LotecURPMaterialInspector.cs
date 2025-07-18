#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Lotec.Utils.Editor.Shaders {
    public class LotecURPMaterialInspector : ShaderGUI {
        private MaterialProperty _glassMode;
        private MaterialProperty _refractionStrength;
        private MaterialProperty _glassColor;
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
            Material material = materialEditor.target as Material;
            
            // Find glass-related properties
            _glassMode = FindProperty("_GlassMode", properties, false);
            _refractionStrength = FindProperty("_RefractionStrength", properties, false);
            _glassColor = FindProperty("_GlassColor", properties, false);
            
            // Draw the default inspector
            base.OnGUI(materialEditor, properties);
            
            // Automatically adjust render queue based on glass mode
            if (_glassMode != null) {
                bool isGlassEnabled = _glassMode.floatValue > 0.5f;
                
                // Set render queue based on glass mode
                if (isGlassEnabled) {
                    // Glass mode - needs transparent queue
                    if (material.renderQueue != (int)RenderQueue.Transparent) {
                        material.renderQueue = (int)RenderQueue.Transparent;
                        EditorUtility.SetDirty(material);
                    }
                    
                    // Enable glass shader keyword
                    material.EnableKeyword("_GLASSMODE_ON");
                    
                    // Show helpful info
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        "Glass mode is enabled. Render queue automatically set to Transparent (3000) for proper screen texture sampling.", 
                        MessageType.Info
                    );
                } else {
                    // Standard mode - use geometry queue
                    if (material.renderQueue != (int)RenderQueue.Geometry) {
                        material.renderQueue = (int)RenderQueue.Geometry;
                        EditorUtility.SetDirty(material);
                    }
                    
                    // Disable glass shader keyword
                    material.DisableKeyword("_GLASSMODE_ON");
                }
            }
        }
    }
}
#endif
