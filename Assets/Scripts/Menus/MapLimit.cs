using UnityEngine;

//[ExecuteInEditMode]
public class MapLimit : MonoBehaviour
{
    public int x = 0;
    public int y = 0;
    public int width = 1024;
    public int height = 654;

    void Start()
    {
        /*
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Vector3[] normals = meshFilter.sharedMesh.normals;
        Vector3[] newNormals = new Vector3[normals.Length];

        int i = 0;
        foreach(Vector3 v in normals)
        {
            newNormals[0] = -v;
            ++i;
        }

        meshFilter.sharedMesh.normals = newNormals;
        normals = null;
         * */
    }

    void OnDrawGizmos()
    {
#if false
        if(Application.isPlaying)
        {
            Color defaultColor = Gizmos.color;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right);
            Gizmos.color = Color.yellow;
            Vector3 flyPosition = Main.Instance.FlightController.gameObject.transform.position;
            Gizmos.DrawLine(transform.position, flyPosition);
            Vector3 projOnZ = Helper.ProjectOntoLine(transform.position, transform.position + transform.forward, flyPosition);
            Vector3 projOnX = Helper.ProjectOntoLine(transform.position, transform.position + transform.right, flyPosition);
            Gizmos.DrawSphere(projOnZ, 1);
            Gizmos.DrawSphere(projOnX, 1);
            //Debug.Log(string.Format("X={0}:Z={1}", (projOnX-transform.position).magnitude, (projOnZ-transform.position).magnitude));
            Gizmos.color = defaultColor;
        }
#endif
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }

    public Vector3 GetRelativePos(Vector3 position)
    {
        Vector3 projOnZ = Helper.ProjectOntoLine(transform.position, transform.position + transform.forward, position);
        Vector3 projOnX = Helper.ProjectOntoLine(transform.position, transform.position + transform.right, position);
        Vector3 projOnY = Helper.ProjectOntoLine(transform.position, transform.position + transform.up, position);
        float relPosZ = (projOnZ - transform.position).magnitude / (transform.localScale.z * 0.5f);
        float relPosX = (projOnX - position).magnitude / (transform.localScale.x * 0.5f);
        float relPosY = (projOnY - transform.position).magnitude / (transform.localScale.y * 0.5f);

        return new Vector3(relPosX, relPosY , relPosZ);
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles[1], 0.0f);
        //WTF ?
        transform.localScale = new Vector3(transform.localScale.x, 0.0f, transform.localScale.z);
        //WTP ?
        //transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
