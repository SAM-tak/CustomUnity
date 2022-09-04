using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace CustomUnity
{
    /// <summary>
    /// Lightweight FPS Counter Controller
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        public Sprite[] digitSprites;
        readonly Image[] digits = new Image[3];
        float lastLapForFps;
        int lastLapFrameCount;

        public static bool hideOnRuntime;

#if UNITY_EDITOR
        void Reset()
        {
            digitSprites = AssetDatabase.LoadAllAssetsAtPath("Packages/net.sam-tak.customunity/Sprites/fps counter digit.png")
                .Where(x => x is Sprite)
                .Select(x => x as Sprite)
                .OrderBy(x => x.name)
                .Take(10)
                .ToArray();
            if(digitSprites is not null && digitSprites.Length > 0) {
                digitSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/CustomUnity/Sprites/fps counter digit.png")
                    .Where(x => x is Sprite)
                    .Select(x => x as Sprite)
                    .OrderBy(x => x.name)
                    .Take(10)
                    .ToArray();
            }
        }
#endif

        void Awake()
        {
            var trs = transform.Find("Digit100");
            if(trs) digits[0] = trs.gameObject.GetComponent<Image>();
            trs = transform.Find("Digit010");
            if(trs) digits[1] = trs.gameObject.GetComponent<Image>();
            trs = transform.Find("Digit001");
            if(trs) digits[2] = trs.gameObject.GetComponent<Image>();
        }

        // Use this for initialization
        void Start()
        {
            lastLapForFps = lastLapFrameCount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if(hideOnRuntime) {
                foreach(var i in digits) if(i) i.enabled = false;
                return;
            }
            else foreach(var i in digits) if(i) i.enabled = true;

            if(Time.frameCount % 30 == 0 && Time.frameCount > lastLapFrameCount) {
                var fps = Mathf.RoundToInt(lastLapForFps > 0 ? 30f / (Time.realtimeSinceStartup - lastLapForFps) : 1f / Time.unscaledDeltaTime);
                if(fps > 999) {
                    if(digits[0]) digits[0].sprite = digitSprites[9];
                    if(digits[1]) digits[1].sprite = digitSprites[9];
                    if(digits[2]) digits[2].sprite = digitSprites[9];
                }
                else {
                    if(digits[0]) digits[0].sprite = digitSprites[Mathf.Min(9, fps / 100)];
                    if(digits[1]) digits[1].sprite = digitSprites[fps / 10 % 10];
                    if(digits[2]) digits[2].sprite = digitSprites[fps % 10];
                }
                lastLapForFps = Time.realtimeSinceStartup;
                lastLapFrameCount = Time.frameCount;
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI/FPSCounter")]
        static void CreateFPSCounter(MenuCommand menuCommand)
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/net.sam-tak.customunity/Prefabs/FPSCounter.prefab");
            if(!pf) pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/FPSCounter.prefab");
            if(pf) {
                var parent = (Selection.activeObject ? Selection.activeObject : menuCommand.context) as GameObject;
                var go = Instantiate(pf, parent ? parent.transform : null);
                go.UniqueName(pf.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }
#endif
    }
}
