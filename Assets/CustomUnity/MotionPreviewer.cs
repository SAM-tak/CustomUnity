using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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

        AnimationClipPlayable[] clipPlayables;
        AnimationPlayableOutput playableOutput;
        PlayableGraph playableGraph;
        bool playableGraphCreated = false;

        void OnEnable()
        {
            if(Application.isPlaying) return;
            // Destroys all Playables and Outputs created by the graph.
            if(!playableGraphCreated) {
                playableGraphCreated = true;
                playableGraph = PlayableGraph.Create();
                playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

                playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

                clips = GetComponent<Animator>().runtimeAnimatorController.animationClips;
                if(clips != null && clips.Length > 0) {
                    clipPlayables = new AnimationClipPlayable[clips.Length];
                    for(int i = 0; i < clips.Length; ++i) {
                        clipPlayables[i] = AnimationClipPlayable.Create(playableGraph, clips[i]);
                    }
                }
            }
        }
        
        void OnDisable()
        {
            if(Application.isPlaying) return;
            if(playableGraphCreated) {
                playableGraphCreated = false;
                if(clipPlayables != null && clipPlayables.Length > 0) {
                    clipPlayables[0].SetTime(0.0f);
                    playableOutput.SetSourcePlayable(clipPlayables[0]);
                    playableGraph.Evaluate();
                }
                // Destroys all Playables and Outputs created by the graph.
                playableGraph.Destroy();
            }
            clips = null;
            clipNames = null;
        }
        
        // Update is called once per frame
        void Update()
        {
            if(Application.isPlaying) return;
            if(clipPlayables != null && clipPlayables.Length > 0 && index < clipPlayables.Length) {
                clipPlayables[index].SetTime(frame * (1.0f / 60.0f));
                playableOutput.SetSourcePlayable(clipPlayables[index]);
                playableGraph.Evaluate();
            }
        }
    }
}