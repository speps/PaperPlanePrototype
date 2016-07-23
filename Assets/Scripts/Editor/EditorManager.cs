using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Xml.Serialization;
using System.Threading;

public class EditorManager : EditorWindow
{
    Vector2 scroll;

    [MenuItem("PaperPlane/Window")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(EditorManager), false, "PaperPlane");
    }

    [MenuItem("PaperPlane/Create Book data")]
    static void CreateBookData()
    {
        Selection.activeObject = CreateScriptableObject<TweakDataAsset>("BookData", true);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("PaperPlane/Create Tweak data")]
    static void CreateTweakData()
    {
        Selection.activeObject = CreateScriptableObject<TweakDataAsset>("TweakData", true);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("PaperPlane/Create Controls data")]
    static void CreateControlsData()
    {
        Selection.activeObject = CreateScriptableObject<ControlsDataAsset>("ControlsData", true);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("PaperPlane/Create Menu data")]
    static void CreateMenuData()
    {
        Selection.activeObject = CreateScriptableObject<MenuDataAsset>("MenuData", true);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
    }

    static T CreateScriptableObject<T>(string name, bool uniqueName) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        var fileName = "Assets/Data/" + name + ".asset";
        if (uniqueName)
            fileName = AssetDatabase.GenerateUniqueAssetPath(fileName);
        AssetDatabase.CreateAsset(asset, fileName);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    void OnProjectChange()
    {
        //Debug.Log("Project change");
    }

    void OnHierarchyChange()
    {
        //Debug.Log("Hierarchy change");
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        if (GUILayout.Button("Reset Terrain"))
        {
            ResetTerrain();
        }
        if (GUILayout.Button("Disable parts colliders"))
        {
            DisablePartsColliders();
        }
        if (GUILayout.Button("Import PopUps from XML"))
        {
            ImportPopUpsXML();
        }

        EditorGUILayout.EndScrollView();
    }

    void ResetTerrain()
    {
        GameObject terrain = GameObject.FindGameObjectWithTag("Terrain");
        MeshFilter meshFilter = (MeshFilter)terrain.GetComponent(typeof(MeshFilter));
        Mesh mesh = meshFilter.sharedMesh;

        Color[] colors = mesh.colors;
        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            colors[i] = Color.black;
        }
        mesh.colors = colors;
    }

    void DisablePartsColliders()
    {
        var gameObjects = (GameObject[])FindSceneObjectsOfType(typeof(GameObject));
        for (int i = 0; i < gameObjects.Length; i++)
        {
            var go = gameObjects[i];
            if (HelperConstants.IsPart(go.name))
            {
                var mc = go.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    mc.enabled = false;
                }
            }
        }
    }

    public class DataPopUp
    {
        public string AnimName = "#not set#";
        public string AnimSound = "";
        public ObjectPopUp.SoundType DefaultSound = ObjectPopUp.SoundType.None;
        public string CustomPopSound = "";
        public bool Activated;
        public bool PopActivates;
        public int SketchCounter;
        public bool IsSketch;
        public List<string> Linked = new List<string>();
        public List<string> KillDuringSketch = new List<string>();
    }

    void ImportPopUpsXML()
    {
        var dir = EditorUtility.OpenFolderPanel("Open PopUps folder", "", "");
        if (string.IsNullOrEmpty(dir))
            return;
        var files = Directory.GetFiles(dir);
        XmlSerializer serializer = new XmlSerializer(typeof(DataPopUp));
        foreach (var file in files)
        {
            if (Path.GetExtension(file) != ".xml")
                continue;
            var name = Path.GetFileNameWithoutExtension(file);

            DataPopUp data;
            using (var r = new StreamReader(file))
            {
                data = (DataPopUp)serializer.Deserialize(r);
            }

            var asset = CreateScriptableObject<PopDataAsset>("Resources/PopUps/" + name, false);

            asset.AnimName = data.AnimName;
            asset.AnimSound = data.AnimSound;
            asset.DefaultSound = data.DefaultSound;
            asset.CustomPopSound = data.CustomPopSound;
            asset.Activated = data.Activated;
            asset.PopActivates = data.PopActivates;
            asset.SketchCounter = data.SketchCounter;
            asset.IsSketch = data.IsSketch;
            foreach (var s in data.Linked)
                if (!string.IsNullOrEmpty(s))
                    asset.Linked.Add(s);
            foreach (var s in data.KillDuringSketch)
                if (!string.IsNullOrEmpty(s))
                    asset.KillDuringSketch.Add(s);
        }

        AssetDatabase.SaveAssets();
    }
}
