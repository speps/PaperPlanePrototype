using System.Collections.Generic;
using UnityEngine;

public class PopDataAsset : ScriptableObject
{
    public string AnimName = "#not set#";
    public string AnimSound = "";
    public ObjectPopUp.SoundType DefaultSound = ObjectPopUp.SoundType.None;
    public string CustomPopSound = "";
    public bool Activated;
    public bool PopActivates;
    public int SketchCounter;
    public bool IsSketch;
    public List<string> Linked = new List<string>();
    public List<string> KillDuringSketch = new List<string>();
}
