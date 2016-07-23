using UnityEngine;
using UnityEditor;

public class RaycastSnap : EditorWindow
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    private Axis direction = Axis.X;
    private bool positive = true;

    private bool applyPosition = true;
    private bool applyRotation = true;

    private Vector3 previewPosition;
    private Quaternion previewRotation;

    public Vector3 Direction
    {
        get
        {
            Vector3 dir = Vector3.right;
            if (direction == Axis.Y)
            {
                dir = Vector3.up;
            }
            else if (direction == Axis.Z)
            {
                dir = Vector3.forward;
            }
            if (!positive)
                dir *= -1;

            return dir;
        }
    }

    [MenuItem("Window/RaycastSnap")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(RaycastSnap), false, "RaycastSnap");
    }

    void Apply()
    {
        if (Selection.activeGameObject == null)
            return;

        GameObject go = Selection.activeGameObject;
        Vector3 dir = Direction;

        Ray ray = new Ray(go.transform.position, go.transform.TransformDirection(dir));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            go.transform.position = hit.point;
            go.transform.rotation = Quaternion.FromToRotation(-dir, hit.normal);
        }
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        applyPosition = EditorGUILayout.Toggle("Apply Position", applyPosition);
        applyRotation = EditorGUILayout.Toggle("Apply Rotation", applyRotation);

        direction = (Axis)EditorGUILayout.EnumPopup("Direction", direction);

        positive = EditorGUILayout.Toggle("Positive", positive);

        if (Selection.activeGameObject != null)
        {
            if (GUILayout.Button("Apply"))
            {
                Apply();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Selection.activeGameObject == null)
            return;

        GameObject go = Selection.activeGameObject;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(go.transform.position, previewPosition);
    }


    void Update()
    {
        
    }
}
