using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Unit
{
    /// <summary>
    /// Main audio script, allow to play sounds by channel
    /// </summary>
    public class AudioTool:MonoBehaviour
    {
        private static AudioTool instance;

        private Dictionary<string, AudioSource> channelsSfx = new Dictionary<string, AudioSource>();
        private Dictionary<string, AudioSource> channelsMusic = new Dictionary<string, AudioSource>();
        private Dictionary<string, float> channelsVolume = new Dictionary<string, float>();
        private Dictionary<string, float> tchannelsVolume = new Dictionary<string, float>();
        
        [HideInInspector] public float masterVol = 1f;
        [HideInInspector] public float sfxVol = 1f;
        [HideInInspector] public float musicVol = 1f;

        private void Awake()
        {
            LoadPrefs();
            RefreshVolume();
        }

        private void Update()
        {
            foreach (KeyValuePair<string, AudioSource> kvp in channelsMusic)
            {
                if (kvp.Value.isPlaying)
                {
                    float tvol = tchannelsVolume[kvp.Key];
                    float vol = channelsVolume[kvp.Key];
                    vol = Mathf.MoveTowards(vol, tvol, 0.5f * Time.deltaTime);
                    channelsVolume[kvp.Key] = vol;
                    kvp.Value.volume = vol*musicVol;
                    
                    if (vol < 0.01f && tvol < 0.01f)
                        StopMusic(kvp.Key);
                    
                }
            }

            foreach (KeyValuePair<string, AudioSource> kvp in channelsSfx)
            {
                if (kvp.Value.isPlaying)
                {
                    float tvol = tchannelsVolume[kvp.Key];
                    float vol = channelsVolume[kvp.Key];
                    vol = Mathf.MoveTowards(vol, tvol, 0.5f * Time.deltaTime);
                    channelsVolume[kvp.Key] = vol;
                    kvp.Value.volume = vol*sfxVol;
                    if (vol < 0.01f && tvol < 0.01f)
                        StopSFX(kvp.Key);
                }
            }
        }

        public void PlaySFX(string channel,AudioClip sound,float vol = 0.6f,bool priority = true, bool loop = false)
        {
            if(string.IsNullOrEmpty(channel)||sound==null)
                return;
            AudioSource source = GetChannel(channel);
            channelsVolume[channel] = vol;
            tchannelsVolume[channel] = vol;
            
            if (source == null)
            {
                source = CreateChannel(channel);
                channelsSfx[channel] = source;
            }
            
            if (source != null)
            {
                if (priority || !source.isPlaying)
                {
                    source.clip = sound;
                    source.volume = vol * sfxVol;
                    source.loop = loop;
                    source.Play();
                }
            }
        }

        public void PlayMusic(string channel, AudioClip music, float vol = 0.6f,
            bool loop = true)
        {
            if(string.IsNullOrEmpty(channel)||music==null)
                return;
            AudioSource source = GetMusicChannel(channel);
            channelsVolume[channel] = vol;
            tchannelsVolume[channel] = vol;

            if (source == null)
            {
                source = CreateChannel(channel);
                channelsMusic[channel] = source;
            }
            
            if (source != null)
            {
                if (!source.isPlaying || source.clip != music)
                {
                    source.clip = music;
                    source.volume = vol * musicVol;
                    source.loop = loop;
                    source.Play();
                }
            }
        }
        
        //Same as PlaySFX but takes random audio in array
        public void PlaySFX(string channel, AudioClip[] sounds, float vol = 0.6f, bool priority = true, bool loop = false)
        {
            if (sounds != null && sounds.Length > 0)
            {
                AudioClip sound = sounds[Random.Range(0, sounds.Length)];
                PlaySFX(channel, sound, vol, priority, loop);
            }
        }
        
        //Same as PlayMusic but takes random audio in array
        public void PlayMusic(string channel, AudioClip[] musics, float vol = 0.6f, bool loop = false)
        {
            if (musics != null && musics.Length > 0)
            {
                AudioClip music = musics[Random.Range(0, musics.Length)];
                PlayMusic(channel, music, vol, loop);
            }
        }
        
        public void StopSFX(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;

            AudioSource source = GetChannel(channel);
            if (source)
            {
                source.Stop();
            }
        }

        public void StopMusic(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return;

            AudioSource source = GetMusicChannel(channel);
            if (source)
            {
                source.Stop();
            }
        }
        
        public void FadeOutMusic(string channel)
        {
            if (tchannelsVolume.ContainsKey(channel))
                tchannelsVolume[channel] = 0f;
        }
        
        public void FadeOutSFX(string channel)
        {
            if (tchannelsVolume.ContainsKey(channel))
                tchannelsVolume[channel] = 0f;
        }

        public void SetMasterVolume(float value)
        {
            masterVol = value;
            RefreshVolume();
            SavePrefs();
        }
        
        public void SetSFXVolume(float value)
        {
            sfxVol = value;
            RefreshVolume();
            SavePrefs();
        }
        
        public void LoadPrefs()
        {
            masterVol = PlayerPrefs.GetFloat("audio_master_volume", 1f);
            musicVol = PlayerPrefs.GetFloat("audio_music_volume", 1f);
            sfxVol = PlayerPrefs.GetFloat("audio_sfx_volume", 1f);
        }

        public void SavePrefs()
        {
            PlayerPrefs.SetFloat("audio_master_volume", masterVol);
            PlayerPrefs.SetFloat("audio_music_volume", musicVol);
            PlayerPrefs.SetFloat("audio_sfx_volume", sfxVol);
        }
        
        public void RefreshVolume()
        {
            AudioListener.volume = masterVol;
            foreach (KeyValuePair<string, AudioSource> kvp in channelsSfx)
            {
                if (kvp.Value != null)
                {
                    float vol = channelsVolume.GetValueOrDefault(kvp.Key, 0.8f);
                    kvp.Value.volume = vol * sfxVol;
                }
            }

            foreach (KeyValuePair<string, AudioSource> kvp in channelsMusic)
            {
                if (kvp.Value != null)
                {
                    float vol = channelsVolume.GetValueOrDefault(kvp.Key, 0.4f);
                    kvp.Value.volume = vol * musicVol;
                }
            }
        }
        
        public bool IsMusicPlaying(string channel)
        {
            AudioSource source = GetMusicChannel(channel);
            if (source != null)
                return source.isPlaying;
            return false;
        }
        
        private AudioSource CreateChannel(string channel, int priority = 128)
        {
            if (string.IsNullOrEmpty(channel))
                return null;
            GameObject cobj = new GameObject("AudioChannel-" + channel);
            cobj.transform.SetParent(transform);
            AudioSource source = cobj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.priority = priority;
            return source;
        }

        private AudioSource GetMusicChannel(string channel)
        {
            return channelsMusic.GetValueOrDefault(channel);
        }

        private AudioSource GetChannel(string channel)
        {
            return channelsSfx.GetValueOrDefault(channel);
        }
        
        public bool DoesChannelExist(string channel)
        {
            return channelsSfx.ContainsKey(channel);
        }
        
        public bool DoesMusicChannelExist(string channel)
        {
            return channelsMusic.ContainsKey(channel);
        }
        
        public float GetMasterVolume()=> masterVol;
        public float GetSFXVolume()=> sfxVol;
        public float GetMusicVolume()=> musicVol;
        
        public static AudioTool Get()
        {
            if (instance == null)
            {
                GameObject audioSystem = new GameObject("AudioSystem");
                instance = audioSystem.AddComponent<AudioTool>();
                DontDestroyOnLoad(audioSystem);
            }
            return instance;
        }

        

        
        
    }
}