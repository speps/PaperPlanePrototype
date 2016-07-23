using UnityEngine;
using System.Collections;

public enum PlayMode
{
    Menu,
    Sketch,
    Flight
}

public delegate void AtEndDelegate();

public class Helper
{
    public enum TubeType
    {
        Vertical,
        Horizontal
    }

    public static void RecursiveMinMax(GameObject go, ref Vector3 min, ref Vector3 max)
    {
        if (go.renderer != null)
        {
            Vector3 localMin, localMax;
            localMin = go.renderer.bounds.min;
            localMax = go.renderer.bounds.max;

            min = Vector3.Min(min, localMin);
            max = Vector3.Max(max, localMax);
        }

        foreach (Transform child in go.transform)
        {
            RecursiveMinMax(child.gameObject, ref min, ref max);
        }
    }

    public static float EaseOut(float x)
    {
        return 1.0f - (x - 1.0f) * (x - 1.0f);
    }

    public static float EaseIn(float x)
    {
        return 1.0f - (1.0f - x) * (1.0f - x);
    }

    public static Quaternion Delta(Quaternion from, Quaternion to)
    {
        return Quaternion.Inverse(from) * to;
    }

    public static Vector3 ProjectOntoLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        float t;
        return ProjectOntoLine(lineStart, lineEnd, point, out t);
    }

    public static Vector3 ProjectOntoLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point, out float t)
    {
        Vector3 line = lineEnd - lineStart;
        Vector3 startToPoint = point - lineStart;

        t = Vector3.Dot(line, startToPoint) / line.sqrMagnitude;

        return lineStart + t * line;
    }

    public static Mesh BuildCone(TubeType tubeType, float length, float radius1, float radius2, int resolution)
    {
        return BuildCone(tubeType, length, radius1, radius2, resolution, Vector3.zero);
    }

    public static Mesh BuildDisk(TubeType tubeType, float length, float radius, int resolution)
    {
        Mesh feedbackMesh = new Mesh();
        Vector3 delta = Vector3.zero;// (tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward);
        float angleDelta = 2 * Mathf.PI / Mathf.Max(1, resolution);
        Vector3[] vertices = new Vector3[resolution + 1];

        vertices[0] = new Vector3(delta.x, delta.y, delta.z);
        for (int n = 1, i = 0; n <= resolution; ++n, ++i)
        {
            float angle = angleDelta * i;
            vertices[n] = delta + Helper.GetCirclePoint(angle, radius, tubeType);
        }

        int[] triangles = new int[resolution * 3];

        for (int i = 0; i < resolution; ++i)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i >= resolution - 1 ? resolution : i + 1;
            triangles[i * 3 + 2] = i >= resolution - 1 ? 1 : i + 2;
        }

        feedbackMesh.vertices = vertices;
        feedbackMesh.triangles = triangles;
        feedbackMesh.name = "SurfaceParticle";
        feedbackMesh.RecalculateBounds();
        feedbackMesh.RecalculateNormals();

        return feedbackMesh;
    }

    public static Mesh BuildCone(TubeType tubeType, float length, float radius1, float radius2, int resolution, Vector3 offset)
    {
        Mesh tubeMesh = new Mesh();
        Vector3 delta = (tubeType == TubeType.Vertical ? Vector3.up : Vector3.forward) * length * 0.5f;
        float angleDelta = 2 * Mathf.PI / Mathf.Max(1, resolution);

        Vector3[] vertices = new Vector3[resolution * 2 + 2];

        vertices[0] = offset + new Vector3(delta.x, delta.y, delta.z);
        for (int n = 1, i = 0; n <= resolution; ++n, ++i)
        {
            float angle = angleDelta * i;
            vertices[n] = offset + delta + GetCirclePoint(angle, radius1, tubeType);
        }
        vertices[resolution + 1] = offset + new Vector3(-delta.x, -delta.y, -delta.z);
        for (int n = resolution + 2, i = 0; n <= resolution * 2 + 1; ++n, ++i)
        {
            float angle = angleDelta * i;
            vertices[n] = offset - delta + GetCirclePoint(angle, radius2, tubeType);
        }

        int[] triangles = new int[resolution * 4 * 3];
        for (int i = 0; i < resolution; ++i)
        {
            triangles[i * 3] = i + 1;
            triangles[i * 3 + 1] = 0;
            triangles[i * 3 + 2] = i >= resolution - 1 ? 1 : (i + 2);

            triangles[(i + resolution) * 3] = resolution + 1;
            triangles[(i + resolution) * 3 + 1] = (i + resolution + 1) + 1;
            triangles[(i + resolution) * 3 + 2] = i >= resolution - 1 ? resolution + 2 : ((i + resolution + 1) + 2);

            triangles[(i + resolution * 2) * 3] = (i + resolution + 1) + 1;
            triangles[(i + resolution * 2) * 3 + 1] = i + 1;
            triangles[(i + resolution * 2) * 3 + 2] = i >= resolution - 1 ? 1 : (i + 2);

            triangles[(i + resolution * 3) * 3] = (i + resolution + 1) + 1;
            triangles[(i + resolution * 3) * 3 + 1] = i >= resolution - 1 ? 1 : (i + 2);
            triangles[(i + resolution * 3) * 3 + 2] = i >= resolution - 1 ? resolution + 2 : ((i + resolution + 1) + 2);
        }

        tubeMesh.vertices = vertices;
        tubeMesh.triangles = triangles;
        tubeMesh.RecalculateBounds();
        tubeMesh.RecalculateNormals();

        return tubeMesh;
    }

    public static Mesh BuildTube(TubeType tubeType, float length, float radius, int resolution)
    {
        return BuildCone(tubeType, length, radius, radius, resolution);
    }

    public static Vector3 GetCirclePoint(float angle, float radius, TubeType tubeType)
    {
        return new Vector3(radius * Mathf.Cos(angle), tubeType == TubeType.Vertical ? 0.0f : radius * Mathf.Sin(angle), tubeType == TubeType.Vertical ? radius * Mathf.Sin(angle) : 0.0f);
    }

    public static bool IsPopped(string name, out ObjectPopUp objectPopUp)
    {
        GameObject linkedObject = null;
        if (!string.IsNullOrEmpty(name))
        {
            linkedObject = ObjectPopUp.Find(name);
        }
        objectPopUp = null;
        if (linkedObject != null)
        {
            objectPopUp = linkedObject.GetComponent<ObjectPopUp>();
            if (objectPopUp == null)
            {
                return false;
            }
            if (objectPopUp.state == ObjectPopUp.State.InGround)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsPopped(string name)
    {
        ObjectPopUp popup;
        return IsPopped(name, out popup);
    }

    public static bool IsTriggerActive(ObjectTriggerBase trigger)
    {
        return IsPopped(trigger.LinkedObject);
    }

}
