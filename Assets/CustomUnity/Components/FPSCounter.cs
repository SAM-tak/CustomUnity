using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
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
        readonly Image[] _digits = new Image[3];
        float _lastLapForFps;
        int _lastLapFrameCount;

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
            if(trs) _digits[0] = trs.gameObject.GetComponent<Image>();
            trs = transform.Find("Digit010");
            if(trs) _digits[1] = trs.gameObject.GetComponent<Image>();
            trs = transform.Find("Digit001");
            if(trs) _digits[2] = trs.gameObject.GetComponent<Image>();
        }

        // Use this for initialization
        void Start()
        {
            _lastLapForFps = _lastLapFrameCount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if(hideOnRuntime) {
                foreach(var i in _digits) if(i) i.enabled = false;
                return;
            }
            else foreach(var i in _digits) if(i) i.enabled = true;

            if(Time.frameCount % 30 == 0 && Time.frameCount > _lastLapFrameCount) {
                var fps = Mathf.RoundToInt(_lastLapForFps > 0 ? 30f / (Time.realtimeSinceStartup - _lastLapForFps) : 1f / Time.unscaledDeltaTime);
                if(fps > 999) {
                    if(_digits[0]) _digits[0].sprite = digitSprites[9];
                    if(_digits[1]) _digits[1].sprite = digitSprites[9];
                    if(_digits[2]) _digits[2].sprite = digitSprites[9];
                }
                else {
                    if(_digits[0]) _digits[0].sprite = digitSprites[Mathf.Min(9, fps / 100)];
                    if(_digits[1]) _digits[1].sprite = digitSprites[fps / 10 % 10];
                    if(_digits[2]) _digits[2].sprite = digitSprites[fps % 10];
                }
                _lastLapForFps = Time.realtimeSinceStartup;
                _lastLapFrameCount = Time.frameCount;
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
                StageUtility.PlaceGameObjectInCurrentStage(go);
                go.UniqueName(pf.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }
#endif
    }
}
