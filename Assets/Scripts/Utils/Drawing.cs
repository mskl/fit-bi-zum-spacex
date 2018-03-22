using UnityEngine;
using System.Collections.Generic;

public class Drawing : MonoBehaviourSingleton<Drawing>
{
    public List<float> values = new List<float>();
    float y_scale = 100;
    float y_size = 10;
    float x_size = 15;


    private void Start() {
        values.Add(0);
    }

    public void OnRenderObject()
    {
        preDrawLine(Color.green);

        // Draw axis
        drawLine(transform.position, transform.position + Vector3.up * y_size);
        drawLine(transform.position, transform.position + Vector3.right * x_size);

        GL.Color(Color.white);

        float x_padding = x_size / values.Count;

        // Draw the individual points
        for (int x = 0; x < values.Count - 1; x++) {
            Vector3 start = transform.position + new Vector3(x * x_padding, values[x] / y_scale);
            Vector3 end = transform.position + new Vector3((x + 1) * x_padding, values[x + 1] / y_scale);
            drawLine(start, end);
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