using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CustomUnity
{
    /// <summary>
    /// Let object be draggable. For Dubugging / Development / Prototyping use.
    /// </summary>
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public PointerEventData.InputButton dragButton;

        public enum Modifier
        {
            Alt   = 1 << 0,
            Shift = 1 << 1,
            Ctrl  = 1 << 2
        }
        
#if !UNITY_2020_1_OR_NEWER
        [EnumFlags]
#endif
        public Modifier modifier;

        bool MatchesInput(PointerEventData eventData)
        {
            if(eventData.button == dragButton) {
                bool match = true;
                if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && (modifier & Modifier.Alt) == 0) match = false;
                if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (modifier & Modifier.Shift) == 0) match = false;
                if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (modifier & Modifier.Ctrl) == 0) match = false;
                return match;
            }
            return false;
        }

        Graphic _graphic;
        Rigidbody _rb;
        Rigidbody2D _rb2d;
        bool _onDragging;
        Vector3 _lastPosition;

        void OnEnable()
        {
            _onDragging = false;
            _graphic = GetComponent<Graphic>();
            _rb = GetComponent<Rigidbody>();
            _rb2d = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if(_onDragging) {
                if(_rb) {
                    _rb.position = _lastPosition;
                    _rb.linearVelocity = Vector3.zero;
                }
                else if(_rb2d) {
                    _rb2d.position = _lastPosition;
                    _rb2d.linearVelocity = Vector2.zero;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(MatchesInput(eventData)) {
                _onDragging = true;
                if(_rb) _lastPosition = _rb.position + transform.GetDragAmount(eventData);
                else if(_rb2d) _lastPosition = _rb2d.position + (Vector2)transform.GetDragAmount(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(_onDragging) {
                if(_graphic) _graphic.ApplyDrag(eventData);
                else if(_rb) _lastPosition = _rb.position + transform.GetDragAmount(eventData);
                else if(_rb2d) _lastPosition = _rb2d.position + (Vector2)transform.GetDragAmount(eventData);
                else transform.ApplyDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(_onDragging) {
                _onDragging = false;
            }
        }
    }
}