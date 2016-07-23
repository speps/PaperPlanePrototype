using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundComponent : MonoBehaviour
{
    // Sound identification
    public string _SoundName = null;
    private uint _SoundNameCrc = 0xFFFFFF;
    private SoundManager _SoundManager = null;
    public int sourceIndex = -1;

    // Called when the component is added by the engine
    public void Start()
    {

    }

    public void Register()
    {
        //SetSoundManager
        _SoundManager = SoundManager.GetInstance();
        if(null == _SoundManager)
        {
            Debug.Break();
        }

        sourceIndex =_SoundManager.Register(this);
    }

    public void Play()
    {
        _SoundManager.Play(sourceIndex);
    }

    public void Stop()
    {
        _SoundManager.Stop(sourceIndex);
    }

    public void Pause()
    {
        _SoundManager.Pause(sourceIndex);
    }

    public void Resume()
    {
        _SoundManager.Resume(sourceIndex);
    }

    //Called before frame rendering
    public void Update(){}

}