using UnityEngine;
using System.Collections.Generic;

public class Graph
{
    struct Value
    {
        public float time;
        public float value;

        public Value(float time, float value)
        {
            this.time = time;
            this.value = value;
        }
    }

    Vector2 min, max, position, size;
    Color color;

    Material lineMaterial;

    List<Value> values = new List<Value>();
    List<Value> tempValues = new List<Value>();

    public Graph(Vector2 min, Vector2 max, Vector2 position, Vector2 size, Color color)
    {
        this.min = min;
        this.max = max;
        this.position = position;
        this.size = size;
        this.color = color;

        lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
        "SubShader { Pass {" +
        "   BindChannels { Bind \"Color\",color }" +
        "   Blend SrcAlpha OneMinusSrcAlpha" +
        "   ZWrite Off Cull Off Fog { Mode Off }" +
        "} } }");
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    public void PushValue(float value)
    {
        values.Add(new Value(Time.time, value));
    }

    public void Draw()
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        for (int i = 0; i < lineMaterial.passCount; ++i)
        {
            lineMaterial.SetPass(i);

            float mid = max.y / (max.y - min.y);

            GL.Begin(GL.LINES);

            GL.Color(Color.white);
            GL.Vertex3(position.x, 1.0f - position.y, 0);
            GL.Vertex3(position.x + size.x, 1.0f - position.y, 0);

            GL.Vertex3(position.x + size.x, 1.0f - position.y, 0);
            GL.Vertex3(position.x + size.x, 1.0f - (position.y + size.y), 0);

            GL.Vertex3(position.x + size.x, 1.0f - (position.y + size.y), 0);
            GL.Vertex3(position.x, 1.0f - (position.y + size.y), 0);

            GL.Vertex3(position.x, 1.0f - (position.y + size.y), 0);
            GL.Vertex3(position.x, 1.0f - position.y, 0);

            //GL.Color(color);
            GL.Vertex3(position.x, 1.0f - (position.y + size.y * mid), 0);
            GL.Vertex3(position.x + size.x, 1.0f - (position.y + size.y * mid), 0);
            GL.End();

            GL.Color(color);
            GL.Begin(GL.LINES);
            for (int v = 0; v < values.Count - 1; ++v)
            {
                Value value = values[v];
                Value valueNext = values[v + 1];

                float x = size.x * (Time.time - value.time) / (max.x - min.x);
                float xNext = size.x * (Time.time - valueNext.time) / (max.x - min.x);

                float y = size.y * -value.value / (max.y - min.y);
                float yNext = size.y * -valueNext.value / (max.y - min.y);

                GL.Vertex3(position.x + x, 1.0f - (position.y + y + size.y * mid), 0);
                GL.Vertex3(position.x + xNext, 1.0f - (position.y + yNext + size.y * mid), 0);
            }
            GL.End();
        }

        GL.PopMatrix();
    }

    public void Update()
    {
        List<Value> swapValues = values;

        tempValues.Clear();
        foreach (Value value in values)
        {
            float delta = Time.time - value.time;
            if (delta < (max.x - min.x))
            {
                tempValues.Add(value);
            }
        }

        values = tempValues;
        tempValues = swapValues;
    }
}
