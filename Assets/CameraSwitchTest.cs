using UnityEngine;
using CustomUnity;

namespace YourProjectNamespace
{
    public class CameraSwitchTest : MonoBehaviour
    {
        public enum SmoothMode
        {
            RubberStep,
            SmoothDamp,
            UnitySmoothDamp,
        }
        public SmoothMode smoothMode;
        public float halfLife = 0.3f;
        public Transform[] positions;
        public int index;

        Vector3 currentTarget;
        Quaternion targetRotation;
        Vector3 currentVelocity;

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void Toggle()
        {
            if(index == 2) index = 4;
            else index = 2;
        }

        public void Next()
        {
            index++;
            if(index >= positions.Length) index = 0;
        }

        void Start()
        {
            currentTarget = transform.position + transform.forward * 5;
            currentVelocity = Vector3.zero;
        }

        void Update()
        {
            if(positions != null && positions.Length > 0) {
                if(index < 0) index = 0;
                if(index >= positions.Length) index = positions.Length - 1;
                currentTarget = smoothMode switch {
                    SmoothMode.RubberStep => Math.RubberStep(currentTarget, positions[index].position, halfLife, Time.deltaTime),
                    SmoothMode.SmoothDamp => Math.SmoothDamp(ref currentVelocity, currentTarget, positions[index].position, halfLife, Time.deltaTime),
                    SmoothMode.UnitySmoothDamp => Vector3.SmoothDamp(currentTarget, positions[index].position, ref currentVelocity, halfLife * 2, float.PositiveInfinity, Time.deltaTime),
                    _ => positions[index].position
                };
                var rotation = transform.rotation;
                transform.LookAt(currentTarget);
                targetRotation = transform.rotation;
                transform.rotation = Math.RubberStep(rotation, targetRotation, halfLife, Time.deltaTime);
                transform.localEulerAngles = transform.localEulerAngles.SetZ(0);
            }
        }
    }
}
