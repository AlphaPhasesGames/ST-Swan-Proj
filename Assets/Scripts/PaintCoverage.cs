using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class PaintCoverage : MonoBehaviour, IPaintCoverage
{
    [Range(0f, 1f)]
    public float completionThreshold = 0.95f;
    [Header("Tuning")]
    public float coverageScale;
    public float sampleSpacing = 0.25f;

    public bool IsComplete => IsFullyPainted;

    // Sample point + which face it belongs to
    private List<(Vector3 point, Vector3 normal)> samplePoints
        = new List<(Vector3, Vector3)>();

    private HashSet<int> paintedSamples = new HashSet<int>();

    public bool IsFullyPainted =>
       paintedSamples.Count >= (samplePoints.Count * coverageScale) * completionThreshold;


    void Awake()
    {
        GenerateSamplePoints();
    }

    // =============================
    // SAMPLE GENERATION
    // =============================

    void GenerateSamplePoints()
    {
        samplePoints.Clear();

        Bounds b = GetComponent<Collider>().bounds;

        Vector3 min = b.min;
        Vector3 max = b.max;

        GenerateFace(min, max, Vector3.right);
        GenerateFace(min, max, Vector3.left);
        GenerateFace(min, max, Vector3.up);
        GenerateFace(min, max, Vector3.down);
        GenerateFace(min, max, Vector3.forward);
        GenerateFace(min, max, Vector3.back);
    }

    void GenerateFace(Vector3 min, Vector3 max, Vector3 normal)
    {
        Vector3 size = max - min;

        int axisA = normal.x != 0 ? 1 : 0;
        int axisB = normal.z != 0 ? 1 : 2;

        for (float a = 0; a <= size[axisA]; a += sampleSpacing)
            for (float b = 0; b <= size[axisB]; b += sampleSpacing)
            {
                Vector3 point = min;
                point[axisA] += a;
                point[axisB] += b;

                if (normal.x > 0) point.x = max.x;
                if (normal.x < 0) point.x = min.x;
                if (normal.y > 0) point.y = max.y;
                if (normal.y < 0) point.y = min.y;
                if (normal.z > 0) point.z = max.z;
                if (normal.z < 0) point.z = min.z;

                samplePoints.Add((point, normal));
            }
    }

    // =============================
    // PAINT REGISTRATION
    // =============================

    public void RegisterPaint(Vector3 hitPoint, Vector3 hitNormal, float radius)
    {
        float sqrRadius = radius * radius;
        bool newlyPainted = false;

        for (int i = 0; i < samplePoints.Count; i++)
        {
            if (paintedSamples.Contains(i))
                continue;

            //  Only paint samples on the hit face
            if (Vector3.Dot(samplePoints[i].normal, hitNormal) < 0.9f)
                continue;

            if ((samplePoints[i].point - hitPoint).sqrMagnitude <= sqrRadius)
            {
                paintedSamples.Add(i);
                newlyPainted = true;
            }
        }

        if (newlyPainted)
        {
            Debug.Log($"{name} coverage: {CoveragePercent:F1}%");
        }

        if (IsFullyPainted)
        {
            Debug.Log($"{name} FULLY PAINTED ({CoveragePercent:F1}%)");
            OnPaintCompleted();
        }
    }

    // =============================
    // INTERFACE
    // =============================
    /*
    public float CoveragePercent
    {
        get
        {
            if (samplePoints.Count == 0) return 0f;
            return (float)paintedSamples.Count / samplePoints.Count * 100f;
        }
    }*/

    private void OnPaintCompleted()
    {
        Renderer r = GetComponent<Renderer>();

        // Visual state = UI 100%
        r.material.color = Color.black;

        r.shadowCastingMode = ShadowCastingMode.On;
        r.receiveShadows = true;
        Debug.Log("Painted 100 text");

    }

    public float CoveragePercent
    {
        get
        {
            float requiredSamples =
                (samplePoints.Count * coverageScale) * completionThreshold;

            if (requiredSamples <= 0f)
                return 0f;

            float progress =
                paintedSamples.Count / requiredSamples;

            return Mathf.Clamp01(progress) * 100f;
        }
    }

    public float DisplayPercent
    {
        get
        {
            float requiredSamples =
                (samplePoints.Count * coverageScale) * completionThreshold;

            if (requiredSamples <= 0f)
                return 0f;

            float progress =
                paintedSamples.Count / requiredSamples;

            return Mathf.Clamp01(progress) * 100f;
        }
    }

    /*
    public float DisplayPercent
    {
        get
        {
            return CoveragePercent;
        }
    }'*/
}
