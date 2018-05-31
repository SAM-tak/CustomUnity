using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnity
{
    public class EnumTest : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            foreach(var i in transform.EnumChildrenRecursive()) {
                LogInfo(i);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
