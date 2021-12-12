using UnityEngine;

namespace CustomUnity
{
    [RequireComponent(typeof(Camera))]
    public class CameraNavigation : MonoBehaviour
    {
        [Range(0.1f, 10f)]
        public float wheelSpeed = 1f;

        [Range(0.1f, 10f)]
        public float dollySpeed = 5f;

        [Range(1f, 50f)]
        public float maxMoveSpeed = 30f;

        [Range(1f, 50f)]
        public float moveAcc = 20f;

        [Range(1f, 100f)]
        public float rotateSpeed = 45f;

        public Texture2D orbitCursor;
        public Texture2D flyModeCursor;
        public Texture2D handCursor;
        public Texture2D loupeCursor;

        Camera targetCamera;

        Vector3 prevMousePos;

        float centerDist = 3;

        Vector3 moveVector;

        bool enableTargetPosition;
        Vector3 targetPosition;

        void Start()
        {
            targetCamera = GetComponent<Camera>();
        }

        void Update()
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if(Mathf.Abs(scrollWheel) > 0.0f) {
                enableTargetPosition = false;
                transform.position += transform.forward * scrollWheel * wheelSpeed;
            }

            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
                prevMousePos = Input.mousePosition;
            }

            bool brake = true;
            Vector3 diff = Input.mousePosition - prevMousePos;
            if(Input.GetMouseButtonDown(2) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
                if(Physics.Raycast(targetCamera.ScreenPointToRay(Input.mousePosition), out var hit)) {
                    enableTargetPosition = true;
                    moveVector = Vector3.zero;
                    targetPosition = hit.point - (transform.TransformDirection(Vector3.forward) * centerDist);
                }
            }
            else if(Input.GetMouseButton(2)) {
                if(diff.magnitude > Vector3.kEpsilon) {
                    enableTargetPosition = false;
                    transform.position += GetDragAmount();
                }
                Cursor.SetCursor(handCursor, new Vector2(16, 16), CursorMode.Auto);
            }
            else if(Input.GetMouseButton(1)) {
                if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                    if(diff.magnitude > Vector3.kEpsilon) {
                        enableTargetPosition = false;
                        var amount = (Mathf.Abs(diff.x) > Mathf.Abs(diff.y) ? diff.x : diff.y) * dollySpeed * Time.unscaledDeltaTime;
                        transform.position += transform.forward * amount;
                        centerDist -= amount;
                        if(centerDist < 0.1f) centerDist = 0.1f;
                    }
                    Cursor.SetCursor(loupeCursor, new Vector2(16, 16), CursorMode.Auto);
                }
                else {
                    if(diff.magnitude > Vector3.kEpsilon) {
                        var swing = new Vector2(-diff.y, diff.x) * rotateSpeed * Time.unscaledDeltaTime;
                        transform.RotateAround(transform.position, transform.right, swing.x);
                        transform.RotateAround(transform.position, Vector3.up, swing.y);
                    }

                    var v = Vector3.zero;
                    if(Input.GetKey(KeyCode.W)) v += Vector3.forward;
                    if(Input.GetKey(KeyCode.S)) v -= Vector3.forward;
                    if(Input.GetKey(KeyCode.D)) v += Vector3.right;
                    if(Input.GetKey(KeyCode.A)) v -= Vector3.right;
                    if(Input.GetKey(KeyCode.E)) v += Vector3.up;
                    if(Input.GetKey(KeyCode.Q)) v -= Vector3.up;
                    if(v.sqrMagnitude > float.Epsilon) {
                        v.Normalize();
                        brake = false;
                    }

                    moveVector = transform.TransformDirection(v) * (moveVector.magnitude + moveAcc * Time.unscaledDeltaTime);
                    if(moveVector.magnitude > maxMoveSpeed) moveVector = moveVector.normalized * maxMoveSpeed;

                    Cursor.SetCursor(flyModeCursor, new Vector2(14, 12), CursorMode.Auto);
                }
            }
            else if(Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
                enableTargetPosition = false;
                var center = transform.TransformPoint(Vector3.forward * centerDist);
                transform.position += GetDragAmount();
                transform.LookAt(center);
                transform.position += transform.TransformDirection(Vector3.forward) * (Vector3.Distance(transform.position, center) - centerDist);
                Cursor.SetCursor(orbitCursor, new Vector2(16, 16), CursorMode.Auto);
            }
            else {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            if(moveVector.sqrMagnitude > Vector3.kEpsilon) {
                enableTargetPosition = false;
                transform.position += moveVector * Time.unscaledDeltaTime;
                if(brake) {
                    moveVector = Math.RubberStep(moveVector, Vector3.zero, 0.15f, Time.unscaledDeltaTime);
                }
            }

            if(enableTargetPosition) {
                if(Vector3.Distance(transform.position, targetPosition) > Vector3.kEpsilon) {
                    transform.position = Math.RubberStep(transform.position, targetPosition, 0.25f, Time.unscaledDeltaTime);
                }
                else {
                    enableTargetPosition = false;
                }
            }

            if(diff.magnitude > Vector3.kEpsilon) {
                prevMousePos = Input.mousePosition;
            }
        }

        public Vector3 GetDragAmount()
        {
            var z = targetCamera.WorldToScreenPoint(transform.TransformPoint(Vector3.forward * centerDist)).z;
            var prev = targetCamera.ScreenToWorldPoint(new Vector3(prevMousePos.x, prevMousePos.y, z));
            var now = targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
            return prev - now;
        }

        //public Vector2 GetSwingAmount()
        //{
        //    var z = camera.WorldToScreenPoint(transform.TransformPoint(centerDir * centerDist)).z;
        //    var prev = camera.ScreenToWorldPoint(new Vector3(prevMousePos.x, prevMousePos.y, z));
        //    var now = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
        //    var v = Vector3.forward + camera.transform.InverseTransformVector(now - prev);
        //    if(v.sqrMagnitude > float.Epsilon) v.Normalize();
        //    return new Vector2(Mathf.Acos(v.y) * (v.y >= 0 ? -1 : 1), Mathf.Acos(v.x) * (v.x >= 0 ? 1 : -1));
        //}
    }
}
