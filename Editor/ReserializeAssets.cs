#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lotec.Utils {
    public static class ReserializeAssets {
        static string[] _paths = new string[1];

        [MenuItem("Tools/Lotec/Reserialize assets")]
        static void ReserializeAllAssets() {
            AssetDatabase.ForceReserializeAssets();
        }

        // Reserialize the asset right-clicked at
        [MenuItem("Tools/Lotec/Reserialize selected asset")]
        static void ReserializeAsset() {
            ReserializeObject(Selection.activeObject);
        }

        // Add Reserialize to context menu
        [MenuItem("Assets/Reserialize", false, 0)]
        static void ReserializeAssetFromContextMenu() {
            ReserializeObject(Selection.activeObject);
        }

        static void ReserializeObject(Object obj) {
            _paths[0] = AssetDatabase.GetAssetPath(obj);
            AssetDatabase.ForceReserializeAssets(_paths);
        }
    }
}
#endif
