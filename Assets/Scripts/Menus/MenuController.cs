using UnityEngine;

public class MenuController : MonoBehaviour
{
    public enum State
    {
        Controls,
        Disclaimer,
        Wait,
        Intro,
        Flight,
        Pause,
        Multiplayer,
        Table,
        Album,
        Exit,
        Quit,
        Animating
    }

    public State state = State.Animating;
    string currentAnim = "";
    State tempState = State.Flight;

    GameObject bookObject;
    public Book book;

    public float pauseTimer = 0.0f;
    float pauseDelay = 0.1f;
    GameObject map;
#if UNITY_STANDALONE_WIN && !INTRO_DISABLED
    MovieTexture intro;
#endif

    void Start()
    {
        
    }

    public void Build()
    {
#if UNITY_STANDALONE_WIN && !INTRO_DISABLED
        intro = (MovieTexture)Resources.Load("Videos/intro");
#endif

        bookObject = (GameObject)Instantiate(GameData.Instance.Menu.BookPrefab);
        bookObject.animation.clip = null;
        bookObject.animation.playAutomatically = false;
        book = bookObject.AddComponent<Book>();
        bookObject.transform.position = new Vector3(-8.678f, 13.79f, -55.66f);
        bookObject.transform.rotation = Quaternion.Euler(272.7f, 96.0f, 218.7f);
        bookObject.animation.Stop();

        map = new GameObject("Map");
        map.AddComponent<GUITexture>();
        map.AddComponent<Map>();
        map.guiTexture.texture = GameData.Instance.Menu.Map;
        map.transform.position = new Vector3(0.5f, 0.5f);
        map.SetActiveRecursively(false);
    }

    void PlayAnim(string name, bool backwards)
    {
        currentAnim = name;
        Main.Instance.MenuCamera.Animation[name].speed = backwards ? -1.0f : 1.0f;
        if (backwards)
            Main.Instance.MenuCamera.Animation[name].time = Main.Instance.MenuCamera.Animation[name].length;
        Main.Instance.MenuCamera.Animation.Play(name);
    }

    void PlayBook(string name)
    {
        //Debug.Log(string.Format("PlayBook {0}", name));
        bookObject.animation.Play(name);
    }

    void OnPauseAnimEnd()
    {
        state = State.Pause;
    }

    public void Pause()
    {
        if (pauseTimer <= 0.0f)
        {
            Main.Instance.MenuCamera.transform.position = Main.Instance.PlayerStart.transform.position;
            Main.Instance.MenuCamera.transform.rotation = Main.Instance.PlayerStart.transform.rotation;
            Main.Instance.Play(PlayMode.Menu);

            state = State.Pause;
            map.SetActiveRecursively(true);

            Main.Instance.MenuCamera.SetEye(GameObject.Find("MapCamera"));

            pauseTimer = pauseDelay;
        }
    }

    public void Unpause()
    {
        if (pauseTimer <= 0.0f)
        {
            Main.Instance.Play(PlayMode.Flight);

            state = State.Flight;
            map.SetActiveRecursively(false);

            pauseTimer = pauseDelay;
        }
    }

    public void NextState()
    {
        if (state == State.Flight)
        {
            tempState = State.Table;
            PlayAnim("flight-table", false);
            state = State.Animating;
        }
        else if (state == State.Table)
        {
            tempState = State.Multiplayer;
            PlayAnim("table-multi", false);
            state = State.Animating;
        }
        else if (state == State.Multiplayer)
        {
            tempState = State.Exit;
            PlayAnim("multi-exit", false);
            state = State.Animating;
        }
        else if (state == State.Exit)
        {
            tempState = State.Flight;
            PlayAnim("exit-flight", false);
            state = State.Animating;
        }
    }

    public void PrevState()
    {
        if (state == State.Flight)
        {
            tempState = State.Exit;
            PlayAnim("exit-flight", true);
            state = State.Animating;
        }
        else if (state == State.Table)
        {
            tempState = State.Flight;
            PlayAnim("flight-table", true);
            state = State.Animating;
        }
        else if (state == State.Multiplayer)
        {
            tempState = State.Table;
            PlayAnim("table-multi", true);
            state = State.Animating;
        }
        else if (state == State.Exit)
        {
            tempState = State.Multiplayer;
            PlayAnim("multi-exit", true);
            state = State.Animating;
        }
    }

    public void SetState(State newState)
    {
        state = newState;
    }

