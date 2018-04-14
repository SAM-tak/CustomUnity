using UnityEngine;

namespace CustomUnity
{
    public class CameraSwitchTest : MonoBehaviour
    {
        public bool smoothDamp;
        public float halfLife = 0.3f;
        public Transform[] positions;
        public int index;

        Vector3 currentTarget;
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

                if(smoothDamp) {
                    currentTarget = Math.SmoothDamp(ref currentVelocity, currentTarget, positions[index].position, halfLife, Time.deltaTime);
                }
                else {
                    currentVelocity = Vector3.zero;
                    currentTarget = Math.RubberStep(currentTarget, positions[index].position, halfLife, Time.deltaTime);
                }
                transform.LookAt(currentTarget);
            }
        }
    }
}
