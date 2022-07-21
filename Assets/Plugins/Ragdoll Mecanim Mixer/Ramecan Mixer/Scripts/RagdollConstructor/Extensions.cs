#if UNITY_EDITOR
using UnityEngine;

namespace RagdollMecanimMixer {
    public static class TransformExtensions {
        public static Transform FindDeep(this Transform parent, string name) {
            foreach (Transform child in parent) {
                if (child.name == name)
                    return child;
                var result = FindDeep(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2) {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++) {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }
        
        public static Vector3 Abs(this Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Vector3 AlignDirectionTo(this Vector3 v, Transform tr) {
            Vector3 vo = v;
            float dot = -1;

            float tempDot = Vector3.Dot(tr.up, v);
            if (tempDot > dot) {
                vo = tr.up;
                dot = tempDot;
            }
            tempDot = Vector3.Dot(-tr.up, v);
            if (tempDot > dot) {
                vo = -tr.up;
                dot = tempDot;
            }

            tempDot = Vector3.Dot(tr.forward, v);
            if (tempDot > dot) {
                vo = tr.forward;
                dot = tempDot;
            }
            tempDot = Vector3.Dot(-tr.forward, v);
            if (tempDot > dot) {
                vo = -tr.forward;
                dot = tempDot;
            }

            tempDot = Vector3.Dot(tr.right, v);
            if (tempDot > dot) {
                vo = tr.right;
                dot = tempDot;
            }
            tempDot = Vector3.Dot(-tr.right, v);
            if (tempDot > dot) {
                vo = -tr.right;
                dot = tempDot;
            }

            vo = vo.normalized * v.magnitude;
            return vo;
        }
    }
}
#endif