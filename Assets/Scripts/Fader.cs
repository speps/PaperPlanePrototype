using System.Collections;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/CameraFade")]
public class Fader : MonoBehaviour
{

    public Shader shaderFade;
    private Material materialFade;

    GameObject cam2;
    Camera camComponent;

    public float fadeAccum = 0.0f;
    public float fadeDelay = 1.0f;
    
    public bool FadeActive
    { get { return fadeAccum > 0.0f; } }

    public AtEndDelegate OnFadeInEnd;
    public AtEndDelegate OnFadeOutEnd;

    void Start()
    {
        if (shaderFade == null)
        {
            shaderFade = Shader.Find("Custom/Fader");
        }
        cam2 = new GameObject("CameraFader");
        cam2.transform.parent = gameObject.transform;
        cam2.transform.localPosition = Vector3.zero;
        cam2.transform.localRotation = Quaternion.identity;
        camComponent = cam2.AddComponent<Camera>();
        cam2.active = false;
        camComponent.cullingMask = (1 << 8); //We choose to display object in tag 8 
        camComponent.clearFlags = CameraClearFlags.Nothing;

        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        if (shaderFade == null)
        {
            Debug.Log("Fade shader are not set up ! Disabling fade effect.");
            enabled = false;
        }
        else
        {
            if (!shaderFade.isSupported)
                enabled = false;
        }

    }

    protected Material material
    {
        get
        {
            if (materialFade == null)
            {
                materialFade = new Material(shaderFade);
                materialFade.hideFlags = HideFlags.HideAndDontSave;
            }
            return materialFade;
        }
    }

    protected void OnDisable()
    {
        if (materialFade)
            DestroyImmediate(materialFade);
    }

    public void FadeIn()
    {
        fadeAccum = 0.0f;

        //Hashtable props = new Hashtable();
        //props.Add("fadeAccum", 1.0f);
        //props.Add("easing", typeof(Ani.Easing.Linear));
        //props.Add("direction", Ani.Easing.InOut);
        //props.Add("atEnd", new AtEndDelegate(OnFadeInEnd));

        //Ani.Mate.Stop(this);
        //Ani.Mate.To(this, fadeDelay, props);

        StartCoroutine(Animate.Lerp(0.0f, fadeDelay, (v, ev) => { fadeAccum = v; if (ev == Animate.Event.End && OnFadeInEnd != null) OnFadeInEnd(); }, Animate.Linear));
    }

    public void FadeOut()
    {
        fadeAccum = 1.0f;

        //Hashtable props = new Hashtable();
        //props.Add("fadeAccum", 0.0f);
        //props.Add("easing", typeof(Ani.Easing.Linear));
        //props.Add("direction", Ani.Easing.InOut);
        //props.Add("atEnd", new AtEndDelegate(OnFadeOutEnd));

        //Ani.Mate.Stop(this);
        //Ani.Mate.To(this, fadeDelay, props);

        StartCoroutine(Animate.Lerp(0.0f, fadeDelay, (v, ev) => { fadeAccum = 1.0f - v; if (ev == Animate.Event.End && OnFadeOutEnd != null) OnFadeOutEnd(); }, Animate.Linear));
    }

    protected void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        material.SetFloat("_Fade", fadeAccum);
        Graphics.Blit(source, dest, material);
        GameObject currentCamera = (GameObject) GameObject.FindWithTag("CurrentCamera");
        if(currentCamera != null)camComponent.fieldOfView = currentCamera.camera.fieldOfView;

        if (FadeActive)
        {
            camComponent.targetTexture = dest;
            camComponent.Render();
        }
    }

}
