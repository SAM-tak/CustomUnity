using UnityEngine;

namespace CustomUnity
{
    [CreateAssetMenu(menuName = "Custom Unity/Spawn Object Parameter")]
    public class SpawnObjectParameter : ScriptableObject
    {
        public string prefabName;
        public string originPath;
        public bool parentToOrigin;
        public bool localPosition;
        public Vector3 position;
        public bool localRotation;
        public Vector3 rotation;

        public Transform GetOriginNode(Transform root)
        {
            return string.IsNullOrEmpty(originPath) ? root : (root?.Find(originPath) ?? root);
        }

        public Vector3 GetPosition(Transform origin)
        {
            return origin ? (localPosition ? origin.TransformPoint(position) : origin.position + position) : position;
        }

        public Quaternion GetRotation(Transform origin)
        {
            return localRotation && origin ? Quaternion.Euler(rotation) * origin.rotation : Quaternion.Euler(rotation);
        }
    }
}