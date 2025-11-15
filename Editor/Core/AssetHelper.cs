#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lotec.Utils.Editor {
    public static class AssetHelper {
        public static T CopyAssetToFolder<T>(T asset, string assetFolder, string newName = null)
            where T : Object {
            if (asset == null) return null;
            if (string.IsNullOrEmpty(assetFolder)) throw new ArgumentNullException(nameof(assetFolder));

            var srcPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(srcPath)) return null;

            EnsureFolder(assetFolder);

            string srcFileName = Path.GetFileName(srcPath);
            string srcExt = Path.GetExtension(srcPath);
            string fileName;
            if (!string.IsNullOrEmpty(newName)) {
                var desiredExt = Path.GetExtension(newName);
                fileName = string.IsNullOrEmpty(desiredExt) ? newName + srcExt : newName;
            } else {
                fileName = srcFileName;
            }

            var destAssetPath = assetFolder.TrimEnd('/') + "/" + fileName;
            Debug.Log($"CopyAssetToFolder: src='{srcPath}', dest='{destAssetPath}'");

            if (AssetDatabase.LoadAssetAtPath<Object>(destAssetPath) == null) {
                bool success = AssetDatabase.CopyAsset(srcPath, destAssetPath);
                if (!success) {
                    try {
                        File.Copy(srcPath, destAssetPath, true);
                        Debug.Log($"CopyAssetToFolder: Fallback File.Copy succeeded to '{destAssetPath}'");
                    } catch (Exception ex) {
                        Debug.LogWarning($"CopyAssetToFolder: Fallback File.Copy failed from '{srcPath}' to '{destAssetPath}': {ex.Message}");
                    }
                }
            } else if (!srcPath.Equals(destAssetPath, StringComparison.OrdinalIgnoreCase)) {
                try {
                    File.Copy(srcPath, destAssetPath, true);
                    Debug.Log($"CopyAssetToFolder: Overwrote existing asset file '{destAssetPath}' via File.Copy");
                } catch (Exception ex) {
                    Debug.LogWarning($"CopyAssetToFolder: Failed to overwrite existing asset file '{destAssetPath}': {ex.Message}");
                }
            } else {
                Debug.Log($"CopyAssetToFolder: Source and destination are the same ('{srcPath}'), skipping copy.");
            }

            AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate);
            var loaded = AssetDatabase.LoadAssetAtPath<T>(destAssetPath);
            if (loaded == null) {
                Debug.LogWarning($"CopyAssetToFolder: Loaded asset is null at '{destAssetPath}' after copy/import.");
            } else {
                Debug.Log($"CopyAssetToFolder: Successfully loaded asset '{loaded.name}' ({typeof(T).Name}) from '{destAssetPath}'");
            }

            return loaded;
        }

        static void EnsureFolder(string assetFolder) {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;

            Debug.Log($"Creating folder path: {assetFolder}");
            var parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
