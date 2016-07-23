using UnityEngine;
#if UNITY_STANDALONE_WIN
using XInputDotNetPure;
#endif

enum ControlScheme
{
    GamePad,
    Keyboard,
    TouchScreen
}

//[ExecuteInEditMode]
public class Controls : MonoBehaviour
{
    GUIStyle textStyle;
    GUIStyle keyStyle;

    float timerStickRUp = 0.0f;
    bool stickRUp = false;

    bool playerIndexSet = false;
#if UNITY_STANDALONE_WIN
    PlayerIndex playerIndex;
    GamePadState padState;
    GamePadState padPrevState;
#endif

    public bool LaunchPlane;
    public bool Pause;
    public bool Restart;
    public bool Boost;
    public bool NoBoost; // To check for return to idle state

    public bool MenuLeft;
    public bool MenuRight;
    public bool MenuEnter;
    public bool MenuExit;

    public float TriggerLeft;
    public float TriggerRight;

    float keyboardTrigSmooth = 0.1f;
    float keyboardTrigLeftCurrent = 0.5f;
    float keyboardTrigLeftTarget = 0.5f;
    float keyboardTrigRightCurrent = 0.5f;
    float keyboardTrigRightTarget = 0.5f;

    static Controls instance;
    public static Controls Instance
    {
        get { return instance; }
    }

    ControlScheme Scheme;

    public Controls()
    {
        instance = this;
    }

    void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
        Scheme = ControlScheme.Keyboard;
#elif UNITY_ANDROID
        Scheme = ControlScheme.TouchScreen;
        if (Application.isEditor)
            Scheme = ControlScheme.Keyboard;
#endif

        textStyle = new GUIStyle();
        textStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        textStyle.font = GameData.Instance.Controls.FontText;

        keyStyle = new GUIStyle();
        keyStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        keyStyle.font = GameData.Instance.Controls.FontKey;
    }

    void Update()
    {
#if UNITY_STANDALONE_WIN
        if (!playerIndexSet || !padPrevState.IsConnected)
        {
            playerIndexSet = false;
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected)
                {
                    Debug.Log(string.Format("GamePad {0}", testPlayerIndex));
                    playerIndex = testPlayerIndex;
                    playerIndexSet = true;
                    padPrevState = testState;
                }
            }
        }

        if (!playerIndexSet)
        {
            Scheme = ControlScheme.Keyboard;
        }
        else
        {
            Scheme = ControlScheme.GamePad;
        }
