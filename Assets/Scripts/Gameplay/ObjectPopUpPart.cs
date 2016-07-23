using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random=UnityEngine.Random;

public class ObjectPopUpPart : MonoBehaviour
{
    bool init = false;
    Vector3 startScale;

    //Vector3 groundPosition;
    //Vector3 groundNormal;

    public ObjectPopUp parent;
    public GameObject strips;

    public bool noStrips = false;
    public List<AnimateTexture> splash = new List<AnimateTexture>();

    public Vector3 size;

    public float delay = 0.0f;

    public void Init()
    {
        if (!init)
        {
            startScale = transform.localScale;
            init = true;
        }

        ComputeSize();

        int delayms;
        string temp;
        HelperConstants.ExtractPart(gameObject.name, out delayms, out noStrips, out temp);
        delay = delayms / 1000.0f;

        transform.localScale = parent.data.Activated ? startScale : Vector3.one * 0.01f;
        if (renderer != null)
            renderer.enabled = parent.data.Activated;

        /* Find ground position */
        {
            /*Vector3 v = transform.position + Vector3.up * parent.size.y;
            Ray ray = new Ray(v, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                groundPosition = hit.point;
            }*/
        }

        /* Animated material */
        {
            foreach (AnimateTexture animateTexture in splash)
            {
                Destroy(animateTexture);
            }

            splash.Clear();
            splash = AnimateTextureOnObject(gameObject);
        }

        if (parent.data.Activated)
        {
            foreach (AnimateTexture animateTexture in splash)
            {
                animateTexture.ToEnd();
            }
        }
        else
        {
            foreach (AnimateTexture animateTexture in splash)
            {
                animateTexture.ToStart();
            }
        }
    }

    public void ComputeSize()
    {
        Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);

        Helper.RecursiveMinMax(gameObject, ref min, ref max);

