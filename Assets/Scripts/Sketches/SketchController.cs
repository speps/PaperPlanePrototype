using System.Collections;
using UnityEngine;

public class SketchController : MonoBehaviour
{
    public GameObject dummyCamera;
    public GameObject sketchCam;
    public GameObject currentSketchObject = null;
    public  ObjectPopUp currentPopUp = null;

    void Start()
    {
        Build();
    }

    public void Build()
    {
        
    }

    public void Reset()
    {
        
    }

    public void SetCamera()
    {
        dummyCamera = GameObject.Find("SketchTemp");

        if (sketchCam != null)
        {
            GameObject go = GameObject.Find("SketchCamera");
            Debug.Log(go);
            SketchCamera camera = GameObject.Find("SketchCamera").GetComponent<SketchCamera>();
            camera.Reset();
            camera.StartAnimation(sketchCam, 2);
            return;
        }

        if (dummyCamera != null)
        {
            GameObject go = GameObject.Find("SketchCamera");
            Debug.Log(go);
            SketchCamera camera = GameObject.Find("SketchCamera").GetComponent<SketchCamera>();
            camera.Reset();
            camera.StartAnimation(dummyCamera, 2);
        }
    }

    public void StartAnimation()
    {
        Main.Instance.SketchCamera.Follow(sketchCam);

        if (currentSketchObject.animation != null)
        {
            currentSketchObject.animation.Play();

            //Play Anim Sound
            ObjectPopUp popUp = currentPopUp;
            if (popUp.data.AnimSound.Length > 0 && popUp.data.IsSketch)
            {
                //SoundManager.Instance.PlayCue(popUp.data.AnimSound); SOUND
            }
        }

        if (currentSketchObject.animation != null)
        {
            //Hashtable props = new Hashtable();
            //props.Clear();
            //props.Add("dummyTimer", 1);
            //props.Add("easing", typeof(Ani.Easing.Linear));
            //props.Add("direction", Ani.Easing.InOut);
            //props.Add("atEnd", new AtEndDelegate(OnAnimationEnd));
            //Ani.Mate.Stop(this);
            //Ani.Mate.To(this, currentSketchObject.animation.clip.length, props);

            StartCoroutine(Animate.Lerp(0.0f, currentSketchObject.animation.clip.length, (v, ev) => { if (ev == Animate.Event.End) OnAnimationEnd(); }, Animate.Linear));
        }
    }

    public void OnAnimationEnd()
    {
        // Do something here for sketch animation length
        GameObject sketchObject = currentSketchObject;
        ObjectPopUp popUp = currentPopUp;

        if (sketchObject != null)
        {
            if (sketchObject.animation.GetClipCount() > 1)
            {

                sketchObject.animation.Stop();
                //Debug.Log("clip length: "+sketchClip.length*sketchClip.frameRate);
                sketchObject.animation.wrapMode = WrapMode.Loop;
                Debug.Log("animation count: " + sketchObject.animation.GetClipCount());
                sketchObject.animation.Play("anim2");
                Debug.Log("playing anim2: " + sketchObject.animation.IsPlaying("anim2"));
            }

        }

        currentSketchObject = null;
        currentPopUp = null;
        Main.Instance.FlightController.ReturnToMenu();
        sketchCam = null;
    }

    void Update()
    {
        
    }
}
