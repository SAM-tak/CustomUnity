using System.Collections;
using System.Collections.Generic;
using CustomUnity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YourProjectNamespace
{
    public class ScrollElementTest : MonoBehaviour, IPointerDownHandler
    {
        bool inPress;
        int fingerId;

        public void OnPointerDown(PointerEventData eventData)
        {
            if(Input.touchCount > 0) {
                inPress = true;
                fingerId = Input.GetTouch(0).fingerId;
            }
        }

        void Update()
        {
            if(inPress) {
                bool pressing = false;
                for(int i = 0; i < Input.touchCount; ++i) {
                    var touch = Input.GetTouch(i);
                    if(touch.fingerId == fingerId) {
                        pressing = true;
                        break;
                    }
                }

                if(!pressing) {
                    inPress = false;
                    LogInfo("Released");
                }
            }
        }
    }
}
