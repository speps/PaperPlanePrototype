using UnityEngine;

public class Loading : MonoBehaviour
{
    public bool isLoading = true;

    Material loadingBackMaterial;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (loadingBackMaterial == null)
        {
            loadingBackMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                "SubShader { Pass { " +
                " Blend SrcAlpha OneMinusSrcAlpha " +
                " ZWrite Off Cull Off Fog { Mode Off } " +
                " BindChannels {" +
                " Bind \"vertex\", vertex Bind \"color\", color }" +
                "} } }");
            /*loadingBackMaterial = new Material("Shader \"Loading/Back\" {" +
            "SubShader { Pass { " +
            " Blend One One " +
            " ZWrite Off Cull Off Fog { Mode Off } " +
            " BindChannels {" +
            " Bind \"vertex\", vertex Bind \"color\", color }" +
            "} } }");*/
            loadingBackMaterial.hideFlags = HideFlags.HideAndDontSave;
            loadingBackMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnPostRender()
    {
        Init();

        loadingBackMaterial.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.Color(Color.white);
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.End();
    }

    void Update()
    {
        
    }
}
