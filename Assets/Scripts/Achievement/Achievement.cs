using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    class Content
    {
        public string text;
        public int score;

        public Content(string text, int score)
        {
            this.text = text;
            this.score = score;
        }
    }

    public Texture Texture;
    public Font Font;

    GUIStyle textStyle;
    AchievementAnim anim;

    static Achievement instance;
    public static Achievement Instance
    { get { return instance; } }

    string text = "";

    bool showing = false;

    List<Content> queue = new List<Content>();

    void Start()
    {
        instance = this;

        textStyle = new GUIStyle();
        textStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        textStyle.font = Font;

        anim = gameObject.AddComponent<AchievementAnim>();
        anim.texWidth = Texture.width;
        anim.texHeight = Texture.height;
        anim.width = 350;
        anim.height = 86;
        anim.frames = 191;

        anim.AtEnd += AtEnd;
        anim.ShowText += ShowText;
        anim.HideText += HideText;
    }

    public void ShowText()
    {
        //Hashtable props = new Hashtable();
        //props.Add("textColor", new Color(1.0f, 1.0f, 1.0f, 1.0f));
        //props.Add("direction", Ani.Easing.Out);
        //Ani.Mate.Stop(textStyle.normal);
        //Ani.Mate.To(textStyle.normal, 0.1f, props);

        StartCoroutine(Animate.LerpTo(0.0f, 0.1f, textStyle.normal.textColor, Color.white, (v, ev) => textStyle.normal.textColor = v, Animate.Linear));
    }

    public void HideText()
    {
        //Hashtable props = new Hashtable();
        //props.Add("textColor", new Color(1.0f, 1.0f, 1.0f, 0.0f));
        //props.Add("direction", Ani.Easing.Out);
        //Ani.Mate.Stop(textStyle.normal);
        //Ani.Mate.To(textStyle.normal, 0.1f, props);

        StartCoroutine(Animate.LerpTo(0.0f, 0.1f, textStyle.normal.textColor, new Color(1.0f, 1.0f, 1.0f, 0.0f), (v, ev) => textStyle.normal.textColor = v, Animate.Linear));
    }

    public void AtEnd()
    {
        showing = false;

        if (queue.Count > 0)
        {
            Content content = queue[0];
            queue.RemoveAt(0);
            Show(content.text, content.score);
        }
    }

    public void Show(string content, int score)
    {
        if (!showing)
        {
            showing = true;
            text = string.Format("Achievement Unlocked\n{0}G - {1}", score, content);
            anim.Play();
            //SoundManager.Instance.PlayCue("achievement"); SOUND
        }
        else
        {
            queue.Add(new Content(content, score));
        }
    }

    void OnGUI()
    {
        GUIx.DrawTexture(new Rect(Screen.width / 2 - anim.width / 2, 20, anim.width, anim.height), anim.uvs, Texture);
        GUI.Label(new Rect(Screen.width / 2 - anim.width / 2 + 90, 36, anim.width, anim.height), text, textStyle);
    }
}
