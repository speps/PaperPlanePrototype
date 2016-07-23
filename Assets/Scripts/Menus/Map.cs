using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(GUITexture))]
public class Map : MonoBehaviour
{
    Texture plane;

    float planeAngle = 0.0f;
    float planeX = 0.0f;
    float planeY = 0.0f;

    void Start()
    {
        if (plane == null)
        {
            plane = GameData.Instance.Menu.MapPlane;
        }
    }

    void Update()
    {
        float scaleX = (float)guiTexture.texture.width / Screen.width;
        float scaleY = (float)guiTexture.texture.height / Screen.height;

        if (scaleX > scaleY)
        {
            transform.localScale = new Vector3(1.0f, (float)Screen.width * guiTexture.texture.height / guiTexture.texture.width / Screen.height);
        }
        else
        {
            transform.localScale = new Vector3((float)Screen.height * guiTexture.texture.width / guiTexture.texture.height / Screen.width, 1.0f);
        }

        GameObject mapLimitObject = GameObject.Find("MapLimit");
        MapLimit mapLimit = mapLimitObject.GetComponent<MapLimit>();

        Vector3 planeWorld = Main.Instance.FlightController.transform.position;
        Vector3 planeLimit = mapLimitObject.transform.InverseTransformPoint(planeWorld);

        planeX = Mathf.Clamp01((planeLimit.x + 0.5f));
        planeY = Mathf.Clamp01((planeLimit.z + 0.5f));

        planeAngle = (mapLimitObject.transform.eulerAngles[1] - 360.0f) + Mathf.Atan2(Main.Instance.FlightController.velocity.x, Main.Instance.FlightController.velocity.z) * Mathf.Rad2Deg;
    }

    void OnGUI()
    {
        GameObject mapLimitObject = GameObject.Find("MapLimit");
        MapLimit mapLimit = mapLimitObject.GetComponent<MapLimit>();

        GUI.matrix = Matrix4x4.identity;

        float sizeX = Screen.width * (float)plane.width / guiTexture.texture.width;
        float sizeY = Screen.height * (float)plane.height / guiTexture.texture.height;

        float texX = Screen.width * 0.5f - (Screen.width * transform.localScale.x * 0.5f) - sizeX * 0.5f;
        float texY = Screen.height * 0.5f - (Screen.height * transform.localScale.y * 0.5f) - sizeY * 0.5f;

        float normX = (float)mapLimit.x / guiTexture.texture.width + planeX * mapLimit.width / guiTexture.texture.width;
        float normY = (float)mapLimit.y / guiTexture.texture.height + planeY * mapLimit.height / guiTexture.texture.height;

        float posX = Screen.width * transform.localScale.x * normX;
        float posY = Screen.height * transform.localScale.y * normY;

        texX += posX;
        texY += posY;

        GUIUtility.RotateAroundPivot(planeAngle, new Vector2(texX + sizeX * 0.5f, texY + sizeY * 0.5f));
        GUI.DrawTexture(new Rect(texX, texY, sizeX, sizeY), plane, ScaleMode.ScaleToFit);
    }

}
