using System.Linq;
using UnityEngine;

namespace CustomUnity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Animator))]
    public class MotionPreviewer : MonoBehaviour
    {
        [HideInInspector]
        public AnimationClip[] clips;
        [HideInInspector]
        public int index = 0;
        [HideInInspector]
        public int frame = 0;

        string[] clipNames = null;
        public string[] ClipNames {
            get {
                if(clipNames == null && clips != null) clipNames = clips.Select(x => x.name).ToArray();
                return clipNames;
            }
        }

        void OnEnable()
        {
            if(Application.isPlaying) return;
            if(clips == null || clips.Length == 0) clips = GetComponent<Animator>().runtimeAnimatorController.animationClips;
        }
        
        void OnDisable()
        {
            if(Application.isPlaying) return;
            clips = null;
            clipNames = null;
        }
        
        // Update is called once per frame
        void Update()
        {
            if(Application.isPlaying) return;
            if(clips != null && clips.Length > 0 && index < clips.Length) {
                clips[index].SampleAnimation(gameObject, frame * (1.0f / 60.0f));
            }
        }

        #region Dummy Event Functions

        void OnAnimatorMove()
        {
            GetComponent<Animator>().applyRootMotion = false;
        }

        void PlaySE(string _)
        {
        }

        void PlayFootSE(string _)
        {
        }

        void PlayVoice(string _)
        {
        }

        void PlayAudio(Object _)
        {
        }

        void SpawnEffect(Object _)
        {
        }

        void StartRenderer(Object _)
        {
        }

        #endregion
    }
}
