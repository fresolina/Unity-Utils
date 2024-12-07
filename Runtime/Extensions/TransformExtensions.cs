using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Utils.Extensions {
    public static class TransformExtensions {
        public static List<Transform> FindAllRecursively(this Transform transform, string name, List<Transform> matches, bool exactMatch = true) {
            matches ??= new List<Transform>();
            int cnt = transform.childCount;
            if (exactMatch) {
                if (transform.name == name) {
                    matches.Add(transform);
                }
            } else {
                if (transform.name.Contains(name)) {
                    matches.Add(transform);
                }
            }
            for (int i = 0; i < cnt; i++) {
                transform.GetChild(i).FindAllRecursively(name, matches, exactMatch);
            }
            return matches;
        }

        /// <summary>
        /// Searches the children of this transform for a transform name containing 'name'.
        /// </summary>
        /// <param name="name">Child transform name to find</param>
        /// <param name="exactMatch">Name must be an exact match</param>
        /// <returns></returns>
        public static Transform FindRecursively(this Transform transform, string name, bool exactMatch = true) {
            int cnt = transform.childCount;
            if (exactMatch) {
                if (transform.name == name) {
                    return transform;
                }
            } else {
                if (transform.name.Contains(name)) {
                    return transform;
                }
            }
            for (int i = 0; i < cnt; i++) {
                Transform match = transform.GetChild(i).FindRecursively(name, exactMatch);
                if (match != null) {
                    return match;
                }
            }
            return null;
        }
    }
}