        size = max - min;
    }

    public List<AnimateTexture> AnimateTextureOnObject(GameObject go)
    {
        List<AnimateTexture> animateTextures = new List<AnimateTexture>();

        if (go.renderer != null)
        {
            AnimateTexture splash = go.AddComponent<AnimateTexture>();
            splash.texName = "_SplashMap";
            splash.nImagesX = 11;
            splash.SetFromTime(1.0f);
            splash.UpdateUV();
            splash.enabled = false;

            foreach (Material material in go.renderer.materials)
            {
                if (material.HasProperty("_SplashMap"))
                {
                    Texture t = (Texture)Resources.Load("Splash_11");
                    material.SetTexture("_SplashMap", t);
                }
                if (material.HasProperty("_FillTex"))
                {
                    Texture t = (Texture)Resources.Load("paper_diffuse");
                    material.SetTexture("_FillTex", t);
                }
            }

            animateTextures.Add(splash);
        }

        foreach (Transform child in go.transform)
        {
            animateTextures.AddRange(AnimateTextureOnObject(child.gameObject));
        }

        return animateTextures;
    }

    void CallbackActivate()
    {
        foreach (AnimateTexture animateTexture in splash)
        {
            animateTexture.PlayForward();
        }
    }

    public void Activate()
    {
        //Debug.Log(string.Format("Part {0} activated", gameObject.name));
        //Hashtable props = new Hashtable();
        //props.Clear();
        //props.Add("dummy", 1.0f);
        //props.Add("atEnd", new AtEndDelegate(CallbackActivate));
        //Ani.Mate.Stop(this);
        //Ani.Mate.By(this, delay, props);

        StartCoroutine(Animate.Lerp(0.0f, delay, (v, ev) => { if (ev == Animate.Event.End) CallbackActivate(); }, Animate.Linear));
    }

    public void Deactivate()
    {
        foreach (AnimateTexture animateTexture in splash)
        {
            animateTexture.PlayBackward();
        }
    }

    void CallbackPop()
    {
        // Strips
        if (!noStrips)
        {
            enabled = true;
            if (strips != null)
            {
                Destroy(strips);
            }
            strips = new GameObject("Strips");
            strips.transform.position = transform.position;
            strips.transform.localRotation = Quaternion.identity;

            GameObject bits = (GameObject)Instantiate(Resources.Load("PaperBits"));
            bits.transform.parent = strips.transform;
            bits.transform.localPosition = Vector3.zero;
            bits.transform.localRotation = Quaternion.identity;

            StripInfo[] infos = GetStripsInfos(0.5f);
            foreach (StripInfo info in infos)
            {
                int stripN = Mathf.FloorToInt(Random.value * 4);
                GameObject strip = (GameObject)Instantiate(Resources.Load(string.Format("PaperStrips/PaperStrip0{0}", 1 + stripN)));
                //GameObject strip = new GameObject("Strip");
                strip.transform.parent = strips.transform;
                strip.transform.position = info.Position;
                //strip.transform.localRotation = Quaternion.LookRotation(info.Normal, Vector3.up);

                Vector3 c = Vector3.Cross(transform.position - info.Position, info.Normal);
                strip.transform.rotation = Quaternion.LookRotation(c, Vector3.up) * Quaternion.AngleAxis(-90.0f + (-5.0f + Random.value * 10.0f), info.Normal);

                float scale = 1.0f + (-0.2f + Random.value * 0.4f);
                strip.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    void AnimatePop(Vector3 v, Animate.Event ev)
    {
        if (ev == Animate.Event.Start)
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.enabled = false;
        }
        transform.localScale = v;
        if (ev == Animate.Event.End)
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.enabled = true;
        }
    }

    public void Pop()
    {
        if (renderer != null)
            renderer.enabled = true;

        //Hashtable props = new Hashtable();
        //props.Clear();
        //props.Add("dummy", 1.0f);
        //props.Add("atEnd", new AtEndDelegate(CallbackPop));
        //Ani.Mate.Stop(this);
        //Ani.Mate.By(this, delay, props);

        StartCoroutine(Animate.Lerp(0.0f, delay, (v, ev) => { if (ev == Animate.Event.End) CallbackPop(); }, Animate.Linear));

        //props.Clear();
        //props.Add("localScale", startScale);
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.Out);
        //props.Add("delay", delay);
        //Ani.Mate.Stop(transform);
        //Ani.Mate.To(transform, 1, props);

        StartCoroutine(Animate.LerpTo(delay, 1.0f, transform.localScale, startScale, AnimatePop, Animate.QuadraticOut));
    }

    void CallbackUnpop()
    {
        if (renderer != null)
            renderer.enabled = false;
    }

    void AnimateUnPop(Vector3 v, Animate.Event ev)
    {
        transform.localScale = v;
        if (ev == Animate.Event.End)
            CallbackUnpop();
    }

    public void Unpop()
    {
        //Hashtable props = new Hashtable();
        //props.Add("localScale", Vector3.one * 0.01f);
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.Out);
        //props.Add("atEnd", new AtEndDelegate(CallbackUnpop));
        //Ani.Mate.Stop(transform);
        //Ani.Mate.To(transform, 1, props);

        StartCoroutine(Animate.LerpTo(0.0f, delay, transform.localScale, Vector3.one * 0.01f, AnimateUnPop, Animate.QuadraticOut));
    }

    StripInfo[] GetStripsInfos(float spacing)
    {
        float radiusStrips = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 0.3f;
        float perimeter = 2.0f * Mathf.PI * radiusStrips;
        int n = (int)(perimeter / spacing);
        float delta = 2 * Mathf.PI / n;

        List<StripInfo> deltas = new List<StripInfo>(n);

        for (int i = 0; i < n; ++i)
        {
            float angle = delta * i + (-1.0f + Random.value * 2.0f) * delta * 1.0f;
            Vector2 p = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radiusStrips;
            Vector3 v = new Vector3(transform.position.x + p.x, transform.position.y + size.y, transform.position.z + p.y);

            Ray ray = new Ray(v, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                StripInfo info = new StripInfo();
                info.Position = hit.point;
                info.Normal = hit.normal;
                deltas.Add(info);
            }
        }

        return deltas.ToArray();
    }

    void Update()
    {
        if (strips != null)
        {
            bool playing = true;
            foreach (Transform child in strips.transform)
            {
                if (child.animation != null)
                {
                    playing &= child.animation.isPlaying;
                }
            }

            if (!playing || strips.transform.childCount == 0)
            {
                Destroy(strips);
                strips = null;
                enabled = false;
            }
        }
    }
}
