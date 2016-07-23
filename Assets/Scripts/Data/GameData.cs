using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour
{
    public BookDataAsset Book;
    public TweakDataAsset Tweak;
    public ControlsDataAsset Controls;
    public MenuDataAsset Menu;

    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.Find("GameData").GetComponent<GameData>();
            return _instance;
        }
    }

}
