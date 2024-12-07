using UnityEditor;
using UnityEngine;

namespace Lotec.Utils {
    public static class ReserializeAssets {
        static string[] _paths = new string[1];

        [MenuItem("Tools/Lotec/Reserialize assets")]
        static void DoIt() {
            // _paths[0] = AssetDatabase.GetAssetPath(Selection.activeObject);
            // AssetDatabase.ForceReserializeAssets(_paths);
            AssetDatabase.ForceReserializeAssets();
        }
    }
}
