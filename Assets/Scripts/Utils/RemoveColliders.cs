using UnityEngine;

public class RemoveColliders : MonoBehaviour
{
    void Start()
    {
        MeshCollider[] colliders = gameObject.GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider collider in colliders)
        {
            Destroy(collider);
        }

        Destroy(this);
    }
}
