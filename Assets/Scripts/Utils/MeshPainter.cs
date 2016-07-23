using UnityEngine;
using System.Collections.Generic;
using System;

public class MeshPainter : MonoBehaviour
{
    public class VertexInfo
    {
        public int index;
        public Color color;
    }

    private VertexInfo[] vertices;
    private int verticesCount = 0;
    private Mesh mesh;

    public void Start()
    {
        GameObject terrain = GameObject.FindGameObjectWithTag("Terrain");

        if (terrain != null)
        {
            //MeshCollider meshCollider = terrain.GetComponent<MeshCollider>();
            //meshCollider.sharedMesh = (Mesh)Mesh.Instantiate(meshCollider.sharedMesh);

            MeshFilter meshFilter = terrain.GetComponent<MeshFilter>();
            mesh = meshFilter.sharedMesh;

            Color[] colors = mesh.colors;
            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                colors[i] = Color.black;
            }
            mesh.colors = colors;

            vertices = new VertexInfo[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                vertices[i] = new VertexInfo { index = -1 };
            }

            GameObject[] popUps = GameObject.FindGameObjectsWithTag("PopUp");

            var meshVerts = new Vector3[mesh.vertexCount];
            Array.Copy(mesh.vertices, meshVerts, mesh.vertexCount);

            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                Vector3 v = meshVerts[i];
                meshVerts[i] = terrain.transform.TransformPoint(v);
            }

            var popVerts = new List<ObjectPopUpPaint.VertexInfo>();
            foreach (GameObject popUp in popUps)
            {
                ObjectPopUp popUpComponent = (ObjectPopUp)popUp.GetComponent(typeof(ObjectPopUp));

                if (popUpComponent == null)
                    continue;

                popUpComponent.paintComponent.meshPainter = this;
                popUpComponent.RetrieveSoundHandles();

                popVerts.Clear();
                for (int i = 0; i < mesh.vertexCount; ++i)
                {
                    Vector3 v = meshVerts[i];

                    Vector3 delta = v - popUp.transform.position;
                    float deltaMag = delta.magnitude;
                    if (deltaMag <= popUpComponent.paintComponent.radiusMax)
                    {
                        var vertexInfo = new ObjectPopUpPaint.VertexInfo();

                        vertexInfo.index = i;
                        vertexInfo.distance = deltaMag;
                        vertexInfo.vertex = v;

                        popVerts.Add(vertexInfo);
                    }
                }
                popUpComponent.paintComponent.vertices = popVerts.ToArray();
                popUpComponent.paintComponent.UpdateColors();
            }
        }
    }

    public void PushVertex(int index, Color color)
    {
        vertices[index].index = index;
        vertices[index].color = color;

        verticesCount++;
    }

    public void Update()
    {
        if (verticesCount == 0)
            return;

        var colors = mesh.colors;
        for (int i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i];
            if (v.index == -1)
                continue;
            colors[i].r += v.color.r;
            colors[i].g += v.color.g;
            colors[i].b += v.color.b;
            vertices[i].index = -1;
        }
        mesh.colors = colors;

        verticesCount = 0;
    }
}
