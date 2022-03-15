using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkRenderer : Graphic
{
    public float thickness = 3f;
    public bool useGameObject = false;
    // world position
    public Vector3 end = Vector3.zero;
    public GameObject go_end;

    float width;
    float height;
    List<Vector2> points;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (useGameObject)
            end = go_end.transform.position;

        width = transform.InverseTransformPoint(end).x;
        height = transform.InverseTransformPoint(end).y;

        points = new List<Vector2>();
        points.Add(Vector2.zero);
        points.Add(Vector2.one);

        //float angle = GetAngle((Vector2)transform.localPosition, (Vector2)end.transform.localPosition) + 90f;
        float angle = GetAngle((Vector2)transform.localPosition, (Vector2)transform.InverseTransformPoint(end)) + 90f;
        DrawVerticesForPoint(points[0], points[1], angle, vh);

        for (int i = 0; i < points.Count - 1; i++)
        {
            int index = i * 4;
            vh.AddTriangle(index + 0, index + 1, index + 2);
            vh.AddTriangle(index + 1, index + 2, index + 3);
        }
    }

    public float GetAngle(Vector2 me, Vector2 target)
    {
        return (float)(Mathf.Atan2(Screen.width * (target.y - me.y), Screen.height * (target.x - me.x)) * (180 / Mathf.PI));
    }

    void DrawVerticesForPoint(Vector2 point, Vector2 point2, float angle, VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        
        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(width * point.x, height * point.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(width * point.x, height * point.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(width * point2.x, height * point2.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(width * point2.x, height * point2.y);
        vh.AddVert(vertex);
    }
}