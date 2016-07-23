using System.Text.RegularExpressions;
using UnityEngine;
using System.Diagnostics;

[RequireComponent(typeof(AudioSource))]
public class Main : MonoBehaviour
{
    GameObject flightController;
    public FlightController FlightController
    { get { return flightController.GetComponent<FlightController>(); } }
    GameObject flightPOV;
    public GameObject FlightPOV
    { get { return flightPOV; } }
    public FlightCamera FlightCamera
    { get { return flightPOV.GetComponent<FlightCamera>(); } }

    GameObject menuController;
    public MenuController MenuController
    { get { return menuController.GetComponent<MenuController>(); } }
    GameObject menuCamera;
    public MenuCamera MenuCamera
    { get { return menuCamera.GetComponent<MenuCamera>(); } }

    GameObject sketchController;
    public SketchController SketchController
    { get { return sketchController.GetComponent<SketchController>(); } }
    GameObject sketchCamera;
    public SketchCamera SketchCamera
    { get { return sketchCamera.GetComponent<SketchCamera>(); } }

    GameObject gameObjectControls;
    Controls controls;
    public Controls Controls
    { get { return controls; } }

    public Fader Fader
    { get { return playerStart.GetComponent<Fader>(); } }

    GameObject achievement;
    public Achievement Achievement
    { get { return achievement.GetComponent<Achievement>(); } }

    GameObject playerStart;
    public GameObject PlayerStart
    { get { return playerStart; } }

    GameObject skyBox;

    PlayMode playMode = PlayMode.Menu;
    public PlayMode PlayMode
    {
        get { return playMode; }
    }

    public GameObject PreviousCamera;

    public bool stopSound = false;
    public bool playcue = false;
    public bool pause = false;
    public bool resume = false;

    int popupCount;
    int popupTotal;

    GameObject screenEffect;
    public GameObject ScreenEffect
    { get { return screenEffect != null ? screenEffect : screenEffect = GameObject.Find("ScreenEffect"); } }

    SoundManager _SoundManager;

    static Main instance;

