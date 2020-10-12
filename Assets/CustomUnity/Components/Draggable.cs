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
        [EnumFlags]
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

        Graphic graphic;
        Rigidbody rb;
        Rigidbody2D rb2d;

        void OnEnable()
        {
            onDragging = false;
            graphic = GetComponent<Graphic>();
            rb = GetComponent<Rigidbody>();
            rb2d = GetComponent<Rigidbody2D>();
        }

        bool onDragging;
        Vector3 lastPosition;

        void FixedUpdate()
        {
            if(onDragging) {
                if(rb) {
                    rb.position = lastPosition;
                    rb.velocity = Vector3.zero;
                }
                else if(rb2d) {
                    rb2d.position = lastPosition;
                    rb2d.velocity = Vector2.zero;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(MatchesInput(eventData)) {
                onDragging = true;
                if(rb) lastPosition = rb.position + transform.GetDragAmount(eventData);
                else if(rb2d) lastPosition = rb2d.position + (Vector2)transform.GetDragAmount(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(onDragging) {
                if(graphic) graphic.ApplyDrag(eventData);
                else if(rb) lastPosition = rb.position + transform.GetDragAmount(eventData);
                else if(rb2d) lastPosition = rb2d.position + (Vector2)transform.GetDragAmount(eventData);
                else transform.ApplyDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(onDragging) {
                onDragging = false;
            }
        }
    }
}