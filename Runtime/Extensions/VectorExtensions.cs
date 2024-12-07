using UnityEngine;

namespace Lotec.Utils.Extensions {
    public static class VectorExtensions {
        public static Vector3 Invert(this Vector3 vector) {
            return new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);
        }
        public static Vector3 Abs(this Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        // public static Vector3 Clamp(this Vector3 vector, Vector3 clamp) {
        // }
    }
}