    public static Main Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("Main").GetComponent<Main>();
            }
            return instance;
        }
    }

    void Start()
    {
        Screen.showCursor = false;

        print(Application.dataPath);

        ResetTerrain();

        playerStart = GameObject.FindWithTag("PlayerStart");
        playerStart.AddComponent(typeof(Fader));

        SoundManager.CreateSingleton(GetComponent<AudioSource>());
        _SoundManager = SoundManager.GetInstance();
        _SoundManager.LoadAllSounds();

        // Controls
        {
            gameObjectControls = new GameObject("Controls");
            controls = gameObjectControls.AddComponent<Controls>();
        }

#if UNITY_ANDROID
        ScreenEffect.active = false;
#endif

        // Flight
        {
            flightPOV = new GameObject("FlightPOV");

            // Place POV at PlayerStart
            flightPOV.transform.position = playerStart.transform.position + (playerStart.transform.rotation * (Vector3.up * -0.1f));
            flightPOV.transform.rotation = playerStart.transform.rotation;
            flightPOV.AddComponent<FlightCamera>().Reset(true);

            // Make camera parent of POV and offset it
            playerStart.transform.parent = flightPOV.transform;
            playerStart.transform.localPosition = new Vector3(0.0f, 0.1f, 0.0f);
            playerStart.transform.localRotation = Quaternion.identity;
            playerStart.name = "FlightCamera";

            flightController = new GameObject("FlightController");
            flightController.transform.position = flightPOV.transform.position + (flightPOV.transform.rotation * (Vector3.forward * FlightCamera.maxDistance));
            flightController.transform.rotation = flightPOV.transform.rotation;

            FlightController controller = flightController.AddComponent<FlightController>();
            controller.Build();

            Fader fader = playerStart.GetComponent<Fader>();
            fader.OnFadeInEnd = controller.OnFadeInEnd;
            fader.OnFadeOutEnd = controller.OnFadeOutEnd;
        }

        // Menu
        {
            menuCamera = new GameObject("MenuCamera");
            Camera camera = menuCamera.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 2000.0f;
            menuCamera.AddComponent<GUILayer>();
            menuCamera.AddComponent<MenuCamera>();

            menuController = new GameObject("MenuController");
            menuController.AddComponent<MenuController>().Build();
        }

        // Sketch
        {
            sketchCamera = new GameObject("SketchCamera");
            Camera camera = sketchCamera.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 2000.0f;
            sketchCamera.AddComponent<GUILayer>();
            sketchCamera.AddComponent<SketchCamera>();

            sketchController = new GameObject("SketchController");
            sketchController.AddComponent<SketchController>().Build();
        }

        // Achievement
        {
            achievement = GameObject.Find("Achievement");
        }

        // Check objects
        {
            popupCount = 0;
            popupTotal = 0;

            var gameObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));

            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                string name;
                if (HelperConstants.ExtractNoCollidersName(go.name, out name))
                {
                    MeshCollider[] colliders = go.GetComponentsInChildren<MeshCollider>();
                    for (int j = 0; j < colliders.Length; j++)
                    {
                        Destroy(colliders[j]);
                    }
                    go.name = name;
                }

                CheckObject(go.transform);
            }

            UnityEngine.Debug.Log(string.Format("PopUps : {0}", popupTotal));
        }

        // Vertex paint
        {
            var meshPainter = new GameObject("MeshPainter");
            meshPainter.AddComponent<MeshPainter>();
        }

        // Sounds
        {
            SoundComponent[] sounds = FindObjectsOfType(typeof(SoundComponent)) as SoundComponent[];
            foreach(SoundComponent sound in sounds)
            {
                sound.Register();
            }
        }

        Play(PlayMode.Menu);

        SetProgress(0.0f);
        //SetProgress((float)popupCount / popupTotal);
    }

    void Update()
    {
        if (stopSound)
        {
            _SoundManager.Stop();
            stopSound = false;
        }

        if (pause)
        {
            _SoundManager.Pause();
            pause = false;
        }


        if (resume)
        {
            _SoundManager.Resume();
            resume = false;
        }


        if (playcue)
        {
            uint soundHandle = _SoundManager.GetSoundHandle("pop_heavy");
            _SoundManager.PlayOneShot(soundHandle);
            playcue = false;
        }
    }

    public void ResetPlayMode()
    {
        FlightController.paused = true;
        flightPOV.SetActiveRecursively(false);

        menuCamera.SetActiveRecursively(false);
        menuController.SetActiveRecursively(false);

        sketchCamera.SetActiveRecursively(false);
        sketchController.SetActiveRecursively(false);
    }

    public void SetAudioListener(GameObject go)
    {
        _SoundManager.SetAudioListener(go);
    }

    public void Play(PlayMode playMode)
    {
        UnityEngine.Debug.Log(string.Format("MENU {0}", playMode));
        switch (playMode)
        {
            case PlayMode.Menu:
                {
                    PreviousCamera = GameObject.FindWithTag("CurrentCamera");

                    ResetPlayMode();

                    playerStart.tag = "Untagged";
                    sketchCamera.tag = "Untagged";
                    menuCamera.tag = "CurrentCamera";

                    menuCamera.SetActiveRecursively(true);
                    menuController.SetActiveRecursively(true);

                    SetAudioListener(menuCamera);

                    //menuController.GetComponent<MenuController>().Reset();
                }
                break;
            case PlayMode.Flight:
                {
                    PreviousCamera = GameObject.FindWithTag("CurrentCamera");

                    ResetPlayMode();

                    menuCamera.tag = "Untagged";
                    sketchCamera.tag = "Untagged";
                    playerStart.tag = "CurrentCamera";

                    FlightController.paused = false;
                    FlightController.Launch();
                    flightPOV.SetActiveRecursively(true);

                    flightPOV.GetComponent<FlightCamera>().Reset();
                    SetAudioListener(playerStart);
                }
                break;
            case PlayMode.Sketch:
                {
                    PreviousCamera = GameObject.FindWithTag("CurrentCamera");

                    ResetPlayMode();

                    playerStart.tag = "Untagged";
                    menuCamera.tag = "Untagged";
                    sketchCamera.tag = "CurrentCamera";

                    sketchCamera.SetActiveRecursively(true);
                    sketchController.SetActiveRecursively(true);

                    sketchController.GetComponent<SketchController>().Reset();

                    SetAudioListener(sketchCamera);
                }
                break;
        }

        this.playMode = playMode;
    }

    void ResetTerrain()
    {
        GameObject terrain = GameObject.FindGameObjectWithTag("Terrain");
        if (terrain == null)
        {
            UnityEngine.Debug.LogError("Terrain not found! Check the tag.");
            return;
        }
        MeshFilter meshFilter = (MeshFilter)terrain.GetComponent(typeof(MeshFilter));
        Mesh mesh = meshFilter.sharedMesh;

        Color[] colors = mesh.colors;
        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            colors[i] = Color.black;
        }
        mesh.colors = colors;
    }

    public void AddPopUp()
    {
        if (popupCount < popupTotal)
            popupCount += 1;
        SetProgress((float)popupCount / popupTotal);
    }

    public void RemovePopUp()
    {
        if (popupCount > 0)
            popupCount -= 1;
        SetProgress((float)popupCount / popupTotal);
    }

    void SetProgress(float alpha)
    {
        if (skyBox == null)
            skyBox = GameObject.FindGameObjectWithTag("Skybox");

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.Lerp(new Color(0.886f, 0.886f, 0.886f), new Color(0.603f, 0.505f, 0.69f), alpha);
        RenderSettings.fogDensity = Mathf.Lerp(0.015f, 0.002f, alpha);

        skyBox.renderer.material.SetFloat("_Blend", alpha);
    }

    void CheckObject(Transform node)
    {
        if (node.gameObject != null)
        {
            GameObject go = node.gameObject;

            string name;
            if (HelperConstants.ExtractPopUpName(go.name, out name))
            {
                if (CountMeshes(node) == 0)
                {
                    go.name = name;
                    return;
                }

                if (!go.tag.Equals("PopUp"))
                {
                    go.tag = "PopUp";
                }

                popupTotal += 1;

                ObjectPopUp popUp = (ObjectPopUp)go.GetComponent(typeof(ObjectPopUp));
                if (popUp == null)
                {
                    popUp = (ObjectPopUp)go.AddComponent(typeof(ObjectPopUp));
                    //popUp.Init();
                }
            }
        }
    }

    int CountMeshes(Transform node)
    {
        int n = 0;

        if (node.renderer != null)
            n += 1;

        foreach (Transform child in node.transform)
        {
            n += CountMeshes(child);
        }

        return n;
    }
}
