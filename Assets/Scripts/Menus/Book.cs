using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Book : MonoBehaviour
{
    [Serializable]
    public class Page
    {
        public string object0;
        public string object1;
        public string object2;
        public string object3;

        [NonSerialized] public bool activated0;
        [NonSerialized] public bool activated1;
        [NonSerialized] public bool activated2;
        [NonSerialized] public bool activated3;

        public Texture texMap;
        public Texture texDrawing0;
        public Texture texDrawing1;
        public Texture texDrawing2;
        public Texture texDrawing3;
        public Texture texPolaroid0;
        public Texture texPolaroid1;
        public Texture texPolaroid2;
        public Texture texPolaroid3;

        public Texture tex0 { get { return activated0 ? texPolaroid0 : texDrawing0; } }
        public Texture tex1 { get { return activated1 ? texPolaroid1 : texDrawing1; } }
        public Texture tex2 { get { return activated2 ? texPolaroid2 : texDrawing2; } }
        public Texture tex3 { get { return activated3 ? texPolaroid3 : texDrawing3; } }
    }

    int pageCurrent = 0;
    int pagePrevious = 0;

    GameObject book;

    Material matPage0;
    Material matPolaroid00;
    Material matPolaroid01;
    Material matPolaroid02;
    Material matPolaroid03;

    Material matPage1;
    Material matPolaroid10;
    Material matPolaroid11;
    Material matPolaroid12;
    Material matPolaroid13;

    public bool CanTurnForward
    {
        get { return pageCurrent < (GameData.Instance.Book.Pages.Count - 1) && !animation.isPlaying; }
    }

    public bool CanTurnBackward
    {
        get { return pageCurrent > 0 && !animation.isPlaying; }
    }

    public BookDataAsset Data;

    void Start()
    {
        book = GameObject.Find("PolaroidBook");

        matPage0 = book.renderer.materials[13];
        matPolaroid00 = book.renderer.materials[5];
        matPolaroid01 = book.renderer.materials[6];
        matPolaroid02 = book.renderer.materials[3];
        matPolaroid03 = book.renderer.materials[4];

        matPage1 = book.renderer.materials[2];
        matPolaroid10 = book.renderer.materials[11];
        matPolaroid11 = book.renderer.materials[12];
        matPolaroid12 = book.renderer.materials[9];
        matPolaroid13 = book.renderer.materials[10];

        matPage0.color = Color.white;
        matPolaroid00.color = Color.white;
        matPolaroid01.color = Color.white;
        matPolaroid02.color = Color.white;
        matPolaroid03.color = Color.white;

        matPage1.color = Color.white;
        matPolaroid10.color = Color.white;
        matPolaroid11.color = Color.white;
        matPolaroid12.color = Color.white;
        matPolaroid13.color = Color.white;

        ResetBook();
    }

    public void ResetBook()
    {
        pageCurrent = pagePrevious = 0;
        Init(true);
    }

    public void Activate(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        foreach (Page page in GameData.Instance.Book.Pages)
        {
            if (string.Compare(name, page.object0, true) == 0)
                page.activated0 = true;
            if (string.Compare(name, page.object1, true) == 0)
                page.activated1 = true;
            if (string.Compare(name, page.object2, true) == 0)
                page.activated2 = true;
            if (string.Compare(name, page.object3, true) == 0)
                page.activated3 = true;
        }
    }

    public void Init(bool forward)
    {
        int page0 = forward ? pagePrevious : pageCurrent;
        int page1 = forward ? pageCurrent : pagePrevious;

        Page data0 = GameData.Instance.Book.Pages[page0];
        Page data1 = GameData.Instance.Book.Pages[page1];

        //data0.Load();
        //data1.Load();

        matPage0.mainTexture = data0.texMap;
        matPolaroid00.mainTexture = data0.tex0;
        matPolaroid01.mainTexture = data0.tex1;
        matPolaroid02.mainTexture = data0.tex2;
        matPolaroid03.mainTexture = data0.tex3;

        matPage1.mainTexture = data1.texMap;
        matPolaroid10.mainTexture = data1.tex0;
        matPolaroid11.mainTexture = data1.tex1;
        matPolaroid12.mainTexture = data1.tex2;
        matPolaroid13.mainTexture = data1.tex3;
    }

    public void TurnForward()
    {
        pagePrevious = pageCurrent;
        ++pageCurrent;
        Init(true);
    }

    public void TurnBackward()
    {
        pagePrevious = pageCurrent;
        --pageCurrent;
        Init(false);
    }

    void Update()
    {
        
    }
}
