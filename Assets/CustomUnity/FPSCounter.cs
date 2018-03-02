using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        float lastLapForFps;
        int lastLapFrameCount;
        StringBuilder stringBuilder = new StringBuilder(16);

        // Use this for initialization
        void Start()
        {
            lastLapForFps = lastLapFrameCount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            var text = GetComponent<Text>();
            if(Time.frameCount % 30 == 0 && Time.frameCount > lastLapFrameCount) {
                stringBuilder.Clear();
                stringBuilder.AppendFormat("{0:00.00}", lastLapForFps > 0 ? 30f / (Time.realtimeSinceStartup - lastLapForFps) : 1f / Time.unscaledDeltaTime);
                text.text = stringBuilder.ToString();
                lastLapForFps = Time.realtimeSinceStartup;
                lastLapFrameCount = Time.frameCount;
            }
        }
    }
}
