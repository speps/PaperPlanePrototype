using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ObjectPopUp : MonoBehaviour
{
    public enum State
    {
        InGround,
        Popped,
        Activated
    }

    public enum SoundType
    {
        None,
        Heavy,
        Medium,
        Light
    }

    public Vector3 size;

    GameObject paint;
    public ObjectPopUpPaint paintComponent;


    public List<ObjectPopUpPart> parts = new List<ObjectPopUpPart>();

    public PopDataAsset data;

    public State state = State.InGround;

    public GameObject SketchObject = null;
    public GameObject SketchCam = null;

    public int Counter = 0;
    public int TextAnimEnds = 0;

    SoundManager _SoundManager = null;
    static private uint lightSoundHandle = 0xFFFFFFFF;
    static private uint mediumSoundHandle = 0xFFFFFFFF;
    static private uint heavySoundHandle = 0xFFFFFFFF;
    private uint customSoundHandle = 0xFFFFFFFF;

    public static GameObject Find(string name)
    {
        var popName = HelperConstants.PrefixPopUp + name;
        popName = popName.ToUpperInvariant();
        var popup = GameObject.Find(popName);
        if (popup == null)
        {
            Debug.LogError(string.Format("PopUp {0} ({1}) not found", popName, name));
        }
        return popup;
    }

    public void RetrieveSoundHandles()
    {
        _SoundManager = SoundManager.GetInstance();
        if (lightSoundHandle == 0xFFFFFFFF)
        {
            lightSoundHandle = _SoundManager.GetSoundHandle("pop_light");
            mediumSoundHandle = _SoundManager.GetSoundHandle("pop_heavy");
            heavySoundHandle = _SoundManager.GetSoundHandle("pop_heavy");
            Debug.Log("LightSound is : "+lightSoundHandle);
            Debug.Log("MediumSound is : " + mediumSoundHandle);
            Debug.Log("HeavySound is : " + heavySoundHandle);
        }

        if (data.DefaultSound == SoundType.None && data.CustomPopSound.Length != 0)
        {
            customSoundHandle = _SoundManager.GetSoundHandle(data.CustomPopSound);
            Debug.Log("CustomSound is : " + customSoundHandle);
        }
    }

    private string realName = null;

    public string RealName
    {
        get { return realName; }
    }

    public void Start()
    {
        if (!HelperConstants.ExtractPopUpName(gameObject.name, out realName))
        {
            Debug.LogError(string.Format("{0} is not a popup", gameObject.name));
        }
        gameObject.name = gameObject.name.ToUpperInvariant();

        Init();

        if (data.IsSketch)
        {
            if(!data.AnimName.Equals("#not set#"))
            {
                //Loading prefab
                GameObject prefab = (GameObject)Resources.Load(string.Format("Sketches/{0}/{0}", data.AnimName));
                if (prefab == null) Debug.LogError(string.Format("Anim prefab {0} doesn't exist", string.Format("{0}/{0}", data.AnimName)));
                if (prefab != null)
                {
                    SketchObject = (GameObject)GameObject.Instantiate(prefab);

                    // Is there a sketchCamera
                    if (SketchObject != null)
                    {
                        SketchObject.animation.playAutomatically = false;
                        Transform transCam = FindCamera(SketchObject.transform);
                        if (transCam != null)
                        {
                            SketchCam = transCam.gameObject;
                        }

                        if (!SketchCam)
                        {
                            Debug.Log(string.Format("No Sketch camera for {0}", data.AnimName));
                            SketchObject.animation.playAutomatically = true;
                        }
                        SketchObject.SetActiveRecursively(false);
                    }

                }
            }
            

            

        }
    }

    public Transform FindCamera(Transform toSearch)
    {
        if (HelperConstants.IsCam(toSearch.name))
        {
            return toSearch;
        }

        foreach (Transform child in toSearch)
        {
            Transform result = FindCamera(child);
            if (result != null)
                return result;
        }

        return null;
    }

    public void Init()
    {
        Load();

        ComputeSize();

        //data.Activated = true;
        state = data.Activated ? State.Popped : State.InGround;

        /* Parts */
        {
            parts.Clear();
            foreach (Transform child in transform)
            {
                GameObject go = child.gameObject;
                if (HelperConstants.IsPart(go.name))
                {
                    ObjectPopUpPart part = go.GetComponent<ObjectPopUpPart>();
                    if (part == null)
                    {
                        part = go.AddComponent<ObjectPopUpPart>();
                    }
                    part.parent = this;
                    part.Init();
                    part.enabled = false;
                    foreach (AnimateTexture at in part.splash)
                    {
                        at.OnAnimateEnd += OnTextureAnimEnd;
                    }
                    parts.Add(part);
                }
            }
        }

        if (paint == null)
        {
            paint = new GameObject("PopUpPaint");
        }
        paint.transform.parent = transform;
        paint.transform.localPosition = Vector3.zero;
        paint.transform.localRotation = Quaternion.identity;

        if (paintComponent == null)
        {
            paintComponent = paint.AddComponent<ObjectPopUpPaint>();
        }
        paintComponent.parent = this;
        paintComponent.radiusMax = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 2.0f;
        paintComponent.Radius = 0.0f;
        if (data.Activated)
        {
            paintComponent.Radius = paintComponent.radiusMax;
        }
    }

    public void Load()
    {
        data = (PopDataAsset)Resources.Load("PopUps/" + RealName);
    }

    public void ComputeSize()
    {
        Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);

        Helper.RecursiveMinMax(gameObject, ref min, ref max);

        size = max - min;
    }

    public void GoToNextState()
    {
        if (state == State.InGround)
        {
            SetState(State.Popped);
        }
        else if (state == State.Popped)
        {
            SetState(State.Activated);
        }
    }

    public void GoToPreviousState()
    {
        if (state == State.Activated)
        {
            SetState(State.Popped);
        }
        else if (state == State.Popped)
        {
            SetState(State.InGround);
        }
    }

    public void OnTextureAnimEnd()
    {
        if(Counter >= data.SketchCounter)
        {
            TextAnimEnds++;
            //Debug.Log(string.Format("n° parts:{0}", parts.Count));
            if (TextAnimEnds >= parts.Count)
            {
                //Debug.Log(string.Format("n° parts:{0} Replace Object, Play Anim", parts.Count));
                if(SketchObject != null)
                {
                    //SketchObject.SetActiveRecursively(true);
                    ////Set the camera correctly
                    //Main.Instance.SketchController.sketchCam = SketchCam;
                    //Main.Instance.SketchController.currentSketchObject = SketchObject;
                    //Main.Instance.SketchController.currentPopUp = this;
                    //if(SketchCam != null)
                    //    Main.Instance.Play(PlayMode.Sketch);

                    gameObject.SetActiveRecursively(false);
                    GameObject objToKill = null;
                    foreach(string toKill in data.KillDuringSketch)
                    {
                        if(toKill.Length > 0)
                        {
                            objToKill = ObjectPopUp.Find(toKill);
                            if(objToKill != null) objToKill.SetActiveRecursively(false);
                        }
                    }
                }
            }
        }
        
    }

    void SetState(State newState)
    {
        //Debug.Log(string.Format("SetState {0} from {1} to {2}", gameObject.name, state, newState));

        if (state == State.InGround && newState == State.Popped)
        {
            Pop();
        }
        if (state == State.Popped && newState == State.Activated)
        {
            Activate();
        }
        if (state == State.Activated && newState == State.Popped)
        {
            Deactivate();
        }
        if (state == State.Popped && newState == State.InGround)
        {
            Unpop();
        }
        
        state = newState;
    }

    void Activate()
    {
        //Debug.Log(string.Format("Pop {0} activated ({1} parts)", gameObject.name, parts.Count));

        Main.Instance.AddPopUp();
        Main.Instance.MenuController.book.Activate(RealName);

        if (data.IsSketch && Counter >= data.SketchCounter && SketchObject != null)
        {
            SketchObject.SetActiveRecursively(true);
            //Set the camera correctly
            Main.Instance.SketchController.sketchCam = SketchCam;
            Main.Instance.SketchController.currentSketchObject = SketchObject;
            Main.Instance.SketchController.currentPopUp = this;
            if (SketchCam != null)
            {
                Main.Instance.Play(PlayMode.Sketch);
                Main.Instance.SketchController.SetCamera();
            }
        }

        foreach (ObjectPopUpPart part in parts)
        {
            part.Activate();
        }

        foreach (string linked in data.Linked)
        {
            GameObject go = ObjectPopUp.Find(linked);
            if (go != null)
            {
                ObjectPopUp objectPopUp = go.GetComponent<ObjectPopUp>();
                if (objectPopUp != null && objectPopUp.state == State.InGround)
                {
                    if(objectPopUp.data.IsSketch)
                    {
                        objectPopUp.Touch();
                        continue;
                    }
                    objectPopUp.GoToNextState();
                }
            }
        }
    }

    public void Touch()
    {
        Counter++;
        //Debug.Log(string.Format("Counter({0}): {1}", gameObject.name, Counter));
        if(Counter >= data.SketchCounter)
        {
            GoToNextState();
        }
    }

    public void UnTouch()
    {
        Counter--;
        //GoToPreviousState();
    }

    void Deactivate()
    {
        if (data.PopActivates)
            return;

        Main.Instance.RemovePopUp();

        foreach (ObjectPopUpPart part in parts)
        {
            part.Deactivate();
        }

        foreach (string linked in data.Linked)
        {
            GameObject go = ObjectPopUp.Find(linked);
            if (go != null)
            {
                ObjectPopUp objectPopUp = go.GetComponent<ObjectPopUp>();
                if (objectPopUp != null && objectPopUp.state == State.Popped)
                {
                    if (objectPopUp.data.IsSketch)
                    {
                        objectPopUp.UnTouch();
                    }
                    else
                    {
                        objectPopUp.GoToPreviousState();
                    }
                    
                }
            }
        }
    }

    void Pop()
    {
        if (customSoundHandle == 0xFFFFFFFF)
        {
            if (data.DefaultSound == SoundType.Light)
            {
                Debug.Log("Light Sound");
                _SoundManager.PlayOneShot(lightSoundHandle);
            }

            if (data.DefaultSound == SoundType.Medium)
            {
                Debug.Log("Medium Sound");
                _SoundManager.PlayOneShot(mediumSoundHandle);
            }

            if (data.DefaultSound == SoundType.Heavy)
            {
                Debug.Log("Heavy Sound");
                _SoundManager.PlayOneShot(heavySoundHandle);
            }

        }
        else
        {
            Debug.Log("CustomSound "+data.CustomPopSound);
            _SoundManager.PlayOneShot(customSoundHandle);
        }

        //Hashtable props = new Hashtable();
        //props.Add("Radius", paintComponent.radiusMax);
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.Out);
        //props.Add("delay", 0.4f);
        //Ani.Mate.Stop(paintComponent);
        //Ani.Mate.To(paintComponent, 1, props);

        StartCoroutine(Animate.LerpTo(0.5f, 1.0f, paintComponent.Radius, paintComponent.radiusMax, (v, ev) => paintComponent.Radius = v, Animate.QuadraticOut));

        float partMaxDelay = 0.0f;
        foreach (ObjectPopUpPart part in parts)
        {
            partMaxDelay = Mathf.Max(partMaxDelay, part.delay);
            part.Pop();
        }

        if (data.PopActivates)
        {
            //props.Clear();
            //props.Add("dummyAnimPopActivates", 1.0f);
            //props.Add("atEnd", new AtEndDelegate(Activate));
            //Ani.Mate.Stop(this);
            //Ani.Mate.By(this, partMaxDelay + 1.0f, props);

            StartCoroutine(Animate.Lerp(0.0f, partMaxDelay + 1.0f, (v, ev) => { if (ev == Animate.Event.End) Activate(); }, Animate.Linear));
        }

        data.Activated = true;
    }

    void OnDrawGizmos()
    {
        if (GameData.Instance.Tweak.DrawPopUpGraph)
        {
            GameObject obj = null;
            foreach (string objName in data.Linked)
            {
                obj = ObjectPopUp.Find(objName);
                Gizmos.color = Color.green;
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                    Vector3 dir = obj.transform.position - transform.position;
                    dir = dir.normalized;
                    Gizmos.DrawSphere(obj.transform.position, 0.3f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, transform.position + dir);
                }
            }
        }
    }

    void Unpop()
    {
        //Hashtable props = new Hashtable();
        //props.Add("Radius", 0.0);
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.Out);
        //Ani.Mate.Stop(paintComponent);
        //Ani.Mate.To(paintComponent, 1, props);

        StartCoroutine(Animate.LerpTo(0.0f, 1.0f, paintComponent.Radius, 0.0f, (v, ev) => paintComponent.Radius = v, Animate.QuadraticOut));

        foreach (ObjectPopUpPart part in parts)
        {
            part.Unpop();
        }

        data.Activated = false;
    }

    void Update()
    {
        
    }
}
