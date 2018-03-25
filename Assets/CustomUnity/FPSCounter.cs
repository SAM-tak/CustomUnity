using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace CustomUnity
{
    public class FPSCounter : MonoBehaviour
    {
        public Sprite[] digitSprites;

        Image[] digits = new Image[4];
        float lastLapForFps;
        int lastLapFrameCount;

#if UNITY_EDITOR
        void Reset()
        {
            digitSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/CustomUnity/Sprites/fps counter digit.png")
                .Where(x => x is Sprite)
                .Select(x => x as Sprite)
                .OrderBy(x => x.name)
                .Take(10)
                .ToArray();
        }
#endif

        // Use this for initialization
        void Start()
        {
            digits[0] = transform.Find("Digit100.0")?.gameObject.GetComponent<Image>();
            digits[1] = transform.Find("Digit010.0")?.gameObject.GetComponent<Image>();
            digits[2] = transform.Find("Digit001.0")?.gameObject.GetComponent<Image>();
            digits[3] = transform.Find("Digit000.1")?.gameObject.GetComponent<Image>();
            lastLapForFps = lastLapFrameCount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if(Time.frameCount % 30 == 0 && Time.frameCount > lastLapFrameCount) {
                var fps = lastLapForFps > 0 ? 30f / (Time.realtimeSinceStartup - lastLapForFps) : 1f / Time.unscaledDeltaTime;
                if(fps > 999.9) {
                    if(digits[0]) digits[0].sprite = digitSprites[9];
                    if(digits[1]) digits[1].sprite = digitSprites[9];
                    if(digits[2]) digits[2].sprite = digitSprites[9];
                    if(digits[3]) digits[3].sprite = digitSprites[9];
                }
                else {
                    if(digits[0]) digits[0].sprite = digitSprites[Mathf.Min(9, (int)(fps / 100))];
                    if(digits[1]) digits[1].sprite = digitSprites[(int)(fps / 10) % 10];
                    if(digits[2]) digits[2].sprite = digitSprites[(int)(fps) % 10];
                    if(digits[3]) digits[3].sprite = digitSprites[(int)(fps * 10) % 10];
                }
                lastLapForFps = Time.realtimeSinceStartup;
                lastLapFrameCount = Time.frameCount;
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI/FPSCounter")]
        static void CreateFPSCounter(MenuCommand menuCommand)
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/FPSCounter.prefab");
            if(pf) {
                var parent = (Selection.activeObject ?? menuCommand.context) as GameObject;
                var go = Instantiate(pf, parent?.transform);
                go.UniqueName(pf.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }
#endif
    }
}
