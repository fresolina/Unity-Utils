#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Lotec.Utils.editor {
    public class PathInScene {
        [MenuItem("Tools/Lotec/Scene path of selected GameObject")]
        static void PrintScenePath() {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                Debug.Log("No GameObject selected!");
                return;
            }
            
            string path = GetGameObjectPath(selected.transform);
            Debug.Log("Scene path: " + path);
        }

        static string GetGameObjectPath(Transform transform) {
            if (transform.parent == null) {
                return transform.name;
            } else {
                return GetGameObjectPath(transform.parent) + "/" + transform.name;
            }
        }
    }
}
#endif
