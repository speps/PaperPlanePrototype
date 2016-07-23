using System;
using UnityEngine;

[RequireComponent(typeof(GUITexture))]
public class AchievementAnim : MonoBehaviour
{
    public int width;
    public int height;

    public int texWidth;
    public int texHeight;

    public int frames;
    public int frameRate = 30;

    public Rect uvs;

    public event AtEndDelegate ShowText;
    public event AtEndDelegate HideText;
    public event AtEndDelegate AtEnd;

    bool playing = false;
    int index;

    int framesX;
    int framesY;

    float timeAccum;

    void Start()
    {
        framesX = Mathf.CeilToInt((float)texWidth / width);
        framesY = Mathf.CeilToInt((float)texHeight / height);
    }

    public void Play()
    {
        index = 0;
        timeAccum = 0.0f;
        playing = true;
    }

    void Update()
    {
        if (playing)
        {
            timeAccum += Time.deltaTime;

            if (timeAccum > 1.0f / frameRate)
            {
                index = (index + 1) % Math.Max(1, frames);
                timeAccum -= 1.0f / frameRate;
            }

            if (index >= frames - 1)
            {
                playing = false;
                if (AtEnd != null)
                    AtEnd();
            }

            if (index == 3)
            {
                if (ShowText != null)
                    ShowText();
            }

            if (index == 179)
            {
                if (HideText != null)
                    HideText();
            }
        }

        int x = index % framesX;
        int y = Mathf.FloorToInt((float)index / framesX);

        uvs.width = (float)width / texWidth;
        uvs.height = (float)height / texHeight;
        uvs.x = x * uvs.width;
        uvs.y = y * uvs.height;
    }
}
