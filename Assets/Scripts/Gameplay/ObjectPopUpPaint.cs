using System.Collections.Generic;
using UnityEngine;

public class ObjectPopUpPaint : MonoBehaviour
{
    public struct VertexInfo
    {
        public int index;
        public float distance;
        public Vector3 vertex;
    }

    public ObjectPopUp parent;
    internal MeshPainter meshPainter;

    public VertexInfo[] vertices;

    float alpha = 1.0f;
    public float Alpha
    {
        get { return alpha; }
        set
        {
            alpha = value;
            UpdateColors();
        }
    }

    public float radius;
    public float Radius
    {
        get { return radius; }
        set
        {
            radius = value;
            UpdateColors();
        }
    }

    public float radiusMax;

    void Start()
    {

    }

    public void UpdateColors()
    {
        for (int i = 0; vertices != null && i < vertices.Length; i++)
        {
            float factor = (vertices[i].distance < radius ? +1.0f : 0.0f) * 1.0f;
            meshPainter.PushVertex(vertices[i].index, new Color(factor, 0.0f, 0.0f));
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white * 0.5f;
        Gizmos.DrawWireSphere(transform.position, radiusMax);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);

        /*if (parent.gameObject.name.Equals("POP_balancoire"))
        {
            foreach (VertexInfo vertex in vertices)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vertex.vertex, 0.1f);
            }
        }*/
    }

    void Update()
    {

    }
}
