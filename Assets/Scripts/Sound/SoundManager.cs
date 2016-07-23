using UnityEngine;
using System.Collections.Generic;

public class SoundManager
{
    // Singleton function handler
    public static void CreateSingleton(AudioSource globalSource) 
    { 
        _Instance = new SoundManager();
        _GlobalSource = globalSource;
        if (globalSource == null)
        {
            Debug.DebugBreak();
        }
    }
    public static void DestroySingleton() { _Instance = null; }
    public static SoundManager GetInstance() { return _Instance; }

    // Singleton static instance
    private static SoundManager _Instance = null;
    static AudioSource _GlobalSource = null;

    // Members
    private AudioClip[] _AudioClips = null;
    private AudioListener _currentListener = null;
    private List<SoundComponent> _SoundComponents = new List<SoundComponent>();
    private List<AudioSource> sources = new List<AudioSource>();
    private List<bool> paused = new List<bool>();

    // private ctor
    private SoundManager() { }

    public void LoadAllSounds()
    {
        Object[] audioclips = Resources.LoadAll("Sounds", typeof(AudioClip));
        int nbClip = audioclips.Length;

        Debug.Log("LoadAllSounds " + nbClip + "sounds loaded");
        _AudioClips = new AudioClip[nbClip];

        int index = 0;
        foreach (var bob in audioclips)
        {
            _AudioClips[index] = (AudioClip)bob;
            //Debug.Log("Audioclip name : " + bob.name);
            ++index;
        }
    }

    public uint GetSoundHandle( string _soundName )
    {
        uint handle = 0xFFFFFFFF;
        int nbClips = _AudioClips.Length;
        for(int idx = 0; idx < nbClips; ++idx)
        {
            AudioClip clip = _AudioClips[idx];
            if (_soundName == clip.name)
            {
                handle = (uint)idx;
                break;
            }
        }
        if(handle == 0xFFFFFFFF)
        {
            Debug.Log("Sound does not exist");
            Debug.DebugBreak();
        }
        return handle;
    }

    //Component related methods
    public int Register(SoundComponent _soundComponent)
    {
        _SoundComponents.Add(_soundComponent);
        AudioSource source = _soundComponent.gameObject.GetComponent<AudioSource>();
        uint handle = GetSoundHandle(_soundComponent._SoundName);
        source.clip = _AudioClips[handle];
        sources.Add(source);
        paused.Add(false);
        int sourceHandle = sources.IndexOf(source);
        if(source.playOnAwake) Play(sourceHandle);
        return sourceHandle;
    }

    public void PlayOneShot(uint _soundHandle)
    {
        if (_soundHandle > _AudioClips.Length - 1 || _soundHandle == 0xFFFFFFFF)
        {
            Debug.DebugBreak();
        }
        AudioClip audioClip = _AudioClips[_soundHandle];
        Debug.Log("PlayOneShot : "+audioClip.name);
        _GlobalSource.PlayOneShot(audioClip);
    }


    public void SetAudioListener(GameObject go)
    {
        AudioListener audioListener = go.GetComponent<AudioListener>();
        if (audioListener == null)
        {
            audioListener = go.AddComponent<AudioListener>();
        }

        if (_currentListener)
        {
            _currentListener.enabled = false;
        }

        _currentListener = audioListener;
        _currentListener.enabled = true;
    }


    // Global stop
    public void Stop()
    {
        foreach (var source in sources)
        {
            source.Stop();
        }
    }

    // Global pause
    public void Pause()
    {
        int nbSource = sources.Count;
        for(int idx = 0; idx < nbSource; ++idx)
        {
            AudioSource source = sources[idx];
            if (source.isPlaying)
            {
                source.Pause();
                paused[idx] = true;
            }
        }
    }


    // Global resume
    public void Resume()
    {
        int nbSource = sources.Count;
        for (int idx = 0; idx < nbSource; ++idx)
        {
            if (paused[idx])
            {
                AudioSource source = sources[idx];
                source.Play();
                paused[idx] = false;
            }
        }
    }

    public void Play(int _sound)
    {
        Debug.Log("Play : "+sources[_sound].clip.name);
        sources[_sound].Play();
        paused[_sound] = false;
    }

    public void Pause(int _sound)
    {
        sources[_sound].Pause();
        paused[_sound] = true;
    }

    public void Resume(int _sound)
    {
        sources[_sound].Play();
        paused[_sound] = false;
    }

    public void Stop(int _sound)
    {
        sources[_sound].Stop();
        paused[_sound] = false;
    }

    public void SendEvent(uint _soundEvent, uint _soundId, uint _value)
    {
    }

    public void SendEvent(uint _soundEvent, uint _soundId, Vector3 _value)
    {
    }
}