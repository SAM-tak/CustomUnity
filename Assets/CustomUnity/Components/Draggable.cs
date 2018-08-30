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
            onDragging = true;
            if(rb) lastPosition = rb.position + transform.GetDragAmount(eventData);
            else if(rb2d) lastPosition = rb2d.position + (Vector2)transform.GetDragAmount(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(graphic) graphic.ApplyDrag(eventData);
            else if(rb) lastPosition = rb.position + transform.GetDragAmount(eventData);
            else if(rb2d) lastPosition = rb2d.position + (Vector2)transform.GetDragAmount(eventData);
            else transform.ApplyDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDragging = false;
        }
    }
}