    void Update()
    {
        string debugText = "";

        debugText += string.Format("\nState {0} Temp {1} Playing? {2}", state, tempState, Main.Instance.MenuCamera.Animation.isPlaying);

        if (state == State.Animating)
        {
            if (!Main.Instance.MenuCamera.Animation.isPlaying || Main.Instance.MenuCamera.Animation[currentAnim].time < 0.0f)
            {
                state = tempState;
                Main.Instance.MenuCamera.Animation.Stop();

                if (state == State.Quit)
                {
                    Debug.Log("Quit");
                    Application.Quit();
                }
            }
        }

        Main.Instance.ScreenEffect.guiTexture.enabled = state != State.Intro;
        GameObject.Find("Avion_ref").renderer.enabled = state == State.Pause;
        //GameObject.Find("Avion_ref").renderer.enabled = state == State.Flight && state != State.Animating || state == State.Pause;

        if (state == State.Flight && Controls.Instance.LaunchPlane)
        {
            GameObject.Find("Avion_ref").renderer.enabled = true;
            Main.Instance.FlightController.fadeOnInit = false;
            Main.Instance.FlightController.GameplayAlpha = 1.0f;
            Main.Instance.FlightCamera.transform.position = Main.Instance.MenuCamera.transform.position;
            Main.Instance.FlightCamera.transform.rotation = Main.Instance.MenuCamera.transform.rotation;
            Main.Instance.Play(PlayMode.Flight);
        }

        if (state == State.Pause && Controls.Instance.Pause)
        {
            Unpause();
        }

        pauseTimer -= Time.deltaTime;

        if (Controls.Instance.MenuRight)
        {
            NextState();
        }

        if (Controls.Instance.MenuLeft)
        {
            PrevState();
        }

        if (state == State.Album && Controls.Instance.MenuRight)
        {
            if (book.CanTurnForward)
            {
                PlayBook("turn-forward");
                book.TurnForward();
            }
        }

        if (state == State.Album && Controls.Instance.MenuLeft)
        {
            if (book.CanTurnBackward)
            {
                PlayBook("turn-backward");
                book.TurnBackward();
            }
        }

        if (state == State.Exit && Controls.Instance.MenuEnter)
        {
            tempState = State.Quit;
            PlayAnim("exit", false);
            state = State.Animating;
        }

#if UNITY_STANDALONE_WIN && !INTRO_DISABLED
        if (state == State.Wait)
        {
            if (intro != null)
            {
                if (!intro.isPlaying && intro.isReadyToPlay)
                {
                    intro.Play();
                    tempState = State.Intro;
                    state = State.Animating;
                }
            }
            else
            {
                tempState = State.Flight;
                state = State.Animating;
            }
        }
#else
        if (state == State.Wait)
        {
            tempState = State.Flight;
            state = State.Animating;
        }
#endif

        if (state == State.Controls && Controls.Instance.MenuEnter)
        {
            tempState = State.Disclaimer;
            state = State.Animating;
        }

        if (state == State.Disclaimer && Controls.Instance.MenuEnter)
        {
            tempState = State.Wait;
            state = State.Animating;
        }

#if UNITY_STANDALONE_WIN && !INTRO_DISABLED
        if (state == State.Intro && (Controls.Instance.MenuEnter || !intro.isPlaying))
        {
            intro.Stop();
            tempState = State.Flight;
            state = State.Animating;

            //Main.Instance.StartSound();
        }
#endif

        if (state == State.Table && Controls.Instance.MenuEnter)
        {
            book.ResetBook();

            tempState = State.Album;
            PlayAnim("table-book", false);
            PlayBook("open");
            state = State.Animating;
        }

        if (state == State.Album && Controls.Instance.MenuExit)
        {
            tempState = State.Table;
            PlayAnim("table-book", true);
            PlayBook("close");
            state = State.Animating;
        }

        /* Debug Text */
        {
            GameObject debugTextObject = GameObject.Find("Debug");
            if (debugTextObject != null)
            {
                debugTextObject.guiText.text = debugText;
            }
        }
    }

    public void OnGUI()
    {
#if UNITY_STANDALONE_WIN && !INTRO_DISABLED
        if (state == State.Intro)
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroBack, ScaleMode.StretchToFill);
            //GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroMovie, ScaleMode.ScaleToFit);
        }
#endif
        if (state == State.Controls)
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroBack, ScaleMode.StretchToFill);
            GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroControls, ScaleMode.ScaleToFit);
        }
        if (state == State.Disclaimer)
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroBack, ScaleMode.StretchToFill);
            GUI.DrawTexture(screenRect, GameData.Instance.Menu.IntroDisclaimer, ScaleMode.ScaleToFit);
        }
    }
}
