using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// move camera by player input in runtime.
    /// </summary>
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

        Camera _targetCamera;

        Vector3 _prevMousePos;

        float _centerDist = 3;

        Vector3 _moveVector;

        bool _enableTargetPosition;
        Vector3 _targetPosition;

        void Start()
        {
            _targetCamera = GetComponent<Camera>();
        }

        void Update()
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if(Mathf.Abs(scrollWheel) > 0.0f) {
                _enableTargetPosition = false;
                transform.position += scrollWheel * wheelSpeed * transform.forward;
            }

            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
                _prevMousePos = Input.mousePosition;
            }

            bool brake = true;
            Vector3 diff = Input.mousePosition - _prevMousePos;
            if(Input.GetMouseButtonDown(2) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
                if(Physics.Raycast(_targetCamera.ScreenPointToRay(Input.mousePosition), out var hit)) {
                    _enableTargetPosition = true;
                    _moveVector = Vector3.zero;
                    _targetPosition = hit.point - (transform.TransformDirection(Vector3.forward) * _centerDist);
                }
            }
            else if(Input.GetMouseButton(2)) {
                if(diff.magnitude > Vector3.kEpsilon) {
                    _enableTargetPosition = false;
                    transform.position += GetDragAmount();
                }
                Cursor.SetCursor(handCursor, new Vector2(16, 16), CursorMode.Auto);
            }
            else if(Input.GetMouseButton(1)) {
                if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                    if(diff.magnitude > Vector3.kEpsilon) {
                        _enableTargetPosition = false;
                        var amount = (Mathf.Abs(diff.x) > Mathf.Abs(diff.y) ? diff.x : diff.y) * dollySpeed * Time.unscaledDeltaTime;
                        transform.position += transform.forward * amount;
                        _centerDist -= amount;
                        if(_centerDist < 0.1f) _centerDist = 0.1f;
                    }
                    Cursor.SetCursor(loupeCursor, new Vector2(16, 16), CursorMode.Auto);
                }
                else {
                    if(diff.magnitude > Vector3.kEpsilon) {
                        var swing = rotateSpeed * Time.unscaledDeltaTime * new Vector2(-diff.y, diff.x);
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

                    _moveVector = transform.TransformDirection(v) * (_moveVector.magnitude + moveAcc * Time.unscaledDeltaTime);
                    if(_moveVector.magnitude > maxMoveSpeed) _moveVector = _moveVector.normalized * maxMoveSpeed;

                    Cursor.SetCursor(flyModeCursor, new Vector2(14, 12), CursorMode.Auto);
                }
            }
            else if(Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
                _enableTargetPosition = false;
                var center = transform.TransformPoint(Vector3.forward * _centerDist);
                transform.position += GetDragAmount();
                transform.LookAt(center);
                transform.position += transform.TransformDirection(Vector3.forward) * (Vector3.Distance(transform.position, center) - _centerDist);
                Cursor.SetCursor(orbitCursor, new Vector2(16, 16), CursorMode.Auto);
            }
            else {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            if(_moveVector.sqrMagnitude > Vector3.kEpsilon) {
                _enableTargetPosition = false;
                transform.position += _moveVector * Time.unscaledDeltaTime;
                if(brake) {
                    _moveVector = Math.RubberStep(_moveVector, Vector3.zero, 0.15f, Time.unscaledDeltaTime);
                }
            }

            if(_enableTargetPosition) {
                if(Vector3.Distance(transform.position, _targetPosition) > Vector3.kEpsilon) {
                    transform.position = Math.RubberStep(transform.position, _targetPosition, 0.25f, Time.unscaledDeltaTime);
                }
                else {
                    _enableTargetPosition = false;
                }
            }

            if(diff.magnitude > Vector3.kEpsilon) {
                _prevMousePos = Input.mousePosition;
            }
        }

        public Vector3 GetDragAmount()
        {
            var z = _targetCamera.WorldToScreenPoint(transform.TransformPoint(Vector3.forward * _centerDist)).z;
            var prev = _targetCamera.ScreenToWorldPoint(new Vector3(_prevMousePos.x, _prevMousePos.y, z));
            var now = _targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
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
