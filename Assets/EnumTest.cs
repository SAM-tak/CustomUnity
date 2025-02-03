using System.Collections;
using System.Collections.Generic;
using System.Text;
using CustomUnity;

namespace YourProjectNamespace
{
    public class EnumTest : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            var sb = new StringBuilder();
            foreach(var i in transform.EnumChildrenRecursive()) {
                sb.AppendLine(i.ToString());
            }
            LogInfo(sb.ToString());
        }

        // Update is called once per frame
        // void Update()
        // {

        // }
    }
}