#endif

        if (Scheme == ControlScheme.GamePad)
        {
#if UNITY_STANDALONE_WIN
            padState = GamePad.GetState(playerIndex);

            LaunchPlane = (padState.ThumbSticks.Right.Y > 0.8f || padState.ThumbSticks.Left.Y > 0.8f) || Input.GetKeyDown(KeyCode.UpArrow);
            Pause = (padState.Buttons.Start == ButtonState.Released && padPrevState.Buttons.Start == ButtonState.Pressed) || Input.GetKeyUp(KeyCode.Escape);
            Restart = (padState.Buttons.Back == ButtonState.Pressed && padPrevState.Buttons.Back == ButtonState.Released) || Input.GetKeyUp(KeyCode.Return);

            MenuLeft = (padState.Buttons.LeftShoulder == ButtonState.Pressed && padPrevState.Buttons.LeftShoulder == ButtonState.Released) || Input.GetKeyUp(KeyCode.LeftArrow);
            MenuRight = (padState.Buttons.RightShoulder == ButtonState.Pressed && padPrevState.Buttons.RightShoulder == ButtonState.Released) || Input.GetKeyUp(KeyCode.RightArrow);
            MenuEnter = (padState.Buttons.A == ButtonState.Pressed && padPrevState.Buttons.A == ButtonState.Released) || Input.GetKeyUp(KeyCode.Return);
            MenuExit = (padState.Buttons.B == ButtonState.Pressed && padPrevState.Buttons.B == ButtonState.Released) || Input.GetKeyUp(KeyCode.Escape);

            Boost = (padState.ThumbSticks.Right.Y >= 0.8f || padState.ThumbSticks.Left.Y >= 0.8f) || Input.GetKeyUp(KeyCode.Space);
            NoBoost = padState.ThumbSticks.Right.Y < 0.2f && padState.ThumbSticks.Left.Y < 0.2f;

            TriggerLeft = Helper.EaseIn(padState.Triggers.Left);
            TriggerRight = Helper.EaseIn(padState.Triggers.Right);

            padPrevState = padState;
#endif
        }
        else if (Scheme == ControlScheme.Keyboard)
        {
            LaunchPlane = Input.GetKeyDown(KeyCode.UpArrow);
            Pause = Input.GetKeyUp(KeyCode.Escape);
            Restart = Input.GetKeyUp(KeyCode.Return);

            MenuLeft = Input.GetKeyUp(KeyCode.LeftArrow);
            MenuRight = Input.GetKeyUp(KeyCode.RightArrow);
            MenuEnter = Input.GetKeyUp(KeyCode.Return);
            MenuExit = Input.GetKeyUp(KeyCode.Escape);

            Boost = Input.GetKeyUp(KeyCode.Space);
            NoBoost = true;

            // Triggers
            {
                keyboardTrigLeftTarget = Input.GetKey(KeyCode.LeftArrow) ? 1.0f : 0.0f;
                keyboardTrigRightTarget = Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f;

                keyboardTrigLeftCurrent += (keyboardTrigLeftTarget - keyboardTrigLeftCurrent) * keyboardTrigSmooth;
                keyboardTrigRightCurrent += (keyboardTrigRightTarget - keyboardTrigRightCurrent) * keyboardTrigSmooth;
            }

            TriggerLeft = keyboardTrigLeftCurrent;
            TriggerRight = keyboardTrigRightCurrent;
        }
        else if (Scheme == ControlScheme.TouchScreen)
        {
            LaunchPlane = Input.touchCount == 2;
        }
    }

    void FixedUpdate()
    {
        timerStickRUp += Time.fixedDeltaTime;

        if (timerStickRUp >= 0.25f)
        {
            stickRUp = !stickRUp;
            timerStickRUp = 0.0f;
        }
    }

    void OnGUI()
    {
        float recWidth = 1280.0f;
        float recHeight = 720.0f;

        float scale = Mathf.Min(
            (float)Screen.width / recWidth,
            (float)Screen.height / recHeight
        );
        int scaledWidth = (int)(recWidth * scale);
        int scaledHeight = (int)(recHeight * scale);

        Vector2 offset = new Vector2(Screen.width - scaledWidth, Screen.height - scaledHeight);

        GUI.matrix = Matrix4x4.TRS(new Vector3(offset.x * 0.5f, offset.y * 0.5f, 0.0f), Quaternion.identity, new Vector3(scale, scale, 1.0f));

        if (Scheme == ControlScheme.GamePad)
        {
            if (Main.Instance.PlayMode == PlayMode.Flight)
            {
                if (Main.Instance.FlightController.flying)
                {
                    if (Main.Instance.FlightController.flyingTimer < 4.0f)
                    {
                        GUI.DrawTexture(new Rect(GameData.Instance.Controls.TexPadLeftTrigger.width, recHeight - GameData.Instance.Controls.TexPadLeftTrigger.height, GameData.Instance.Controls.TexPadLeftTrigger.width / 2, GameData.Instance.Controls.TexPadLeftTrigger.height / 2), GameData.Instance.Controls.TexPadLeftTrigger);
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadRightTrigger.width * 1.5f, recHeight - GameData.Instance.Controls.TexPadRightTrigger.height, GameData.Instance.Controls.TexPadRightTrigger.width / 2, GameData.Instance.Controls.TexPadRightTrigger.height / 2), GameData.Instance.Controls.TexPadRightTrigger);
                        GUI.DrawTexture(new Rect(recWidth * 0.5f - GameData.Instance.Controls.TexPadBack.width * 0.5f, recHeight - GameData.Instance.Controls.TexPadBack.height * 2, GameData.Instance.Controls.TexPadBack.width, GameData.Instance.Controls.TexPadBack.height), GameData.Instance.Controls.TexPadBack);
                    }
                    else if (Main.Instance.FlightController.boostAvailable)
                    {
                        var texPad = stickRUp ? GameData.Instance.Controls.TexPadStickRUp : GameData.Instance.Controls.TexPadStickR;
                        GUI.DrawTexture(new Rect(recWidth - texPad.width, recHeight - texPad.height, texPad.width / 2, texPad.height / 2), texPad);
                    }
                }
            }

            if (Main.Instance.PlayMode == PlayMode.Menu && Main.Instance.MenuController.state != MenuController.State.Animating)
            {
                if (Main.Instance.MenuController.state == MenuController.State.Pause)
                {
                    GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadStart.width * 2, recHeight - GameData.Instance.Controls.TexPadStart.height * 2, GameData.Instance.Controls.TexPadStart.width, GameData.Instance.Controls.TexPadStart.height), GameData.Instance.Controls.TexPadStart);
                }
                else
                {
                    GUI.DrawTexture(new Rect(GameData.Instance.Controls.TexPadLeftButton.width, GameData.Instance.Controls.TexPadLeftButton.height, GameData.Instance.Controls.TexPadLeftButton.width / 2, GameData.Instance.Controls.TexPadLeftButton.height / 2), GameData.Instance.Controls.TexPadLeftButton);
                    GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadRightButton.width * 1.5f, GameData.Instance.Controls.TexPadRightButton.height, GameData.Instance.Controls.TexPadRightButton.width / 2, GameData.Instance.Controls.TexPadRightButton.height / 2), GameData.Instance.Controls.TexPadRightButton);

                    if (Main.Instance.MenuController.state == MenuController.State.Flight)
                    {
                        var texPad = stickRUp ? GameData.Instance.Controls.TexPadStickRUp : GameData.Instance.Controls.TexPadStickR;
                        GUI.DrawTexture(new Rect(recWidth - texPad.width, recHeight - texPad.height, texPad.width / 2, texPad.height / 2), texPad);
                    }
                    else if (Main.Instance.MenuController.state == MenuController.State.Album)
                    {
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadB.width * 2, recHeight - GameData.Instance.Controls.TexPadB.height * 2, GameData.Instance.Controls.TexPadB.width, GameData.Instance.Controls.TexPadB.height), GameData.Instance.Controls.TexPadB);
                    }
                    else
                    {
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadA.width * 2, recHeight - GameData.Instance.Controls.TexPadA.height * 2, GameData.Instance.Controls.TexPadA.width, GameData.Instance.Controls.TexPadA.height), GameData.Instance.Controls.TexPadA);
                        if (Main.Instance.MenuController.state == MenuController.State.Multiplayer)
                        {
                            GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexPadA.width * 1.5f - GameData.Instance.Controls.TexNotAvailable.width * 0.5f, recHeight - GameData.Instance.Controls.TexPadA.height * 1.5f - GameData.Instance.Controls.TexNotAvailable.height * 0.5f, GameData.Instance.Controls.TexNotAvailable.width, GameData.Instance.Controls.TexNotAvailable.height), GameData.Instance.Controls.TexNotAvailable);
                        }
                    }
                }
            }
        }
        else if (Scheme == ControlScheme.Keyboard)
        {
            if (Main.Instance.PlayMode == PlayMode.Flight)
            {
                if (Main.Instance.FlightController.flying)
                {
                    if (Main.Instance.FlightController.flyingTimer < 4.0f)
                    {
                        GUI.DrawTexture(new Rect(GameData.Instance.Controls.TexKeyLeft.width * 0.5f, recHeight - GameData.Instance.Controls.TexKeyLeft.height * 1.5f, GameData.Instance.Controls.TexKeyLeft.width, GameData.Instance.Controls.TexKeyLeft.height), GameData.Instance.Controls.TexKeyLeft);
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyRight.width * 1.5f, recHeight - GameData.Instance.Controls.TexKeyRight.height * 1.5f, GameData.Instance.Controls.TexKeyRight.width, GameData.Instance.Controls.TexKeyRight.height), GameData.Instance.Controls.TexKeyRight);
                        GUI.DrawTexture(new Rect(recWidth / 2 - (GameData.Instance.Controls.TexKeyEnter.width * 0.75f) / 2, recHeight - 1.5f * GameData.Instance.Controls.TexKeyEnter.height * 0.75f, GameData.Instance.Controls.TexKeyEnter.width * 0.75f, GameData.Instance.Controls.TexKeyEnter.height * 0.75f), GameData.Instance.Controls.TexKeyEnter);
                    }
                    else if (Main.Instance.FlightController.boostAvailable)
                    {
                        GUI.DrawTexture(new Rect(recWidth / 2 - GameData.Instance.Controls.TexKeySpace.width / 2, recHeight - GameData.Instance.Controls.TexKeySpace.height * 1.5f, GameData.Instance.Controls.TexKeySpace.width, GameData.Instance.Controls.TexKeySpace.height), GameData.Instance.Controls.TexKeySpace);
                    }
                }
            }

            if (Main.Instance.PlayMode == PlayMode.Menu && Main.Instance.MenuController.state != MenuController.State.Animating)
            {
                if (Main.Instance.MenuController.state == MenuController.State.Pause)
                {
                    GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyEsc.width * 2, recHeight - GameData.Instance.Controls.TexKeyEsc.height * 2, GameData.Instance.Controls.TexKeyEsc.width, GameData.Instance.Controls.TexKeyEsc.height), GameData.Instance.Controls.TexKeyEsc);
                }
                else if ((int)Main.Instance.MenuController.state > (int)MenuController.State.Intro)
                {
                    GUI.DrawTexture(new Rect(GameData.Instance.Controls.TexKeyLeft.width * 0.5f, GameData.Instance.Controls.TexKeyLeft.height, GameData.Instance.Controls.TexKeyLeft.width, GameData.Instance.Controls.TexKeyLeft.height), GameData.Instance.Controls.TexKeyLeft);
                    GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyRight.width * 1.5f, GameData.Instance.Controls.TexKeyRight.height, GameData.Instance.Controls.TexKeyRight.width, GameData.Instance.Controls.TexKeyRight.height), GameData.Instance.Controls.TexKeyRight);

                    if (Main.Instance.MenuController.state == MenuController.State.Flight)
                    {
                        GUI.DrawTexture(new Rect(recWidth / 2 - GameData.Instance.Controls.TexKeyUp.width / 2, recHeight - GameData.Instance.Controls.TexKeyUp.height * 1.5f, GameData.Instance.Controls.TexKeyUp.width, GameData.Instance.Controls.TexKeyUp.height), GameData.Instance.Controls.TexKeyUp);
                    }
                    else if (Main.Instance.MenuController.state == MenuController.State.Album)
                    {
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyEsc.width * 2, recHeight - GameData.Instance.Controls.TexKeyEsc.height * 2, GameData.Instance.Controls.TexKeyEsc.width, GameData.Instance.Controls.TexKeyEsc.height), GameData.Instance.Controls.TexKeyEsc);
                    }
                    else
                    {
                        GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyEnter.width * 2, recHeight - GameData.Instance.Controls.TexKeyEnter.height * 2, GameData.Instance.Controls.TexKeyEnter.width, GameData.Instance.Controls.TexKeyEnter.height), GameData.Instance.Controls.TexKeyEnter);
                        if (Main.Instance.MenuController.state == MenuController.State.Multiplayer)
                        {
                            GUI.DrawTexture(new Rect(recWidth - GameData.Instance.Controls.TexKeyEnter.width * 1.5f - GameData.Instance.Controls.TexNotAvailable.width * 0.5f, recHeight - GameData.Instance.Controls.TexKeyEnter.height * 1.5f - GameData.Instance.Controls.TexNotAvailable.height * 0.5f, GameData.Instance.Controls.TexNotAvailable.width, GameData.Instance.Controls.TexNotAvailable.height), GameData.Instance.Controls.TexNotAvailable);
                        }
                    }
                }
            }
        }

        GUI.matrix = Matrix4x4.identity;
    }
}
