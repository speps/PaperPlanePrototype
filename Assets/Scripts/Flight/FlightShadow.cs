using UnityEngine;

public class FlightShadow : MonoBehaviour
{
    Vector3 offset = new Vector3(0.0f, 0.1f, 0.0f);

    GameObject flightController;

    Projector shadowProjector;

    void Start()
    {
        flightController = GameObject.Find("FlightController");

        shadowProjector = (Projector)gameObject.AddComponent(typeof(Projector));
        shadowProjector.nearClipPlane = 0.01f;
        shadowProjector.farClipPlane = 0.5f;
        shadowProjector.orthographic = true;
        shadowProjector.orthographicSize = 0.2f;
        shadowProjector.material = (Material)Resources.Load("Shadow");
        shadowProjector.ignoreLayers = 1 << 8;
    }

    void Update()
    {
        transform.position = flightController.transform.position + offset;
        transform.rotation = Quaternion.Euler(90.0f, flightController.transform.eulerAngles[1], 0.0f);
    }
}
