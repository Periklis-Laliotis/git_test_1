using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class TerrainPathGeneratorSmooth : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;

    [Header("Path Points")]
    public Transform[] pathPoints;

    [Header("Path Settings")]
    [Range(0.01f, 1f)] public float heightOffset = 0.05f;
    [Range(2, 50)] public int segmentsPerCurve = 10; // περισσότερα = πιο smooth

    [Header("Runtime")]
    public bool generatePath = false;

    private LineRenderer line;

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && generatePath)
        {
            generatePath = false;
            GenerateSmoothPath();
        }
#endif
    }

    public void GenerateSmoothPath()
    {
        if (terrain == null || pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogWarning("⚠️ Δεν έχει Terrain ή αρκετά path points.");
            return;
        }

        line = GetComponent<LineRenderer>();
        List<Vector3> finalPoints = new List<Vector3>();

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            // Υπολογισμός Catmull-Rom spline
            Vector3 p0 = i == 0 ? pathPoints[i].position : pathPoints[i - 1].position;
            Vector3 p1 = pathPoints[i].position;
            Vector3 p2 = pathPoints[i + 1].position;
            Vector3 p3 = i + 2 < pathPoints.Length ? pathPoints[i + 2].position : pathPoints[i + 1].position;

            for (int j = 0; j < segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                Vector3 pos = CatmullRom(p0, p1, p2, p3, t);

                // Κόλλημα στο terrain
                pos.y = terrain.SampleHeight(pos) + terrain.GetPosition().y + heightOffset;
                finalPoints.Add(pos);
            }
        }

        // Προσθέτουμε και το τελευταίο σημείο
        Vector3 end = pathPoints[pathPoints.Length - 1].position;
        end.y = terrain.SampleHeight(end) + terrain.GetPosition().y + heightOffset;
        finalPoints.Add(end);

        // Εφαρμογή στη γραμμή
        line.positionCount = finalPoints.Count;
        line.SetPositions(finalPoints.ToArray());

        Debug.Log($"✅ Smooth μονοπάτι δημιουργήθηκε και παραμένει σταθερό ({finalPoints.Count} σημεία).");
    }

    // Catmull-Rom spline interpolation
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
