using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnity
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourcePool : MonoBehaviour
    {
        public int quantity = 3;

        AudioSource[] audioSources;
        float[] lastRequestedTime;

        AudioSource TryGetAudioSource()
        {
            for(var i = 0; i < audioSources.Length; i++) {
                if(!audioSources[i].isPlaying) {
                    lastRequestedTime[i] = Time.realtimeSinceStartup;
                    return audioSources[i];
                }
            }
            return null;
        }

        public AudioSource GetAudioSource()
        {
            var ret = TryGetAudioSource();
            if(ret) return ret;
            int index = 0;
            for(var i = 1; i < lastRequestedTime.Length; i++) {
                if(lastRequestedTime[index] > lastRequestedTime[i]) index = i;
            }
            lastRequestedTime[index] = Time.realtimeSinceStartup;
            audioSources[index].Stop();
            ExpirePlayHandle(audioSources[index]);
            return audioSources[index];
        }

        public class PlayHandle
        {
            public AudioSource AudioSource { get; private set; }

            public bool IsPlaying { get { return AudioSource && AudioSource.isPlaying; } }

            public PlayHandle(AudioSource audioSource)
            {
                AudioSource = audioSource;
            }
        }

        public static bool IsPlaying(WeakReference<PlayHandle> playHandle)
        {
            if(playHandle.TryGetTarget(out var i)) return i.IsPlaying;
            return false;
        }

        readonly List<PlayHandle> activePlayHandles = new (64);

        void ExpirePlayHandle(AudioSource audioSource)
        {
            activePlayHandles.RemoveAll(x => x.AudioSource == audioSource);
        }

        WeakReference<PlayHandle> NewPlayHandle(AudioSource audioSource)
        {
            var playHandle = new PlayHandle(audioSource);
            activePlayHandles.Add(playHandle);
            return new WeakReference<PlayHandle>(playHandle);
        }

        public void PlayOneShot(AudioClip clip)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
        }

        public bool TryPlayOneShot(AudioClip clip)
        {
            var audioSource = TryGetAudioSource();
            if(audioSource) {
                audioSource.clip = clip;
                audioSource.loop = false;
                audioSource.Play();
                return true;
            }
            return false;
        }

        public WeakReference<PlayHandle> Play(AudioClip clip, bool loop = false)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
            return NewPlayHandle(audioSource);
        }

        public WeakReference<PlayHandle> TryPlay(AudioClip clip, bool loop = false)
        {
            var audioSource = TryGetAudioSource();
            if(audioSource) {
                audioSource.clip = clip;
                audioSource.loop = loop;
                audioSource.Play();
                return NewPlayHandle(audioSource);
            }
            return null;
        }

        void Start()
        {
            audioSources = new AudioSource[quantity];
            audioSources[0] = GetComponent<AudioSource>();
            for(var i = 1; i < quantity; i++) audioSources[i] = DuplicateAudioSource(audioSources[0]);
            lastRequestedTime = new float[quantity];
            for(var i = 0; i < quantity; i++) lastRequestedTime[i] = float.NegativeInfinity;
        }

        AudioSource DuplicateAudioSource(AudioSource src)
        {
            var ret = gameObject.AddComponent<AudioSource>();

            ret.bypassEffects = src.bypassEffects;
            ret.bypassListenerEffects = src.bypassListenerEffects;
            ret.bypassReverbZones = src.bypassReverbZones;
            ret.dopplerLevel = src.dopplerLevel;
            ret.ignoreListenerPause = src.ignoreListenerPause;
            ret.ignoreListenerVolume = src.ignoreListenerVolume;
            ret.maxDistance = src.maxDistance;
            ret.minDistance = src.minDistance;
            ret.outputAudioMixerGroup = src.outputAudioMixerGroup;
            ret.panStereo = src.panStereo;
            ret.pitch = src.pitch;
            ret.priority = src.priority;
            ret.reverbZoneMix = src.reverbZoneMix;
            ret.rolloffMode = src.rolloffMode;
            ret.spatialBlend = src.spatialBlend;
            ret.spatialize = src.spatialize;
            ret.spatializePostEffects = src.spatializePostEffects;
            ret.spread = src.spread;
            ret.velocityUpdateMode = src.velocityUpdateMode;
            ret.volume = src.volume;

            return ret;
        }
    }
}