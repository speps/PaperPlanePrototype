using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlightDebug : MonoBehaviour
{
    Graph graphAltitude;
    public Graph Altitude
    {
        get { return graphAltitude; }
    }
    Graph graphEulerX;
    public Graph EulerX
    {
        get { return graphEulerX; }
    }
    Graph graphEulerY;
    public Graph EulerY
    {
        get { return graphEulerY; }
    }
    Graph graphEulerZ;
    public Graph EulerZ
    {
        get { return graphEulerZ; }
    }
    Graph graphShadowDistance;
    public Graph ShadowDistance
    {
        get { return graphShadowDistance; }
    }

    // Use this for initialization
    void Awake()
    {
        graphAltitude = new Graph(new Vector2(0, -10), new Vector2(5, 10), new Vector2(0.75f, 0.1f), new Vector2(0.25f, 0.25f), Color.white);
        graphEulerX = new Graph(new Vector2(0, 0), new Vector2(5, 360), new Vector2(0.75f, 0.1f), new Vector2(0.25f, 0.25f), Color.red);
        graphEulerY = new Graph(new Vector2(0, 0), new Vector2(5, 360), new Vector2(0.75f, 0.1f), new Vector2(0.25f, 0.25f), Color.green);
        graphEulerZ = new Graph(new Vector2(0, 0), new Vector2(5, 360), new Vector2(0.75f, 0.1f), new Vector2(0.25f, 0.25f), Color.blue);
        graphShadowDistance = new Graph(new Vector2(0, 0), new Vector2(5, 1), new Vector2(0.75f, 0.1f), new Vector2(0.25f, 0.25f), Color.magenta);
    }

    void OnPostRender()
    {
        /*graphAltitude.Draw();
        graphEulerX.Draw();
        graphEulerY.Draw();
        graphEulerZ.Draw();
        graphShadowDistance.Draw();*/
    }

    // Update is called once per frame
    void Update()
    {
        graphAltitude.Update();
        graphEulerX.Update();
        graphEulerY.Update();
        graphEulerZ.Update();
        graphShadowDistance.Update();
    }
}
