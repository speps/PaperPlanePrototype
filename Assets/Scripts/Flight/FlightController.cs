using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlightController : MonoBehaviour
{
    public enum CollisionType
    {
        Normal,
        Gameplay,
        Feedback
    }

    public enum GameplayType
    {
        None,
        NormalFlight,
        TubeVertical,
        TubeHorizontal,
        FlyThrough,
        FlySurface,
        LevelLimits
    }

    string debugText = "";
    bool initFirst = true;

    float timerAccum = 0.0f;
    float fixedTimeStep = 0.02f;
    float timerAlpha = 0.0f;

    public GameObject disturbance;
    float disturbanceDelay = 0.25f;
    float disturbanceTimer;
    float disturbanceTarget;
    private float disturbanceLevelLimit = 1.0f;
    Quaternion disturbanceQuaternion;

    float vibrationIntensity = 0.0f;
    float vibrationAngle = 0.0f;
    float vibrationAngleMax = 5.0f;
    Quaternion vibrationQuaterion;

    /* Trail */
    private GameObject leftTrail;
    private GameObject rightTrail;
    private int soundVolume = 0;

    /* Feedback */
    private float distanceFeedback = 9.0f;
    private SphereCollider feedbackCollider = null;

    /* Boost particles*/
    private GameObject particleTrailPrefab = null;
    private GameObject leftTrailParticle = null;
    private GameObject rightTrailParticle = null;

    /* Tube Gameplay */
    public float velocityMax = 5.0f;
    public float distanceMin;
    public bool doStoreEnter = true;
    public bool didWin = false;
    public float angleAccum = 0.0f;
    public float lastAngle;
    public PadTrigger pulledTrigger;

    /* Timer bell */
    public float timerBell = 0;
    public float timerBellTarget = 300;
    public bool bellPlayed = false;


    /* Fly Gameplay */
    public float distanceInTrigger = 0.0f;

    public enum PadTrigger
    {
        Left,
        Right,
        None
    }

    Vector3 gravity = new Vector3(0, -3, 0);

    Vector3 startPosition;
    Quaternion startRotation;

    Vector3 startImpulse = new Vector3(0, 0, 0.05f);

    float actionFactor = 0.0f;
    bool actionCombine = true;

    float combinedAccum = 0.0f;

    float boostTimer = 0.0f;
    bool boostFlicked = false;
    float boostingTimer = 0.0f;
    bool boosting = false;
    private bool boostFade = false;

    public bool boostAvailable
    {
        get { return boostTimer > 0.0f && !boostFlicked; }
    }

    float roll = 0.0f;
    //float rollBonus = 32;
    Quaternion rollRender;

    float damping = 0.99f;
    Vector3 acceleration;
    public Vector3 velocity;

    public Vector3 position;
    public Vector3 previousPosition;
    public Quaternion rotation;
    public Quaternion previousRotation;

    //Distance ground
    private float groundY;
    private float maxDistGround = 5.0f; //Max distance to ground for XACT
    private RaycastHit hitGround;
    private RaycastHit hitLevelLimit;

    float wingAngleMax = 20.0f;
    Quaternion wingLeftBase;
    public float wingLeftAngle = 0.0f;
    public float wingLeftAngleTarget = 0.0f;
    GameObject wingLeftDummy;
    Quaternion wingRightBase;
    public float wingRightAngle = 0.0f;
    public float wingRightAngleTarget = 0.0f;
    GameObject wingRightDummy;

    GameObject wingCollideLeft;
    Vector3 wingCollideLeftOriginalPosition;
    GameObject wingCollideRight;
    Vector3 wingCollideRightOriginalPosition;

    GameObject wingFlameLeft;
    GameObject wingFlameRight;

    bool maintainDistance = false;
    float distToMaintain = 0.0f;
    //float distToMaintainEnter = 0.0f;
    bool directionClockwise = true;
    bool perpendicularEntry = false;

    ObjectTriggerApproach approachTrigger = null;

    float lastForwardVelocity = 0.0f;

    float gameplayDelay = 1.0f;
    float gameplayAlpha = 0.0f;
    public float GameplayAlpha
    {
        get { return gameplayAlpha; }
        set
        {
            gameplayAlpha = Mathf.Clamp01(value);
            gameplayTimer = gameplayAlpha * gameplayDelay;
        }
    }
    float gameplayTimer = 0.0f;
    ObjectTriggerBase currentTrigger = null;
    public GameplayType CurrentGameplayType
    {
        get { return currentTrigger == null ? GameplayType.NormalFlight : currentTrigger.GameplayType; }
    }
    public ObjectTriggerBase CurrentTrigger
    {
        get { return currentTrigger; }
    }

    List<ObjectTriggerBase> triggers = new List<ObjectTriggerBase>();

    GameObject planeModel;
    public GameObject PlaneModel
    {
        get { return planeModel; }
    }

    GameObject shadow;

    public bool fadeOnInit = true;
    public bool launch = false;
    public bool launched = false;
    public bool paused = true;

    public bool flying
    {
        get { return rigidbody.isKinematic && launched; }
    }

    public float flyingTimer = 0.0f;

    void Start()
    {
        Init(false);
    }

    // Use this for initialization
    public void Build()
    {
        /*Tube varTube = GameObject.Find("varTube").GetComponent<Tube>();
        velocityMax = varTube.velocityMax;
        distanceMin = varTube.distanceMin;*/

        initFirst = true;

        /* Rigid Body */
        {
            gameObject.AddComponent(typeof(Rigidbody));
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.mass = 1.0f;
            rigidbody.drag = 2.0f;
            rigidbody.angularDrag = 1.0f;
        }

        /* Shadow */
        {
            shadow = new GameObject("FlightShadow");
            shadow.AddComponent(typeof(FlightShadow));
        }

        disturbance = new GameObject("FlightDisturbance");
        disturbance.transform.parent = transform;
        disturbance.transform.localPosition = Vector3.zero;
        disturbance.transform.localRotation = Quaternion.identity;

        GameObject planePrefab = (GameObject)Resources.Load("Plane");
        planeModel = (GameObject)GameObject.Instantiate(planePrefab);
        planeModel.name = "Plane";
        planeModel.transform.parent = disturbance.transform;
        planeModel.transform.localPosition = Vector3.zero;
        planeModel.transform.localRotation = Quaternion.identity;

        GameObject.Find("Avion_ref").layer = 8;

        particleTrailPrefab = (GameObject)Resources.Load("Particles/PlaneParticle");
        if (particleTrailPrefab == null) Debug.Log("Plane's article prefab null");

        /* Wings collide */
        {
            wingCollideLeft = GameObject.Find("Dummy_collide_left_ex");
            wingCollideLeftOriginalPosition = wingCollideLeft.transform.localPosition;
            wingCollideRight = GameObject.Find("Dummy_collide_right_ex");
            wingCollideRightOriginalPosition = wingCollideRight.transform.localPosition;
        }

        /* Wings flame */
        {
            GameObject wingLeft = GameObject.Find("Dummy_Aile_Left");
            GameObject wingRight = GameObject.Find("Dummy_Aile_Right");

            GameObject prefab = (GameObject)Resources.Load("FlamesBis/Flames");

            wingFlameLeft = (GameObject)Instantiate(prefab);
            //wingFlameLeft.animation["default"].speed = 2.0f;
            wingFlameLeft.transform.parent = wingLeft.transform;
            //wingFlameLeft.transform.localPosition = new Vector3(0.0234f, -0.0731f, -0.003f);
            //wingFlameLeft.transform.localRotation = Quaternion.Euler(275.0f, 181.74f, 0.0f);
            wingFlameLeft.transform.localPosition = new Vector3(0.0234f, -0.0731f, -0.003f);
            wingFlameLeft.transform.localRotation = Quaternion.Euler(355.0f, 1.74f, 180.0f);

            wingFlameRight = (GameObject)Instantiate(prefab);
            //wingFlameRight.animation["default"].speed = 2.0f;
            wingFlameRight.transform.parent = wingRight.transform;
            //wingFlameRight.transform.localPosition = new Vector3(-0.0234f, -0.0731f, -0.003f);
            //wingFlameRight.transform.localRotation = Quaternion.Euler(275.0f, -181.74f, 0.0f);
            wingFlameRight.transform.localPosition = new Vector3(-0.0234f, -0.0731f, -0.003f);
            wingFlameRight.transform.localRotation = Quaternion.Euler(355.0f, 358.26f, 180.0f);

            // Texture animation
            AnimateTexture animLeft = wingFlameLeft.GetComponentInChildren<Renderer>().gameObject.AddComponent<AnimateTexture>();
            //animLeft.nImagesX = 4;
            animLeft.nImagesX = 3;
            animLeft.nImagesY = 1;
            animLeft.loop = true;
            animLeft.frameRate = 10;
            animLeft.PlayForward();

            AnimateTexture animRight = wingFlameRight.GetComponentInChildren<Renderer>().gameObject.AddComponent<AnimateTexture>();
            //animRight.nImagesX = 4;
            animRight.nImagesX = 3;
            animRight.nImagesY = 1;
            animRight.loop = true;
            animRight.frameRate = 10;
            animRight.PlayForward();
        }

        /* Triggers */
        {
            GameObject triggers = new GameObject("Triggers");
            triggers.transform.parent = transform;
            triggers.transform.localPosition = Vector3.zero;
            triggers.transform.localRotation = Quaternion.identity;

            GameObject wingLeft = GameObject.Find("Dummy_Aile_Left");
            GameObject wingRight = GameObject.Find("Dummy_Aile_Right");

            GameObject leftBackTrigger = AddTrigger(wingLeft, "LeftBackTrigger", CollisionType.Normal, new Vector3(0.02f, -0.04f, 0.0f), Quaternion.Euler(90, 0, 0));
            /* Colliders */
            {
                BoxCollider collider = (BoxCollider)leftBackTrigger.AddComponent(typeof(BoxCollider));
                collider.isTrigger = true;
                collider.size = new Vector3(0.06f, 0.005f, 0.04f);
            }

            GameObject rightBackTrigger = AddTrigger(wingRight, "RightBackTrigger", CollisionType.Normal, new Vector3(-0.02f, -0.04f, 0.0f), Quaternion.Euler(90, 0, 0));
            /* Colliders */
            {
                BoxCollider collider = (BoxCollider)rightBackTrigger.AddComponent(typeof(BoxCollider));
                collider.isTrigger = true;
                collider.size = new Vector3(0.06f, 0.005f, 0.04f);
            }

            GameObject leftWingTrigger = AddTrigger(wingLeft, "LeftWingTrigger", CollisionType.Normal, new Vector3(0.015f, 0.0632f, 0.0f), Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 18, 0));
            /* Colliders */
            {
                BoxCollider collider = (BoxCollider)leftWingTrigger.AddComponent(typeof(BoxCollider));
                collider.isTrigger = true;
                collider.size = new Vector3(0.02f, 0.005f, 0.18f);
            }

            GameObject rightWingTrigger = AddTrigger(wingRight, "RightWingTrigger", CollisionType.Normal, new Vector3(-0.015f, 0.0632f, 0.0f), Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 342, 0));
            /* Colliders */
            {
                BoxCollider collider = (BoxCollider)rightWingTrigger.AddComponent(typeof(BoxCollider));
                collider.isTrigger = true;
                collider.size = new Vector3(0.02f, 0.005f, 0.18f);
            }

            GameObject middleTrigger = AddTrigger(triggers, "MiddleTrigger", CollisionType.Normal,
                                                  new Vector3(0.0f, 0.0f, -0.03f), Quaternion.identity);
            /* Colliders */
            {
                CapsuleCollider collider = (CapsuleCollider)middleTrigger.AddComponent(typeof(CapsuleCollider));
                collider.isTrigger = true;
                collider.radius = 0.01f;
                collider.height = 0.25f;
                collider.direction = 2;
            }

            GameObject frontTrigger = AddTrigger(triggers, "FrontTrigger", CollisionType.Normal,
                                                 new Vector3(0.0f, 0.0f, 0.12f), Quaternion.identity);
            /* Colliders */
            {
                CapsuleCollider collider = (CapsuleCollider)frontTrigger.AddComponent(typeof(CapsuleCollider));
                collider.isTrigger = true;
                collider.radius = 0.01f;
                collider.height = 0.05f;
                collider.direction = 2;
            }

            GameObject gameplayTrigger = AddTrigger(triggers, "GameplayTrigger", CollisionType.Gameplay, Vector3.zero,
                                                    Quaternion.identity);
            /* Colliders */
            {
                CapsuleCollider collider = (CapsuleCollider)gameplayTrigger.AddComponent(typeof(CapsuleCollider));
                collider.isTrigger = true;
                collider.radius = 0.05f;
                collider.height = 0.4f;
                collider.direction = 2;
            }

            /* Feedback Trigger */
            GameObject feedbackTrigger = AddTrigger(triggers, "feedbackTrigger", CollisionType.Feedback, Vector3.zero, Quaternion.identity);
            /* Colliders */
            {
                feedbackCollider = (SphereCollider)feedbackTrigger.AddComponent(typeof(SphereCollider));
                feedbackCollider.isTrigger = true;
                feedbackCollider.radius = distanceFeedback;
            }
        }

        /* Colliders */
        {
            GameObject colliders = new GameObject("Colliders");
            colliders.transform.parent = transform;
            colliders.transform.localPosition = Vector3.zero;
            colliders.transform.localRotation = Quaternion.identity;

            GameObject boxCollider = AddCollider(colliders, "BoxCollider", Vector3.zero, Quaternion.identity);
            /* Colliders */
            {
                CapsuleCollider collider = (CapsuleCollider)boxCollider.AddComponent(typeof(CapsuleCollider));
                collider.radius = 0.04f;
                collider.height = 0.3f;
                collider.direction = 2;
            }

            GameObject leftWingCollider = AddCollider(colliders, "LeftWingCollider", Vector3.zero, Quaternion.identity);
            /* Colliders */
            {
                SphereCollider collider = (SphereCollider)leftWingCollider.AddComponent(typeof(SphereCollider));
                collider.center = new Vector3(-0.0531f, 0.01f, -0.12f);
                collider.radius = 0.04f;
            }

            GameObject rightWingCollider = AddCollider(colliders, "RightWingCollider", Vector3.zero, Quaternion.identity);
            /* Colliders */
            {
                SphereCollider collider = (SphereCollider)rightWingCollider.AddComponent(typeof(SphereCollider));
                collider.center = new Vector3(0.0531f, 0.01f, -0.12f);
                collider.radius = 0.04f;
            }
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        /* SoundManager */
        //if (GameObject.FindWithTag("CurrentCamera") != null)
        //    maxDistGround = GameObject.FindWithTag("CurrentCamera").transform.position.y;

        hitGround = new RaycastHit();
        hitLevelLimit = new RaycastHit();

        Init(true);
    }

    GameObject AddTrigger(GameObject parent, string name, CollisionType type, Vector3 position, Quaternion rotation)
    {
        GameObject trigger = new GameObject(name);
        trigger.tag = "Trigger";

        CollisionPropagate collisionPropagate = (CollisionPropagate)trigger.AddComponent(typeof(CollisionPropagate));
        collisionPropagate.type = type;

        Rigidbody rb = (Rigidbody)trigger.AddComponent(typeof(Rigidbody));
        rb.isKinematic = true;

        trigger.transform.parent = parent.transform;
        trigger.transform.localPosition = position;
        trigger.transform.localRotation = rotation;

        return trigger;
    }

    GameObject AddCollider(GameObject parent, string name, Vector3 position, Quaternion rotation)
    {
        GameObject collider = new GameObject(name);
        collider.tag = "Trigger";
        // To avoid intercollision
        collider.AddComponent(typeof(CollisionPropagate));

        collider.transform.parent = parent.transform;
        collider.transform.localPosition = position;
        collider.transform.localRotation = rotation;

        return collider;
    }

    void Init()
    {
        Init(false);
    }

    void Init(bool fromMenu)
    {
        position = startPosition;
        previousPosition = position;
        transform.position = position;
        rotation = startRotation;
        previousRotation = rotation;
        transform.rotation = rotation;
        acceleration = Vector3.zero;
        velocity = transform.forward;

        disturbanceTimer = 0.0f;
        disturbanceTarget = 0.0f;
        disturbanceQuaternion = Quaternion.identity;

        roll = 0.0f;
        rollRender = Quaternion.identity;

        wingLeftDummy = GameObject.Find("Dummy_Aile_Left");
        wingLeftAngleTarget = 0.0f;
        if (initFirst)
        {
            wingLeftBase = wingLeftDummy.transform.localRotation;
        }

        wingRightDummy = GameObject.Find("Dummy_Aile_Right");
        wingRightAngleTarget = 0.0f;
        if (initFirst)
        {
            wingRightBase = wingRightDummy.transform.localRotation;
        }

        rigidbody.isKinematic = true;

        GameObject.Find("Avion_ref").renderer.enabled = true;
        planeModel.animation.Play("idle");

        currentTrigger = null;
        approachTrigger = null;

        maintainDistance = false;

        ApplyImpulse(rotation * startImpulse);

        Main.Instance.FlightCamera.Init();

        initFirst = false;
        DetachTrail();
        DetachParticle();

        rigidbody.centerOfMass = new Vector3(0.0f, 0.0f, -0.05f);

        GameObject currentCamera = GameObject.FindWithTag("CurrentCamera");
        if (currentCamera != null)
        {
            //Debug.Log(string.Format("InitFade {0}", fadeOnInit));

            Fader fader = currentCamera.GetComponent<Fader>();
            if (fader == null)
            {
                fader = Main.Instance.Fader;
            }
            if (fadeOnInit && fader != null && !paused)
            {
                fader.fadeDelay = GameData.Instance.Tweak.RespawnDelay;
                fader.FadeOut();
            }

            fadeOnInit = true;
        }

        launched = false;
        flyingTimer = 0.0f;

        // Init boost when respawning
        boostTimer = 0.0f;
        boostingTimer = 0.0f;
        boostFlicked = false;
        boostFade = false;
        boosting = false;

        wingFlameLeft.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        wingFlameRight.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

        /*wingFlameLeft.SetActiveRecursively(false);
        wingFlameRight.SetActiveRecursively(false);*/

        //SoundManager.Instance.OnControllerInit(); SOUND

        Main.Instance.FlightCamera.FadeOff();

        /* Feedback*/
        feedbackCollider.radius = 0.1f;
    }

    public void Launch()
    {
        Debug.Log("Launch!");
        planeModel.animation.Play("wings-corner");
        launched = true;
        launch = false;
        flyingTimer = 0.0f;
    }

    void TriggerSignal(ObjectTriggerBase trigger)
    {
        trigger.PlaySignal();
        //Debug.Log("PlaySignal: "+trigger.name);
    }

    void TriggerAchievement(AchievementTrigger trigger)
    {
        string text = trigger.achievementText.Trim();
        int score = trigger.achievementScore;

        if (string.IsNullOrEmpty(text) || score <= 0)
        {
            return;
        }

        ObjectPopUp popup;
        if (string.IsNullOrEmpty(trigger.condition))
        {
            Achievement.Instance.Show(text, score);
        }
        else if (Helper.IsPopped(trigger.condition, out popup))
        {
            Debug.Log(string.Format("Achievement {0}", popup.state));

            if (trigger.conditionTextured)
            {
                if (popup.state == ObjectPopUp.State.Activated)
                    Achievement.Instance.Show(text, score);
            }
            else
            {
                Achievement.Instance.Show(text, score);
            }
        }
    }

    public void OnFadeInEnd()
    {
        Debug.Log("FadeInEnd");
        fadeOnInit = true;
        Init();
    }

    public void OnFadeOutEnd()
    {
        Debug.Log("FadeOutEnd");
        if (!launched)
        {
            if (launch)
            {
                Launch();
            }
            else
            {
                Main.Instance.MenuController.SetState(MenuController.State.Flight);
                Main.Instance.MenuCamera.transform.position = Main.Instance.FlightCamera.transform.position;
                Main.Instance.MenuCamera.transform.rotation = Main.Instance.FlightCamera.transform.rotation;
                Main.Instance.MenuCamera.smoothTimer = 0.0f;
                Main.Instance.Play(PlayMode.Menu);
            }
        }
    }

    public void ReturnToMenu()
    {
        fadeOnInit = false;
        Init();
        Main.Instance.MenuController.SetState(MenuController.State.Flight);
        Main.Instance.MenuCamera.transform.position = Main.Instance.FlightCamera.transform.position;
        Main.Instance.MenuCamera.transform.rotation = Main.Instance.FlightCamera.transform.rotation;
        Main.Instance.Play(PlayMode.Menu);
    }

    public void FixedUpdate()
    {
        if (!paused)
        {
            /* Control */
            if (Input.GetKeyUp(KeyCode.X))
            {
                Time.timeScale *= 0.5f;
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                Time.timeScale *= 2.0f;
            }

            /*if (Input.GetKeyUp(KeyCode.O))
            {
                GameObject[] popUps = GameObject.FindGameObjectsWithTag("PopUp");
                //GameObject popUp = GameObject.Find("POP_vache");
                foreach (GameObject popUp in popUps)
                {
                    ObjectPopUp objectPopUp = (ObjectPopUp)popUp.GetComponent(typeof(ObjectPopUp));
                    objectPopUp.GoToPreviousState();
                }
            }*/

            if (Input.GetKeyUp(KeyCode.P))
            {
                GameObject[] popUps = GameObject.FindGameObjectsWithTag("PopUp");
                //Debug.Log(popUps.Length);
                //GameObject popUp = GameObject.Find("POP_vache");
                foreach (GameObject popUp in popUps)
                {
                    ObjectPopUp objectPopUp = (ObjectPopUp)popUp.GetComponent(typeof(ObjectPopUp));
                    objectPopUp.data.PopActivates = true;
                    objectPopUp.GoToNextState();
                }

                Achievement.Instance.Show("Hacked prototype", 10);
            }

            if (Input.GetKeyUp(KeyCode.H))
            {
                var avion = GameObject.Find("Avion_ref");
                avion.renderer.enabled = !avion.renderer.enabled;
            }

            if (Controls.Instance.Pause)
            {
                if (!Main.Instance.Fader.FadeActive)
                {
                    Main.Instance.MenuController.Pause();
                }
            }

            Main.Instance.MenuController.pauseTimer -= Time.deltaTime;

            if (Controls.Instance.LaunchPlane && !launched)
            {
                Launch();
            }

            if (Controls.Instance.Restart)
            {
                fadeOnInit = true;
                Init();
                Launch();
            }

            //Press 'M' to mute the sound
            if (Input.GetKeyUp(KeyCode.M))
            {
                soundVolume = (soundVolume + 1) % 2;
                //SoundManager.Instance.DefaultVolume = soundVolume; SOUND
                //SoundManager.Instance.MusicVolume = soundVolume; SOUND
            }

            Step();

            if (rigidbody.isKinematic && launched && (currentTrigger == null || currentTrigger.GameplayType == GameplayType.FlyThrough))
            {
                StepDisturbance();
                //StepVibration();
            }
        }

        if (!launched)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
        else
        {
            if (!paused)
            {
                if (rigidbody.isKinematic)
                {
                    transform.position = Vector3.Lerp(previousPosition, position, timerAlpha);
                    transform.rotation = Quaternion.Slerp(previousRotation, rotation, timerAlpha);
                }
            }
        }
    }

    void Step()
    {
        debugText = "";

        if (approachTrigger != null)
            debugText += string.Format("{0}", approachTrigger.gameObject.name);

        debugText += string.Format("\nTriggers queue : {0}", triggers.Count);
        debugText += string.Format("\nVelocity : {0}", velocity.magnitude);

        if (launched)
        {
            if (rigidbody.isKinematic)
            {
                if (currentTrigger == null)
                {
                    gameplayTimer -= Time.fixedDeltaTime;
                    GameplayNormalFlight();
                }
                else
                {
                    gameplayTimer += Time.fixedDeltaTime;
                }
                gameplayTimer = Mathf.Clamp(gameplayTimer, 0.0f, gameplayDelay);
                gameplayAlpha = gameplayTimer / gameplayDelay;

                flyingTimer += fixedTimeStep;

                /* Wings */
                {
                    float pitch = Vector3.Dot(velocity.normalized, Vector3.up);

                    //debugText += string.Format("\nPitch {0}", pitch);

                    wingLeftAngleTarget = pitch * -wingAngleMax + wingAngleMax * -roll / GameData.Instance.Tweak.RollMax;
                    wingRightAngleTarget = pitch * -wingAngleMax + wingAngleMax * +roll / GameData.Instance.Tweak.RollMax;
                }

                /* Boost */
                {
                    boostTimer -= fixedTimeStep;
                    boostTimer = Mathf.Max(0.0f, boostTimer);
                    boostingTimer -= fixedTimeStep;
                    boostingTimer = Mathf.Max(0.0f, boostingTimer);

                    debugText += string.Format("\nBoost : {0}", boostTimer);

                    if (boostTimer > 0.0f && rightTrailParticle == null && leftTrailParticle == null)
                    {
                        //AttachParticle();
                    }

                    if (boostingTimer > 0.0f)
                    {
                        vibrationIntensity = 2f;
                        wingFlameLeft.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        wingFlameRight.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                    else if (boostTimer > 0.0f)
                    {
                        vibrationIntensity = 4f;
                        wingFlameLeft.transform.localScale = new Vector3(0.4f, 0.4f, 0.2f + Random.value * 0.3f);
                        wingFlameRight.transform.localScale = new Vector3(0.4f, 0.4f, 0.2f + Random.value * 0.3f);
                    }
                    else
                    {
                        vibrationIntensity = 0.0f;
                        wingFlameLeft.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                        wingFlameRight.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                    }

                    if (boostTimer == 0.0f && rightTrailParticle != null && leftTrailParticle != null && boostingTimer == 0.0f)
                    {
                        //DetachParticle();
                    }

                    if (boostingTimer == 0.0f && boosting)
                    {
                        boosting = false;

                        /*wingFlameLeft.SetActiveRecursively(false);
                        wingFlameRight.SetActiveRecursively(false);*/
                    }

                    if (Controls.Instance.Boost && !boostFlicked && boostTimer > 0.0f)
                    {
                        boostFlicked = true;
                        boostTimer = 0.0f;
                        boostingTimer = GameData.Instance.Tweak.BoostingDelay;
                        //SoundManager.Instance.PlayCue("boost"); SOUND
                        boostFade = true;
                        boosting = true;
                    }


                    if (boostingTimer > 0.0f && currentTrigger == null)
                    {
                        Main.Instance.FlightCamera.FadeOn();
                        boostFade = false;
                    }

                    if (boostingTimer == 0.0f && !boostFade && currentTrigger == null)
                    {
                        Main.Instance.FlightCamera.FadeOff();
                    }

                    if (Controls.Instance.NoBoost && boostFlicked)
                    {
                        boostFlicked = false;
                    }

                    if (boostingTimer > 0.0f)
                    {
                        ApplyForce(transform.forward * GameData.Instance.Tweak.BoostingForce);
                    }


                }

                Approach();

                if (currentTrigger != null)
                {
                    if (currentTrigger.GameplayType == GameplayType.TubeVertical)
                    {
                        GameplayTube();
                    }
                    if (currentTrigger.GameplayType == GameplayType.TubeHorizontal)
                    {
                        GameplayTubeH();
                    }
                    if (currentTrigger.GameplayType == GameplayType.FlyThrough)
                    {
                        GameplayFlyThrough();
                    }
                    if (currentTrigger.GameplayType == GameplayType.FlySurface)
                    {
                        GameplayFlySurface();
                    }
                    if (currentTrigger.GameplayType == GameplayType.LevelLimits)
                    {
                        GamePlayLevelLimits();
                    }
                }
            }
            else
            {
                wingLeftAngleTarget = 0.0f;
                wingRightAngleTarget = 0.0f;

                Vector3 direction = transform.rotation * Vector3.forward;
                Vector3 up = transform.rotation * Vector3.up;
                Vector3 physicsVelocity = rigidbody.GetRelativePointVelocity(Vector3.zero);
                float forwardVelocity = Vector3.Project(physicsVelocity, direction).magnitude;

                //debugText += string.Format("Velocity {0} Last {1}", forwardVelocity, lastForwardVelocity);

                //rigidbody.MoveRotation(Quaternion.LookRotation(physicsVelocity));
                //rigidbody.AddForce(gravity);
                rigidbody.AddForceAtPosition(gravity, transform.position);
                rigidbody.AddRelativeForce(Vector3.forward * lastForwardVelocity * 2.0f);
                //rigidbody.AddRelativeForce(Vector3.up * lastForwardVelocity * 2.0f);
                //rigidbody.AddRelativeForce(Vector3.up * forwardVelocity * 2.0f);
                rigidbody.AddForceAtPosition(up * forwardVelocity * 1.0f, transform.position);

                lastForwardVelocity -= lastForwardVelocity * 0.005f;

                rigidbody.AddRelativeTorque(0.0f, 0.0f, (Random.value - 0.5f) * 2.0f);
            }
        }

        /* Global transformations */
        {
            wingLeftAngleTarget = Mathf.Clamp(wingLeftAngleTarget, -wingAngleMax, wingAngleMax);
            wingRightAngleTarget = Mathf.Clamp(wingRightAngleTarget, -wingAngleMax, wingAngleMax);

            wingLeftAngle += (wingLeftAngleTarget - wingLeftAngle) * 0.1f;
            wingRightAngle += (wingRightAngleTarget - wingRightAngle) * 0.1f;

            wingLeftDummy.transform.localRotation = wingLeftBase * Quaternion.Euler(0.0f, -wingLeftAngle, 0.0f);
            wingRightDummy.transform.localRotation = wingRightBase * Quaternion.Euler(0.0f, +wingRightAngle, 0.0f);

            wingCollideLeft.transform.localPosition += (wingCollideLeftOriginalPosition - wingCollideLeft.transform.localPosition) * 0.1f;
            wingCollideRight.transform.localPosition += (wingCollideRightOriginalPosition - wingCollideRight.transform.localPosition) * 0.1f;
        }

        /* Sound Variables */
        {
            if (Physics.Raycast(transform.position, -Vector3.up, out hitGround, 100.0f, 1 << 9))
            {
                groundY = Mathf.Clamp01(hitGround.distance / maxDistGround);
                //Debug.Log("[Vincent] distanceGround: " + groundY);
                //Debug.Log(hitGround.collider.name);
                //SoundManager.Instance.WindAltitude = groundY; SOUND
                timerBell += Time.fixedDeltaTime;
                if (timerBell > timerBellTarget && !bellPlayed)
                {
                    bellPlayed = true;
                    OnPlayBells();
                }
            }
        }

        /* Level Limits */
        {
            Vector3 relPosOnMap = Vector3.zero;
            GameObject mapLimitObj = GameObject.Find("MapLimit");
            if (mapLimitObj != null)
            {
                MapLimit mapLimit = mapLimitObj.GetComponent<MapLimit>();
                if (mapLimit != null)
                {
                    relPosOnMap = mapLimit.GetRelativePos(transform.position);
                }
            }

            AckLevelLimit(relPosOnMap);
        }

        /* Feedback variable */
        {

            feedbackCollider.radius += Time.fixedDeltaTime * distanceFeedback;
            feedbackCollider.radius = Mathf.Clamp(feedbackCollider.radius, 0, distanceFeedback);

        }

        /* Debug Text */
        {
            GameObject debugTextObject = GameObject.Find("Debug");
            if (debugTextObject != null)
            {
                debugTextObject.guiText.text = debugText;
            }
        }

        //SoundManager.Instance.SetGlobalVariable("PlaneSpeed", velocity.magnitude); SOUND

    }

    void AckLevelLimit(Vector3 mapPosition)
    {
        //disturbanceLevelLimit = 10.0f;
        //disturbanceDelay = 0.01f;
        //Debug.Log(string.Format("mapPos X: {0}, Y: {1}", mapPosition.x, mapPosition.z));
        /*disturbanceLevelLimit = Mathf.Max(10*Mathf.Abs(mapPosition.x)*2f, 10*Mathf.Abs(mapPosition.z)*2f);
        disturbanceDelay = Mathf.Min((1-Mathf.Abs(mapPosition.x))*0.7f, (1-Mathf.Abs(mapPosition.z))*0.7f);
        disturbanceLevelLimit = Mathf.Clamp(disturbanceLevelLimit, 1, 10);
        disturbanceDelay = Mathf.Clamp(disturbanceDelay, 0.01f, 0.25f);*/
        //Debug.Log(string.Format("disturbance: {0},distubanceDelay: {1}", disturbanceLevelLimit, disturbanceDelay));
    }

    void StepDisturbance()
    {
        disturbanceTimer -= Time.deltaTime;
        if (disturbanceTimer <= 0.0f)
        {
            disturbanceTarget += (Random.value * 2.0f - 1.0f) * 5.0f;
            disturbanceTarget = Mathf.Clamp(disturbanceTarget, -5.0f, 5.0f);
            disturbanceTimer = disturbanceDelay + (Random.value * 2.0f - 1.0f) * disturbanceDelay * 0.25f;
        }
        Vector3 direction = rotation * Vector3.forward;
        float forwardVelocity = Vector3.Project(velocity, direction).magnitude;
        disturbanceQuaternion = Quaternion.Slerp(disturbanceQuaternion, Quaternion.AngleAxis(disturbanceTarget, Vector3.forward), Mathf.Lerp(0.02f, 0.06f, forwardVelocity / 0.05f) * disturbanceLevelLimit);
        rollRender = Quaternion.Slerp(rollRender, Quaternion.identity, 10.0f * Time.deltaTime);
        disturbance.transform.localRotation = rollRender * disturbanceQuaternion * vibrationQuaterion;
    }

    void StepVibration()
    {
        vibrationAngle += Random.Range(-vibrationIntensity, +vibrationIntensity);
        float sign = Mathf.Sign(vibrationAngle);
        vibrationAngle = sign * Mathf.Min(Mathf.Abs(vibrationAngle), vibrationAngleMax);
        vibrationQuaterion = Quaternion.AngleAxis(vibrationAngle, Vector3.forward);
    }

    void Approach()
    {
        if (approachTrigger == null)
            return;

        Vector3 directionFlight = transform.forward;
        Vector3 directionTrigger = approachTrigger.transform.forward * (approachTrigger.gameObject.name.Equals("ApproachEnter") ? +1 : -1);
        float dot = Vector3.Dot(directionFlight, directionTrigger);

        if (dot > 0.0f)
        {
            Vector3 proj = Helper.ProjectOntoLine(approachTrigger.transform.position, approachTrigger.transform.position + directionTrigger, transform.position);
            Vector3 dirAxis = proj - transform.position;

            Vector3 dirAxisLocal = approachTrigger.transform.InverseTransformPoint(proj) - approachTrigger.transform.InverseTransformPoint(transform.position);

            debugText += string.Format("\nDistance {0} Local {1}", dirAxis.magnitude, dirAxisLocal.magnitude);

            ApplyForce(dirAxis.normalized * dirAxisLocal.magnitude * GameData.Instance.Tweak.ApproachForce * Mathf.Max(0.0f, approachTrigger.parent.approachForceRatio));
            //ApplyForce(directionTrigger * dirAxisLocal.magnitude * 2.0f);
        }
    }

    void InitGameplay(GameplayType gameplayType)
    {
        GameObject pov = GameObject.Find("FlightPOV");
        FlightCamera cam = (FlightCamera)pov.GetComponent(typeof(FlightCamera));
        cam.InitCamera();

        if (gameplayType == GameplayType.FlyThrough || gameplayType == GameplayType.FlySurface)
        {
            distanceInTrigger = 0.0f;
        }

        if (gameplayType == GameplayType.FlyThrough)
        {
            Main.Instance.FlightCamera.FadeOn();
        }
    }

    void ExitGameplay(GameplayType gameplayType)
    {
        switch (gameplayType)
        {
            case GameplayType.TubeVertical:
            case GameplayType.TubeHorizontal:
                /* Compute roll */
                {
                    Vector3 direction = rotation * Vector3.forward;

                    Quaternion qCurrent = rotation;
                    Quaternion qTarget = Quaternion.AngleAxis(roll, direction) * Quaternion.LookRotation(velocity, Vector3.up);

                    Quaternion delta = Helper.Delta(qTarget, qCurrent);

                    rollRender = delta;
                    rotation = qTarget;
                }
                pulledTrigger = PadTrigger.None;

                break;
            case GameplayType.FlyThrough:
                Main.Instance.FlightCamera.FadeOff();
                break;
            case GameplayType.FlySurface:
                /* Activate object */
                {
                    //Debug.Log("ACTIVATE " + rigidbody.isKinematic);
                    if (rigidbody.isKinematic)
                    {
                        float minimumDistance = 0.0f;
                        if (currentTrigger is ObjectTriggerFlyThrough)
                        {
                            minimumDistance = (currentTrigger as ObjectTriggerFlyThrough).minimumDistance;
                        }
                        if (currentTrigger is ObjectTriggerFlySurface)
                        {
                            minimumDistance = (currentTrigger as ObjectTriggerFlySurface).minimumDistance;
                        }

                        //Debug.Log(string.Format("Distance {0} sur {1} requis", distanceInTrigger, minimumDistance));

                        if (distanceInTrigger >= minimumDistance)
                        {
                            TriggerSuccess(currentTrigger);
                        }
                        else
                        {
                            TriggerFail(currentTrigger);
                        }
                    }
                    else
                    {
                        TriggerFail(currentTrigger);
                    }
                }
                break;
        }
    }

    void GameplayNormalFlight()
    {
        GameplayNormalFlightTriggersHalfPull();
    }

    void GameplayNormalFlightTriggersHalfPull()
    {
        Vector3 direction = rotation * Vector3.forward;
        Vector3 up = rotation * Vector3.up;

        float forwardVelocity = Vector3.Project(velocity, direction).magnitude;

        ApplyForce(gravity);

        float trigLeft = Controls.Instance.TriggerLeft;
        float trigRight = Controls.Instance.TriggerRight;

        if (Mathf.Abs(trigLeft - trigRight) < GameData.Instance.Tweak.TriggersThreshold)
        {
            trigLeft = trigRight = (trigLeft + trigRight) * 0.5f;
        }

        float horizontalAxis = (trigLeft - 0.5f) - (trigRight - 0.5f);

        debugText += string.Format("\nAxis: {0}", horizontalAxis);

        horizontalAxis = Mathf.Clamp(horizontalAxis, -1.0f, 1.0f);

        float targetRoll = horizontalAxis * GameData.Instance.Tweak.RollMax;
        roll += (targetRoll - roll) * (0.1f + actionFactor * 0.1f);

        float combinedTriggers = ((trigLeft - 0.5f) + (trigRight - 0.5f)) + Mathf.Abs(horizontalAxis) * (actionCombine ? 1.0f : (1.0f - actionFactor));

        combinedAccum += fixedTimeStep;
        combinedAccum *= Mathf.Lerp(GameData.Instance.Tweak.CombineReleasedFactor, GameData.Instance.Tweak.CombinePressedFactor, Controls.Instance.TriggerLeft * Controls.Instance.TriggerRight);

        float pitch = Vector3.Dot(velocity.normalized, Vector3.up);

        debugText += string.Format("\nCombined: {0} Accum {1}", combinedTriggers, combinedAccum);

        ApplyForce(direction * Mathf.Lerp(GameData.Instance.Tweak.DirectionForceMin, GameData.Instance.Tweak.DirectionForceMax, trigLeft * trigRight));
        ApplyForce(up
            * Mathf.Clamp(forwardVelocity, GameData.Instance.Tweak.LiftVelocityMin, GameData.Instance.Tweak.LiftVelocityMax)
            * Mathf.Lerp(1.0f, 0.0f, combinedAccum / GameData.Instance.Tweak.CombineMaximumTime)
            * Mathf.Lerp(GameData.Instance.Tweak.LiftForceMin + (GameData.Instance.Tweak.LiftForceMinAction * actionFactor), GameData.Instance.Tweak.LiftForceMax + (GameData.Instance.Tweak.LiftForceMaxAction * actionFactor), combinedTriggers)
        );

        damping = 0.99f;
        UpdatePosition(Time.fixedDeltaTime);

        debugText += string.Format("\nPitch {0}", pitch);
        Vector3 lookUp = Vector3.up;
        if (pitch > 0.9f)
        {
            lookUp = up;
        }

        previousRotation = rotation;
        rotation = Quaternion.LookRotation(velocity, lookUp) * Quaternion.AngleAxis(roll, Vector3.forward);
    }

    void GamePlayLevelLimits()
    {
        //Vector3 direction = rotation * Vector3.forward;
        damping = 0.99f;
        UpdatePosition(Time.fixedDeltaTime);
        previousRotation = rotation;
        rotation = Quaternion.LookRotation(velocity, Vector3.up) * Quaternion.AngleAxis(roll, Vector3.forward);

    }

    PadTrigger GetPadTrigger()
    {
        float leftTrigger = Mathf.Abs(Controls.Instance.TriggerLeft);
        float rightTrigger = Mathf.Abs(Controls.Instance.TriggerRight);

        if (leftTrigger > rightTrigger)
        {
            if (leftTrigger > GameData.Instance.Tweak.PadTresholdTubeH)
            {
                //Debug.Log("GPHZ: Left");
                return PadTrigger.Left;
            }

        }
        else
        {
            if (rightTrigger > GameData.Instance.Tweak.PadTresholdTubeH)
            {
                //Debug.Log("GPHZ: Right");
                return PadTrigger.Right;

            }
        }

        if (leftTrigger + rightTrigger == 0)
        {
            //Debug.Log("GPHZ: None");
            return PadTrigger.None;
        }
        return PadTrigger.None;
    }


    void GameplayTubeH()
    {
        //ObjectTriggerTube trigger = currentTrigger as ObjectTriggerTube;


        //Check the fist pulled trigger
        if (pulledTrigger == PadTrigger.None)
        {
            pulledTrigger = GetPadTrigger();
        }







        // Trigger related
        ObjectTriggerTube trigger = (ObjectTriggerTube)currentTrigger;
        GameObject go = currentTrigger.gameObject;
        Vector3 directionTrigger = go.transform.TransformDirection(trigger.Direction);
        Vector3 projPosition = trigger.GetPosition(position);
        Vector3 projOnVelocity = Helper.ProjectOntoLine(previousPosition, position, projPosition);
        Vector3 reprojVelocity = trigger.GetPosition(projOnVelocity);
        Vector3 dirCenter = projPosition - position;
        Vector3 realDirCenter = reprojVelocity - position;

        #region Detect Win

        Vector3 flightPos = (position - trigger.GetPosition(position)).normalized;
        Vector3 triggerUp = trigger.Direction;
        Vector3 triggerRight = (Vector3.Cross(flightPos, triggerUp)).normalized;
        Vector3 flightDirection = (rotation * Vector3.forward).normalized;
        float tubeClockwise = Vector3.Dot(triggerRight, flightDirection);

        if (doStoreEnter)
        {
            doStoreEnter = false;
            angleAccum = 0.0f;
            didWin = false;

        }
        else
        {
            //Debug.Log(trigger.winLaps);
            Vector3 flightPoint = (position - trigger.GetPosition(position)).normalized;
            Vector3 flightPrevPoint = (previousPosition - trigger.GetPosition(previousPosition)).normalized;
            float deltaAngle = Vector3.Angle(flightPrevPoint, flightPoint);
            if (tubeClockwise > 0) angleAccum += deltaAngle;
            if (tubeClockwise < 0) angleAccum -= deltaAngle;

            //SoundManager.Instance.TriggerActivation = angleAccum / (trigger.winLaps * 360f); SOUND
            //Debug.Log(Mathf.Abs(angleAccum / 360f));

            if (Mathf.Abs(angleAccum / 360.0f) > trigger.winLaps && !didWin)
            {
                Debug.Log("You win mother fucker");
                didWin = true;
                TriggerSuccess(trigger);
            }
        }

        #endregion

        if (pulledTrigger != PadTrigger.None && pulledTrigger == GetPadTrigger())
        {
            /* Centripete */
            if (maintainDistance)
            {
                //Debug.Log(String.Format("roll {0}", roll));

                Vector3 f = Vector3.Cross(dirCenter, directionTrigger * (directionClockwise ? 1.0f : -1.0f));

                float v = Vector3.Project(velocity, f).magnitude;
                Vector3 centripete = dirCenter.normalized * v * v / distToMaintain;
                ApplyForce(centripete);
            }


            damping = 1.0f;
            UpdatePosition(Time.fixedDeltaTime);

            float targetRoll = 0.0f;
            roll += (targetRoll - roll) * 0.1f;

            /* Detect turn */
            {
                Vector3 deltaDetect = projOnVelocity - position;
                float distDetect = deltaDetect.magnitude;

                if (distDetect < 0.1f && !maintainDistance)
                {
                    maintainDistance = true;
                    distToMaintain = realDirCenter.magnitude;

                    /* Detect direction */
                    {
                        Vector3 normal = Vector3.Cross(dirCenter, directionTrigger);
                        float dot = Vector3.Dot(projOnVelocity - projPosition, normal);
                        directionClockwise = dot > 0.0f;
                    }

                    /* Detect perpendicular */
                    {
                        if (trigger.tubeType == Helper.TubeType.Horizontal)
                        {
                            float angle = Vector3.Angle(velocity, directionTrigger);
                            perpendicularEntry = angle < 120.0f;
                        }
                        else
                        {
                            perpendicularEntry = false;
                        }
                    }

                    /* Set correct velocity */
                    {
                        Vector3 dirCentripete = Vector3.Cross(realDirCenter, directionTrigger * (directionClockwise ? 1.0f : -1.0f));

                        Vector3 velVertical = Vector3.Project(velocity, directionTrigger);
                        Vector3 velForward = Vector3.Project(velocity, dirCentripete);
                        if (velForward.magnitude > velocityMax) velForward = velForward.normalized * velocityMax;

                        float vVertical = Mathf.Max(0.25f, velVertical.magnitude);
                        velVertical = velVertical.normalized * vVertical;

                        previousPosition = position - (velForward + velVertical) * Time.fixedDeltaTime;
                    }

                    /* Set correct orientation */
                    {
                        Quaternion qCurrent = rotation;

                        Vector3 lookUpTemp = Vector3.up;
                        if (maintainDistance)
                        {
                            if (trigger.tubeType == Helper.TubeType.Horizontal)
                            {
                                lookUpTemp = perpendicularEntry ? dirCenter : Vector3.up;
                            }
                            else if (trigger.tubeType == Helper.TubeType.Vertical)
                            {
                                lookUpTemp = Vector3.up;
                            }
                        }
                        Quaternion qTarget = Quaternion.LookRotation(velocity, lookUpTemp) * Quaternion.AngleAxis(roll, Vector3.forward);

                        Quaternion delta = Helper.Delta(qTarget, qCurrent);
                        rollRender = delta;
                        rotation = qTarget;
                    }
                }
            }

            rollRender = Quaternion.Slerp(rollRender, Quaternion.identity, 0.1f);
            disturbance.transform.localRotation = rollRender * disturbanceQuaternion;

            previousRotation = rotation;
            Vector3 lookUp = Vector3.up;
            if (maintainDistance)
            {
                if (trigger.tubeType == Helper.TubeType.Horizontal)
                {
                    lookUp = perpendicularEntry ? dirCenter : Vector3.up;
                }
                else if (trigger.tubeType == Helper.TubeType.Vertical)
                {
                    lookUp = Vector3.up;
                }
            }
            rotation = Quaternion.LookRotation(velocity, lookUp) * Quaternion.AngleAxis(roll, Vector3.forward);
            //SoundManager.Instance.TriggerActivation = angleAccum / (trigger.winLaps * 360f); SOUND
        }
        else
        {
            //GameplayTube();
            GameplayNormalFlight();
        }

    }

    void GameplayTube()
    {
        ObjectTriggerTube trigger = currentTrigger as ObjectTriggerTube;
        //Vector3 refPoint = trigger.GetPosition(position) + trigger.transform.forward;

        // Are we clockwise
        Vector3 flightPos = (position - trigger.GetPosition(position)).normalized;
        Vector3 triggerUp = trigger.Direction;
        Vector3 triggerRight = (Vector3.Cross(flightPos, triggerUp)).normalized;
        Vector3 flightDirection = (rotation * Vector3.forward).normalized;
        float tubeClockwise = Vector3.Dot(triggerRight, flightDirection);

        // Calculate angle beetween tube's forward and relative flight vector position
        if (doStoreEnter)
        {
            doStoreEnter = false;
            angleAccum = 0.0f;
            didWin = false;

        }
        else
        {
            Vector3 flightPoint = (position - trigger.GetPosition(position)).normalized;
            Vector3 flightPrevPoint = (previousPosition - trigger.GetPosition(previousPosition)).normalized;
            float deltaAngle = Vector3.Angle(flightPrevPoint, flightPoint);
            if (tubeClockwise > 0) angleAccum += deltaAngle;
            if (tubeClockwise < 0) angleAccum -= deltaAngle;

            //SoundManager.Instance.TriggerActivation = angleAccum / (trigger.winLaps * 360f); SOUND
            //Debug.Log(angleAccum/(trigger.winLaps*360f));

            if (Mathf.Abs(angleAccum / 360.0f) > trigger.winLaps && !didWin)
            {
                Debug.Log("You win mother fucker");
                didWin = true;
                TriggerSuccess(trigger);
            }
        }

        if (currentTrigger.GameplayType == GameplayType.TubeVertical)
        {

        }


        //// flightControl
        GameplayNormalFlight();
    }

    void TriggerSuccess(ObjectTriggerBase trigger)
    {
        trigger.Activate();

        //SoundManager.Instance.PlayCue("trigger_ok"); SOUND

        boostTimer = GameData.Instance.Tweak.BoostDelay;
    }

    void TriggerFail(ObjectTriggerBase trigger)
    {
        trigger.Deactivate();
    }

    void GameplayFlyThrough()
    {

        if (doStoreEnter)
        {
            doStoreEnter = false;
            //angleAccum = 0.0f;
            didWin = false;

        }
        //Vector3 direction = rotation*Vector3.forward;
        //Vector3 up = rotation*Vector3.up;

        //float forwardVelocity = Vector3.Project(velocity, direction).magnitude;

        //ApplyForce(velocity);
        //ApplyForce(direction * Mathf.Lerp(0.0f, 4.0f, lift));
        //ApplyForce(up * -state.ThumbSticks.Left.Y * (Mathf.Lerp(1.0f, 2.0f, Mathf.Clamp(forwardVelocity, 0.0f, 4.0f) / 4.0f)));

        distanceInTrigger += (position - previousPosition).magnitude;
        ObjectTriggerFlyThrough trigger = (ObjectTriggerFlyThrough)currentTrigger;
        float distance = 0;
        if (trigger.minimumDistance == 0)
        {
            //SoundManager.Instance.TriggerActivation = 1; SOUND
        }
        else
        {
            distance = Mathf.Clamp01(distanceInTrigger / trigger.minimumDistance);
            //SoundManager.Instance.TriggerActivation = distance; SOUND
        }

        if (distanceInTrigger > trigger.minimumDistance && !didWin)
        {
            didWin = true;
            TriggerSuccess(trigger);
        }

        GameplayNormalFlight();

    }

    void GameplayFlySurface()
    {
        ObjectTriggerFlySurface trigger = (ObjectTriggerFlySurface)currentTrigger;

        float prevActionFactor = actionFactor;
        bool prevActionCombine = actionCombine;

        actionFactor = GameData.Instance.Tweak.ActionFactorFlySurface * gameplayAlpha;
        actionCombine = trigger.turnAlsoLifts;

        distanceInTrigger += (position - previousPosition).magnitude;

        GameplayNormalFlight();

        actionFactor = prevActionFactor;
        actionCombine = prevActionCombine;
    }

    void OnPlayBells()
    {
        Debug.Log("miawou Bell");
        GameObject bell = (GameObject)GameObject.Find("Clocher");
        if (bell != null)
        {
            //SoundManager.Instance.PlayInstance("clocher", bell.transform); SOUND
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.yellow;


        /*if (currentTrigger is ObjectTriggerFlySurface)
        {
            ObjectTriggerFlySurface trigger = currentTrigger as ObjectTriggerFlySurface;

            Gizmos.color = Color.white;

            Vector3 dir = trigger.Direction;
            Ray ray = new Ray(transform.position, trigger.transform.TransformDirection(dir));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Gizmos.DrawLine(ray.origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);

                Vector3 local = trigger.transform.InverseTransformPoint(transform.position);
                float t;
                Vector3 projLocal = Helper.ProjectOntoLine(dir * 0.5f, dir * -0.5f, local, out t);
                //local += -dir.normalized;
                Vector3 global = trigger.transform.TransformPoint(projLocal);
                Gizmos.DrawSphere(global, 0.1f);
            }
        }*/

        /* Gizmos.DrawSphere(position, 0.01f);
         Ray ray = new Ray(position, velocity);
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit))
         {
             Gizmos.DrawLine(position, hit.point);
             Gizmos.color = Color.blue;
             Gizmos.DrawLine(hit.point, hit.point + hit.normal);
         }

         if (currentTrigger is ObjectTriggerTube)
         {
             Gizmos.DrawLine(position, position + velocity.normalized * 0.5f);
             Gizmos.color = Color.yellow;
             Gizmos.DrawLine(currentTrigger.gameObject.transform.position, currentTrigger.gameObject.transform.position+tmp);
             //Gizmos.DrawLine(currentTrigger.gameObject.transform.position, currentTrigger.gameObject.transform.position+tmp2);
         }

         if(currentTrigger is ObjectTriggerFlyThrough)
         {
             ObjectTriggerFlyThrough trigger = currentTrigger as ObjectTriggerFlyThrough;
             Gizmos.color = Color.red;
             Gizmos.DrawLine(gameObject.transform.position, trigger.transform.position - gameObject.transform.position);
             Gizmos.color = Color.white;
             //trigger.GetPosition()

         }*/
    }

    void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }

    void ApplyImpulse(Vector3 impulse)
    {
        previousPosition -= impulse;
    }

    void UpdatePosition(float timeStep)
    {
        UpdatePosition(timeStep, false);
    }

    void UpdatePosition(float timeStep, bool force)
    {
        if (!launched && !force)
            return;

        float sqTimeStep = timeStep * timeStep;

        Vector3 delta = damping * (position - previousPosition) + acceleration * sqTimeStep;

        previousPosition = position;

        position += delta;

        velocity = delta / timeStep;
        acceleration = Vector3.zero;
    }

    public void TriggerEnter(GameObject go, Collider other, CollisionType type)
    {
        if (other != null)
        {
            CollisionPropagate otherCollision = (CollisionPropagate)other.gameObject.GetComponent(typeof(CollisionPropagate));
            if (otherCollision != null)
            {
                return;
            }

            ObjectTriggerBase trigger = (ObjectTriggerBase)other.gameObject.GetComponent(typeof(ObjectTriggerBase));
            if (trigger != null)
            {
                if (type == CollisionType.Gameplay)
                {
                    GameplayStart(trigger);
                }

                if (type == CollisionType.Feedback)
                {
                    if (Helper.IsTriggerActive(trigger)) TriggerSignal(trigger);
                }
                return;
            }

            ObjectTriggerApproach approach = other.gameObject.GetComponent<ObjectTriggerApproach>();
            if (approach != null)
            {
                if (type == CollisionType.Gameplay && approachTrigger == null)
                {
                    approachTrigger = approach;
                }
                return;
            }

            AchievementTrigger achievement = other.gameObject.GetComponent<AchievementTrigger>();
            if (achievement != null)
            {
                if (type == CollisionType.Gameplay)
                {
                    TriggerAchievement(achievement);
                }
                return;
            }
        }

        if (type == CollisionType.Normal)
        {
            Debug.Log(string.Format("Collision {0}", type));

            if (go.name.Equals("FrontTrigger"))
            {
                GameObject noseRoot = GameObject.Find("Dummy_noze00");
                RandomNose(noseRoot);
                ResetNose(noseRoot);
            }
            if (go.name.Equals("LeftWingTrigger") || go.name.Equals("LeftBackTrigger"))
            {
                float delta = Random.value * -0.01f;
                wingCollideLeft.transform.localPosition += new Vector3(0.0f, 0.0f, delta);
            }
            if (go.name.Equals("RightWingTrigger") || go.name.Equals("RightBackTrigger"))
            {
                float delta = Random.value * -0.01f;
                wingCollideRight.transform.localPosition += new Vector3(0.0f, 0.0f, delta);
            }

            if (currentTrigger != null)
                GameplayEnd(currentTrigger);

            Vector3 direction = rotation * Vector3.forward;
            float forwardVelocity = Vector3.Project(velocity, direction).magnitude;
            lastForwardVelocity = forwardVelocity;

            rigidbody.isKinematic = false;
            planeModel.animation.Play("idle");
            launch = false;
            GameObject currentCamera = GameObject.FindWithTag("CurrentCamera");
            if (currentCamera != null)
            {
                Debug.Log(currentCamera.name);
                Fader currentFader = currentCamera.GetComponent<Fader>();
                if (currentFader != null) currentFader.FadeIn();
            }

            //SoundManager.Instance.PlayCue("crash"); SOUND
            //Debug.Log(string.Format("Velocity at impact {0}", forwardVelocity));


            rigidbody.velocity = velocity;
        }
    }

    void RandomNose(GameObject nose)
    {
        if (nose == null)
            return;

        nose.transform.localRotation = Quaternion.AngleAxis((Random.value * 2.0f - 1.0f) * 45.0f, Vector3.forward);

        foreach (Transform child in nose.transform)
        {
            RandomNose(child.gameObject);
        }
    }

    void ResetNose(GameObject nose)
    {
        if (nose == null)
            return;

        Debug.Log("Reset " + nose.name);

        //Hashtable props = new Hashtable();
        //props.Add("delay", GameData.Instance.Tweak.RespawnDelay * 0.5f);
        //props.Add("localRotation", Quaternion.identity);
        //props.Add("drive", typeof(Ani.Drive.Slerp));
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //Ani.Mate.Finish(nose.transform);
        //Ani.Mate.To(nose.transform, GameData.Instance.Tweak.RespawnDelay, props);

        StartCoroutine(Animate.LerpTo(GameData.Instance.Tweak.RespawnDelay * 0.5f, GameData.Instance.Tweak.RespawnDelay, nose.transform.localRotation, Quaternion.identity, (v,ev) => nose.transform.localRotation = v, Animate.QuadraticInOut));

        foreach (Transform child in nose.transform)
        {
            ResetNose(child.gameObject);
        }
    }

    public void ReachLevelLimits(FadeTrigger fadeTrigger)
    {
        GameObject currentCamera = GameObject.FindWithTag("CurrentCamera");
        if (currentCamera != null)
        {
            Fader currentFader = currentCamera.GetComponent<Fader>();
            if (currentFader != null) currentFader.FadeIn();

        }
        if (currentTrigger != null) GameplayEnd(currentTrigger);
        GameplayStart(fadeTrigger);
    }

    public void TriggerExit(GameObject go, Collider other, CollisionType type)
    {
        if (other != null)
        {
            CollisionPropagate otherCollision = (CollisionPropagate)other.gameObject.GetComponent(typeof(CollisionPropagate));
            if (otherCollision != null)
            {
                return;
            }

            ObjectTriggerBase trigger = (ObjectTriggerBase)other.gameObject.GetComponent(typeof(ObjectTriggerBase));
            if (trigger != null)
            {
                if (type == CollisionType.Gameplay)
                {
                    GameplayEnd(trigger);
                }
            }

            ObjectTriggerApproach approach = other.gameObject.GetComponent<ObjectTriggerApproach>();
            if (approach != null && type == CollisionType.Gameplay && approachTrigger != null)
            {
                approachTrigger = null;
            }
        }
    }

    public void GameplayStart(ObjectTriggerBase trigger)
    {
        GameplayStart(trigger, false);
    }

    public void GameplayStart(ObjectTriggerBase trigger, bool force)
    {
        if (!Helper.IsTriggerActive(trigger)) return;

        if (currentTrigger == null || force)
        {
            //Debug.Log(string.Format("Starting gameplay {0}", trigger.GameplayType));
            currentTrigger = trigger;
            gameplayTimer = 0.0f;

            InitGameplay(trigger.GameplayType);

            if (trigger.GameplayType != GameplayType.LevelLimits) AddTrail();
            //if (currentTrigger.GameplayType == GameplayType.TubeVertical) 
            doStoreEnter = true;
            //SoundManager.Instance.TriggerEnter(); SOUND
        }
        else
        {
            //Debug.Log("Enqueue trigger");
            triggers.Add(trigger);
        }
    }

    public void GameplayEnd(ObjectTriggerBase trigger)
    {
        if (currentTrigger != null)
        {
            Debug.Log(string.Format("Ending gameplay {0}", currentTrigger.GameplayType));

            ExitGameplay(currentTrigger.GameplayType);
            //SoundManager.Instance.TriggerExit(); SOUND

            currentTrigger = null;

            if (triggers.Count > 0)
            {
                GameplayStart(triggers[0], true);
                triggers.RemoveAt(0);
            }

            List<ObjectTriggerBase> triggersTemp = new List<ObjectTriggerBase>();
            foreach (ObjectTriggerBase triggerBase in triggers)
            {
                if (trigger != triggerBase)
                {
                    triggersTemp.Add(triggerBase);
                }
            }
            triggers = triggersTemp;

            if (triggers.Count == 0)
            {
                gameplayTimer = gameplayDelay;
                DetachTrail();
            }
        }
    }

    void AddTrail()
    {
        rightTrail = new GameObject("RightTrail");
        rightTrail.transform.parent = GameObject.Find("RightTrail_dummy").transform;
        rightTrail.transform.localPosition = Vector3.zero;
        Trail rightTrailRender = (Trail)rightTrail.AddComponent(typeof(Trail));
        rightTrailRender.material = new Material(Shader.Find("Particles/Alpha Blended"));
        rightTrailRender.material.SetColor("_TintColor", Color.white);
        rightTrailRender.material.SetTexture("_MainTex", (Texture2D)Resources.Load("Textures/trail"));
        rightTrailRender.colors = new Color[1];
        rightTrailRender.colors[0] = new Color(124, 34, 23);// Color.white;
        rightTrailRender.widths = new float[2];
        rightTrailRender.widths[0] = 0.01f;
        rightTrailRender.widths[1] = 0.04f;
        rightTrailRender.emit = true;
        rightTrailRender.maxVertexDistance = 0.0001f;
        rightTrailRender.lifeTime = 15;

        leftTrail = new GameObject("LeftTrail");
        leftTrail.transform.parent = GameObject.Find("LeftTrail_dummy").transform;
        leftTrail.transform.localPosition = Vector3.zero;
        Trail leftTrailRender = (Trail)leftTrail.AddComponent(typeof(Trail));
        leftTrailRender.material = new Material(Shader.Find("Particles/Alpha Blended"));
        leftTrailRender.material.SetColor("_TintColor", Color.white);
        leftTrailRender.material.SetTexture("_MainTex", (Texture2D)Resources.Load("Textures/trail"));
        leftTrailRender.colors = new Color[1];
        leftTrailRender.colors[0] = new Color(124, 34, 23);//Color.white;
        leftTrailRender.widths = new float[2];
        leftTrailRender.widths[0] = 0.01f;
        leftTrailRender.widths[1] = 0.04f;
        leftTrailRender.emit = true;
        leftTrailRender.lifeTime = 15;
    }

    public void AttachParticle()
    {
        //GameObject rightTrail = GameObject.Find("RightTrail");
        //GameObject leftTrail = GameObject.Find("LeftTrail");

        //Trail rightComponentTrail = (Trail)(rightTrail.GetComponent(typeof(Trail)));
        //Trail leftComponentTrail = (Trail)(leftTrail.GetComponent(typeof(Trail)));

        //rightComponentTrail.material.SetColor("_TintColor", Color.Lerp(new Color(124f / 255f, 34f / 255f, 23f / 255f), Color.white, boostTimer));
        //leftComponentTrail.material.SetColor("_TintColor", Color.Lerp(new Color(124f / 255f, 34f / 255f, 23f / 255f), Color.white, boostTimer));
        Debug.Log("Attachparticle");
        rightTrailParticle = (GameObject)GameObject.Instantiate(particleTrailPrefab);
        rightTrailParticle.transform.parent = GameObject.Find("RightTrail_dummy").transform;
        rightTrailParticle.transform.localPosition = Vector3.zero;
        rightTrailParticle.transform.localRotation = Quaternion.identity;
        leftTrailParticle = (GameObject)GameObject.Instantiate(particleTrailPrefab);
        leftTrailParticle.transform.parent = GameObject.Find("LeftTrail_dummy").transform;
        leftTrailParticle.transform.localPosition = Vector3.zero;
        leftTrailParticle.transform.localRotation = Quaternion.identity;
    }

    public void DetachParticle()
    {
        if (rightTrailParticle != null)
        {
            rightTrailParticle.name = "PendingPlaneParticle";
            rightTrailParticle.transform.parent = null;
            rightTrailParticle.particleEmitter.emit = false;
            rightTrailParticle = null;
            leftTrailParticle.name = "PendingPlaneParticle";
            leftTrailParticle.transform.parent = null;
            leftTrailParticle.particleEmitter.emit = false;
            leftTrailParticle = null;
        }
    }

    public void DetachTrail()
    {
        GameObject rightTrail = GameObject.Find("RightTrail");
        GameObject leftTrail = GameObject.Find("LeftTrail");
        if (rightTrail != null && leftTrail != null)
        {
            rightTrail.transform.parent = null;
            leftTrail.transform.parent = null;
            rightTrail.name = "PendingTrail";
            leftTrail.name = "PendingTrail";
            rightTrail.tag = "ToDestroy";
            leftTrail.tag = "ToDestroy";
            Trail trail = (Trail)(rightTrail.GetComponent(typeof(Trail)));
            if (trail != null) trail.emit = false;
            trail = (Trail)(leftTrail.GetComponent(typeof(Trail)));
            if (trail != null) trail.emit = false;
        }

        GameObject[] garbTrail = GameObject.FindGameObjectsWithTag("ToDestroy");
        foreach (GameObject obj in garbTrail)
        {
            Trail pendingTrail = (Trail)obj.GetComponent(typeof(Trail));
            if (pendingTrail == null)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
