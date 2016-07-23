using UnityEngine;

public class AnimateTexture : MonoBehaviour
{
    public float frameRate = 12.0f;
    public int nImagesX = 1;
    public int nImagesY = 1;
    public string texName = "_MainTex";

    public bool loop = false;

    bool _playing = false;
    bool playing
    {
        get { return _playing; }
        set { _playing = value; enabled = value; }
    }
    bool forward = true;

    private bool changed = true;

    int _totalImages = 1;
    public int totalImages
    {
        get { return _totalImages; }
        set { _totalImages = value; changed = true; }
    }
    int _index = 0;
    public int index
    {
        get { return _index; }
        set { _index = value; changed = true; }
    }
    float _timer = 0.0f;
    public float timer
    {
        get { return _timer; }
        set { _timer = value; changed = true; }
    }

    public event AtEndDelegate OnAnimateEnd;

    void Start()
    {
        frameRate = Mathf.Max(0.01f, frameRate);
        nImagesX = Mathf.Max(1, nImagesX);
        nImagesY = Mathf.Max(1, nImagesY);

        index = 0;
        timer = 0.0f;
    }

    public void SetFromTime(float seconds)
    {
        totalImages = nImagesX * nImagesY;
        frameRate = totalImages / seconds;
    }

    public void ToStart()
    {
        index = 0;
    }

    public void ToEnd()
    {
        totalImages = nImagesX * nImagesY;
        index = totalImages - 1;
    }

    public void PlayForward()
    {
        playing = true;
        forward = true;
        ToStart();
    }

    public void PlayBackward()
    {
        playing = true;
        forward = false;
        ToEnd();
    }

    public void UpdateUV()
    {
        if (renderer == null)
            return;

        int indexX = index % nImagesX;
        int indexY = indexX / totalImages;

        foreach (Material material in renderer.materials)
        {
            if (material != null && material.HasProperty(texName))
            {
                material.SetTextureOffset(texName, new Vector2(indexX * (1.0f / nImagesX), indexY * (1.0f / nImagesY)));
                material.SetTextureScale(texName, new Vector2(1.0f / nImagesX, 1.0f / nImagesY));
            }
        }
    }

    void Update()
    {
        totalImages = nImagesX * nImagesY;

        if (playing)
        {
            timer += Time.deltaTime;

            while (timer >= 1.0f / frameRate)
            {
                index = (index + totalImages + (forward ? 1 : -1)) % totalImages;
                timer -= 1.0f / frameRate;
            }

            if (((index == totalImages - 1 && forward) || (index == 0 && !forward)) && !loop)
            {
                playing = false;
                if (OnAnimateEnd != null)
                {
                    OnAnimateEnd();
                    //Debug.Log("OnAnimateEnd");
                }
                else
                {
                    //Debug.Log(gameObject.name+" is not registered");
                }
                //raise event here to notify animation end
            }
        }

        if (changed)
        {
            UpdateUV();
            changed = false;
        }
    }
}
