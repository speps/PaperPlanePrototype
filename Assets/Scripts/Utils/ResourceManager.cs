using UnityEngine;
using System.Collections;

public class ResourceManager
{
    private static ResourceManager _instance;

    public ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ResourceManager();
            }
            return _instance;
        }
    }


}
