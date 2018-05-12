using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlotGraph : MonoBehaviourSingleton<PlotGraph>
{
    private List<float> average_values = new List<float>();
    private List<float> landed_count = new List<float>();
    private float max_value = 0;
    public float padding = 0.5f;

    // will be recalculated at start
    Vector2 size = new Vector2(10, 5);
    Vector2 left_bottom_origin = Vector2.zero;

    private void Start() {
        size = new Vector2(transform.localScale.x - padding * 2, transform.localScale.y - padding * 2);
        left_bottom_origin = new Vector2(transform.position.x - size.x / 2, transform.position.y - size.y / 2);

        average_values.Add(0);
        landed_count.Add(0);
    }

    public void AddValueAverage(float val) {
        max_value = Mathf.Max(max_value, val);
        average_values.Add(val);
    }

    public void AddValueCount(float val) {
        landed_count.Add(val);
    }


    public void OnRenderObject()
    {
        // Draw axis
        preDrawLine(Color.green);
        drawLine(left_bottom_origin, left_bottom_origin + new Vector2(0, size.y));
        drawLine(left_bottom_origin, left_bottom_origin + new Vector2(size.x, 0));

        float x_padding = size.x / average_values.Count;
        float y_scale = size.y / max_value;

        // Draw the individual points
        for (int x = 0; x < average_values.Count - 1; x++) {
            Vector2 start = left_bottom_origin + new Vector2(x * x_padding, average_values[x] * y_scale);
            Vector2 end = left_bottom_origin + new Vector2((x + 1) * x_padding, average_values[x + 1] * y_scale);
            drawLine(start, end, Color.magenta);
            drawLine(left_bottom_origin + new Vector2((x+1) * x_padding, -0.1f), 
                     left_bottom_origin + new Vector2((x+1) * x_padding, +0.1f), 
                     Color.blue);
        }

        int ga_generation_size = Generator.Instance.GA_NumOfEntitiesInGeneration;
        for (int x = 0; x < landed_count.Count - 1; x++) {
            Vector2 start = left_bottom_origin + new Vector2(x * x_padding, landed_count[x] * size.y / ga_generation_size);
            Vector2 end = left_bottom_origin + new Vector2((x + 1) * x_padding, landed_count[x+1] * size.y / ga_generation_size);
            drawLine(start, end, Color.yellow);
            drawLine(left_bottom_origin + new Vector2((x + 1) * x_padding, -0.1f),
                     left_bottom_origin + new Vector2((x + 1) * x_padding, +0.1f),
                     Color.yellow);
        }

        postDrawLine();
    }


    static Material lineMaterial;
    public static void preDrawLine(Color col)
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }

        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        //GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);

        GL.Color(col);
    }

    public static void drawLine(Vector3 start, Vector3 end)
    {
        GL.Vertex3(start.x, start.y, start.z);
        GL.Vertex3(end.x, end.y, end.z);
    }

    public static void drawLine(Vector3 start, Vector3 end, Color col)
    {
        GL.Color(col);

        GL.Vertex3(start.x, start.y, start.z);
        GL.Vertex3(end.x, end.y, end.z);
    }

    public static void drawSquare(Vector3 center, float diameter)
    {
        Vector3 p1 = new Vector3(center.x + diameter / 2, 0, center.z + diameter / 2);
        Vector3 p2 = new Vector3(center.x + diameter / 2, 0, center.z - diameter / 2);
        Vector3 p3 = new Vector3(center.x - diameter / 2, 0, center.z - diameter / 2);
        Vector3 p4 = new Vector3(center.x - diameter / 2, 0, center.z + diameter / 2);

        drawLine(p1, p2);
        drawLine(p2, p3);
        drawLine(p3, p4);
        drawLine(p4, p1);
    }

    public static void postDrawLine()
    {
        GL.End();
        GL.PopMatrix();
    }

}