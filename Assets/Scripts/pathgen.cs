using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshPathGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;
    public LayerMask terrainLayer;

    [Header("Path Points")]
    public bool useChildPoints = true;
    public List<Transform> pathPoints = new List<Transform>();

    [Header("Path Appearance")]
    public float pathWidth = 1.5f;
    public float pathHeightOffset = 0.05f;
    public bool smoothPath = true;
    [Range(2, 50)] public int smoothSegments = 10;

    [Header("Generate")]
    public bool generatePath = false;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void OnValidate()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (generatePath)
        {
            generatePath = false;
            GenerateMeshPath();
        }
    }

    public void GenerateMeshPath()
    {
        if (terrain == null)
        {
            Debug.LogWarning("⚠️ Terrain not assigned!");
            return;
        }

        // Συλλογή σημείων
        List<Vector3> points = new List<Vector3>();
        if (useChildPoints)
        {
            pathPoints.Clear();
            for (int i = 0; i < transform.childCount; i++)
                pathPoints.Add(transform.GetChild(i));
        }

        if (pathPoints.Count < 2)
        {
            Debug.LogWarning("⚠️ Need at least 2 points to make a path!");
            return;
        }

        foreach (var t in pathPoints)
        {
            if (t == null) continue;
            points.Add(t.position);
        }

        // Smooth path
        if (smoothPath && points.Count > 2)
            points = SmoothPoints(points, smoothSegments);

        // Δημιουργία Mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float totalDistance = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0)
                totalDistance += Vector3.Distance(points[i - 1], points[i]);

            // Forward vector
            Vector3 forward = Vector3.zero;
            if (i < points.Count - 1)
                forward += (points[i + 1] - points[i]).normalized;
            if (i > 0)
                forward += (points[i] - points[i - 1]).normalized;
            forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized * (pathWidth * 0.5f);

            // Αριστερό και δεξί vertex
            Vector3 leftPos = points[i] - right;
            Vector3 rightPos = points[i] + right;

            // Raycast για κάθε vertex ώστε να κολλάει στο terrain
            leftPos = AlignToTerrain(leftPos);
            rightPos = AlignToTerrain(rightPos);

            vertices.Add(leftPos);
            vertices.Add(rightPos);

            uvs.Add(new Vector2(0, totalDistance));
            uvs.Add(new Vector2(1, totalDistance));

            if (i < points.Count - 1)
            {
                int baseIndex = i * 2;
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }

        // World -> Local
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = transform.InverseTransformPoint(vertices[i]);

        Mesh mesh = new Mesh();
        mesh.name = "Generated Path Mesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        Debug.Log($"✅ Mesh path generated with {vertices.Count} verts and {points.Count} points!");
    }

    private Vector3 AlignToTerrain(Vector3 pos)
    {
        Ray ray = new Ray(pos + Vector3.up * 50f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, terrainLayer))
        {
            pos.y = hit.point.y + pathHeightOffset;
        }
        else
        {
            // fallback σε SampleHeight
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + pathHeightOffset;
        }
        return pos;
    }

    private List<Vector3> SmoothPoints(List<Vector3> pts, int segments)
    {
        List<Vector3> smoothed = new List<Vector3>();
        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector3 p0 = pts[i];
            Vector3 p1 = pts[i + 1];
            for (int s = 0; s < segments; s++)
            {
                float t = s / (float)segments;
                smoothed.Add(Vector3.Lerp(p0, p1, t));
            }
        }
        smoothed.Add(pts[pts.Count - 1]);
        return smoothed;
    }
}
