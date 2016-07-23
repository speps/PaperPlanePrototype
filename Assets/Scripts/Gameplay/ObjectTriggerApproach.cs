using UnityEngine;

public class ObjectTriggerApproach : MonoBehaviour
{
    Mesh coneMesh;

    public float radius1 = 0.5f;
    public float radius2 = 1.0f;
    public float length = 1.0f;
    public Vector3 offset = Vector3.zero;

    public ObjectTriggerFlyThrough parent;

    void Start()
    {
        if (radius1 <= 0.0f || radius2 <= 0.0f)
            return;

        if (rigidbody == null)
        {
            Rigidbody rb = (Rigidbody)gameObject.AddComponent(typeof(Rigidbody));
            rb.isKinematic = true;
        }

        coneMesh = Helper.BuildCone(Helper.TubeType.Horizontal, length, radius1, radius2, 12, offset);
        MeshCollider c = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
        c.sharedMesh = coneMesh;
        c.isTrigger = true;
        c.convex = true;
    }

    void Update()
    {
        //transform.localScale = new Vector3(1.0f, 1.0f, 1.0f / transform.parent.localScale.z);
    }
}
