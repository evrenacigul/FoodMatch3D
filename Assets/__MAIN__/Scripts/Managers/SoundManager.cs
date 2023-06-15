using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Managers
{
    [RequireComponent(typeof(AudioListener)), RequireComponent(typeof(AudioSource))]
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        [System.Serializable] public class Sfx 
        {
            [Range(0f, 1f)]
            public float volume = 1f;

            [Range(0f, 1f)]
            public float pitch = 1f;

            public List<AudioClip> clips;
        }
        [System.Serializable] public class SFXDictionary : SerializableDictionary<string, Sfx> { }

        [Header("------Music Settings------")]
        [SerializeField]
        List<AudioClip> backgroundMusicList;

        [SerializeField]
        AudioSource musicPlayer;

        [SerializeField, Range(0f, 1f)]
        float musicVolume;

        [SerializeField, Range(0f, 1f)]
        float musicPitch;

        [SerializeField]
        bool playRandomMusic = false;

        [Header("------SFX Settings------")]
        [SerializeField]
        SFXDictionary sfxList;

        [SerializeField]
        int sfxChannelCount = 4;

        List<AudioSource> channels;

        float volume;
        float pitch;

        void Start()
        {
            if(backgroundMusicList != null && backgroundMusicList.Count > 0)
            {
                musicPlayer.loop = true;

                if (playRandomMusic)
                {
                    var selectRandom = Random.Range(0, backgroundMusicList.Count);
                    musicPlayer.clip = backgroundMusicList[selectRandom];
                }
                else
                {
                    musicPlayer.clip = backgroundMusicList[0];
                }

                musicPlayer.volume = Mathf.Clamp01(musicVolume);
                musicPlayer.pitch = Mathf.Clamp01(musicPitch);

                musicPlayer.Play();
            }

            channels = new();

            for(int i = 0; i < sfxChannelCount; i++)
            {
                GameObject newObj = new();
                newObj.transform.SetParent(transform);
                newObj.name = "SFX Channel " + i.ToString();
                var source = newObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                channels.Add(source);
            }
        }

        public void SetMusicActive(bool isOn)
        {
            musicPlayer.enabled = isOn;
        }

        public void SetSFXActive(bool isOn)
        {
            foreach(AudioSource channel in channels)
            {
                channel.enabled = isOn;
            }
        }

        public void PlaySFX(string sfxName)
        {
            if (sfxList.ContainsKey(sfxName))
            {
                var sfx = sfxList[sfxName];
                SetVolumePitch(sfx.volume, sfx.pitch);
                PlayRandomAudio(sfx.clips);
            }
        }

        private void PlayRandomAudio(List<AudioClip> audioClips)
        {
            var selectRandom = SelectRandom(audioClips);

            if(selectRandom != null)
                PlaySound(selectRandom);
        }

        private void PlaySound(AudioClip clip)
        {
            var source = GetIdleSource();
            if (source is null || clip is null) return;

            source.volume = volume;
            source.pitch = pitch;
            source.PlayOneShot(clip);
        }

        private AudioSource GetIdleSource()
        {
            foreach(AudioSource source in channels)
            {
                if (!source.isPlaying) return source;
            }

            return null;
        }

        private AudioClip SelectRandom(List<AudioClip> audioClips)
        {
            if (audioClips is null || audioClips.Count == 0) return null;

            if (audioClips.Count > 1)
                return audioClips[Random.Range(0, audioClips.Count)];
            else
                return audioClips[0];
        }

        private void SetVolumePitch(float volume, float pitch)
        {
            this.volume = Mathf.Clamp01(volume);
            this.pitch = Mathf.Clamp01(pitch);
        }
    }
}