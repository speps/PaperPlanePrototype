using UnityEngine;

public class AchievementTrigger : MonoBehaviour
{
    public enum Shape
    {
        Box,
        Sphere
    }

    public Shape shape = Shape.Box;
    public string condition;
    public bool conditionTextured;

    public string achievementText;
    public int achievementScore;

    void Start()
    {
        if (shape == Shape.Box)
        {
            BoxCollider current = (BoxCollider)gameObject.AddComponent(typeof(BoxCollider));
            current.isTrigger = true;
        }

        if (shape == Shape.Sphere)
        {
            SphereCollider current = (SphereCollider)gameObject.AddComponent(typeof(SphereCollider));
            current.isTrigger = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        if (shape == Shape.Box)
        {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
        if (shape == Shape.Sphere)
        {
            Gizmos.DrawWireSphere(Vector3.zero, 1.0f);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    void Update()
    {
        
    }
}
