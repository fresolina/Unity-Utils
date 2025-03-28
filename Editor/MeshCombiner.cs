#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using static Lotec.Utils.MeshCombinerConstants;
using Unwrapping = UnityEditor.Unwrapping;

namespace Lotec.Utils.editor {
    public class MeshCombiner {
        readonly Dictionary<Material, List<CombineInstance>> _simplifiedMeshes = new();

        // Combine children meshes to one mesh.
        // NOTE: Only uses first material of each mesh.
        // * Uses LODGroup component to get least detailed LOD mesh, if available.
        // * If combined mesh has more than 65535 vertices, it gets split into multiple meshes, named _part1, _part2, etc.
        // * If meshes have different materials, they are split into separate meshes.
        // TODO: Add option to support HLOD, with one mesh per Transform container.
        public GameObject CreateSimplifiedMesh(Transform meshGroup) {
            _simplifiedMeshes.Clear();
            CollectMeshCombines(meshGroup);
            Transform container = meshGroup.parent;
            string simplifiedContainerName = $"{meshGroup.name}{SimplifiedPostfix}";

            if (_simplifiedMeshes.Count > 1) {
                container = new GameObject(simplifiedContainerName).transform;
                container.SetParent(meshGroup.parent);
            }

            // Create a new mesh for each material.
            foreach (var kvp in _simplifiedMeshes) {
                List<CombineInstance> combines = kvp.Value;
                Material material = kvp.Key;
                string simplifiedName = $"{meshGroup.name}{SimplifiedPostfix}.{material.name}";
                ProcessCombineInstances(meshGroup, container, simplifiedName, material, combines);
            }

            return container.gameObject;
        }

        void ProcessCombineInstances(Transform meshGroup, Transform container, string simplifiedName, Material material, List<CombineInstance> combines) {
            const int maxVertices = 65535;
            List<CombineInstance> currentCombines = new List<CombineInstance>();
            int currentVertexCount = 0;
            int partIndex = 1;

            // If mesh would have more than 65535 vertices, split into multiple parts.
            foreach (var combine in combines) {
                int vertexCount = combine.mesh.vertexCount;
                if (currentVertexCount + vertexCount > maxVertices && currentCombines.Count > 0) {
                    GameObject part = CreateMeshObject(meshGroup, $"{simplifiedName}_part{partIndex}", material, currentCombines);
                    part.transform.SetParent(container, worldPositionStays: true);
                    partIndex++;
                    currentCombines.Clear();
                    currentVertexCount = 0;
                }
                currentCombines.Add(combine);
                currentVertexCount += vertexCount;
            }

            if (currentCombines.Count > 0) {
                bool isMeshSplit = partIndex > 1;
                string meshName = isMeshSplit ? $"{simplifiedName}_part{partIndex}" : simplifiedName;
                GameObject go = CreateMeshObject(meshGroup, meshName, material, currentCombines);
                if (!isMeshSplit) {
                    go.transform.SetParent(container, worldPositionStays: true);
                }
            }
        }

        GameObject CreateMeshObject(Transform meshGroup, string gameObjectName, Material material, List<CombineInstance> combineInstances) {
            Debug.Log($"Creating {gameObjectName}. {combineInstances.Count} submeshes", meshGroup);
            Mesh mesh = new Mesh { name = gameObjectName };
            mesh.CombineMeshes(combineInstances.ToArray(), true, true, true);
            Unwrapping.GenerateSecondaryUVSet(mesh);
            GameObject go = new GameObject(gameObjectName, typeof(MeshFilter), typeof(MeshRenderer));
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.transform.SetParent(meshGroup.parent);
            return go;
        }

        // Recursively collects mesh combines (CombineInstance) from children.
        void CollectMeshCombines(Transform t) {
            if (t.name.Contains(SimplifiedPostfix)) return; // Skip simplified meshes.

            // If group is a LODGroup, use lowest detailed mesh.
            if (t.TryGetComponent(out LODGroup lodGroup)) {
                CollectLeastDetailedLodCombines(lodGroup);
            } else {
                // Use mesh on this transform.
                AddMeshToCombines(t.GetComponent<Renderer>());
            }

            // Search children recursively.
            for (int i = 0; i < t.childCount; i++) {
                CollectMeshCombines(t.GetChild(i));
            }
        }

        void CollectLeastDetailedLodCombines(LODGroup lodGroup) {
            LOD[] lods = lodGroup.GetLODs();
            LOD simplest = lods[lods.Length - 1];
            foreach (Renderer r in simplest.renderers) {
                AddMeshToCombines(r);
            }
        }

        // void CollectSimplifiedCombines(Transform t) {
        //     // If group already has simplified mesh, use that and skip checking children.
        //     Transform simplifiedTransform = GetSimplifiedTransform(t);
        //     if (simplifiedTransform) {
        //         Renderer simplifiedRenderer = simplifiedTransform.GetComponent<Renderer>();
        //         AddMeshToCombines(simplifiedRenderer);
        //         return;
        //     }
        // }
        // Transform GetSimplifiedTransform(Transform t) {
        //     string simplifiedName = $"/{t.name}{_simplifiedPostfix}";
        //     Transform simplifiedTransform = null;
        //     if (t.parent == null) {
        //         GameObject go = GameObject.Find(simplifiedName);
        //         if (go)
        //             simplifiedTransform = go.transform;
        //     } else {
        //         simplifiedTransform = t.parent.Find(simplifiedName);
        //     }
        //     return simplifiedTransform;
        // }

        void AddMeshToCombines(Renderer renderer) {
            if (!renderer) return;

            // Create new simplified mesh per material.
            Material material = renderer.sharedMaterial;
            if (!_simplifiedMeshes.TryGetValue(material, out var combines)) {
                combines = new List<CombineInstance>();
                _simplifiedMeshes.Add(material, combines);
            }

            Mesh mesh = GetSharedMesh(renderer);
            if (mesh == null) {
                Debug.LogError($"{renderer.name} mesh is null! Skipping.", renderer);
                return;
            }

            CombineInstance combine = new CombineInstance {
                mesh = mesh,
                lightmapScaleOffset = renderer.lightmapScaleOffset,
                transform = renderer.localToWorldMatrix
            };
            combines.Add(combine);
        }

        Mesh GetSharedMesh(Renderer renderer) {
            Mesh mesh;
            // SkinnedMeshRenderer does not have a MeshFilter.
            if (renderer is SkinnedMeshRenderer skinRenderer) {
                mesh = skinRenderer.sharedMesh;
            } else {
                mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            }

            return mesh;
        }
    }
}
#endif
