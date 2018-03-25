using UnityEngine;

namespace CustomUnity
{
    [CreateAssetMenu(menuName = "Custom Unity/SpawnObject Parameter")]
    public class SpawnObjectParameter : ScriptableObject
    {
        public string prefabName;
        public string pivotPath;
        public bool parenting;
        public bool localPosition;
        public Vector3 position;
        public bool localRotation;
        public Vector3 rotation;

        public Transform GetPivotNode(Transform root)
        {
            return string.IsNullOrEmpty(pivotPath) ? root : (root?.Find(pivotPath) ?? root);
        }

        public Vector3 GetPosition(Transform pivot)
        {
            return pivot ? (localPosition ? pivot.TransformPoint(position) : pivot.position + position) : position;
        }

        public Quaternion GetRotation(Transform pivot)
        {
            return localRotation && pivot ? Quaternion.Euler(rotation) * pivot.rotation : Quaternion.Euler(rotation);
        }
    }
}