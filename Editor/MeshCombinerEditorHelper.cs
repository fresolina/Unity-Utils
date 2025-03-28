#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lotec.Utils.editor {
    public static class MeshCombinerMenu {
        static readonly MeshCombiner s_meshCombiner = new MeshCombiner();

        [MenuItem("Tools/Lotec/Create simplified mesh")]
        public static void CombineSelectedTransform() {
            Transform container = Selection.activeTransform;
            if (container == null) {
                Debug.LogError("A transform must be selected, that contains the objects");
                return;
            }
            s_meshCombiner.CreateSimplifiedMesh(container);
        }
    }
}
#endif
