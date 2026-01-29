using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshCollider))]
public class PaintCoverage : MonoBehaviour, IPaintCoverage
{
    [Header("Completion")]
    [Range(0f, 100f)]
    public float completionThreshold = 95f;

    [Header("Sub-Triangle Grid")]
    [Range(2, 10)]
    public int gridSize = 3;   // 3×3 feels great

    [Header("Debug")]
    public bool logDebug = true;

    public bool IsComplete { get; private set; }

    // ===== Internal =====
    private Dictionary<int, bool[]> triangleCells = new();

    private int totalTriangles;
    private int cellsPerTriangle;
    private int totalCells;

    [Header("Coverage Tuning")]
    [Tooltip("Scales how much coverage contributes ( >1 = faster, <1 = slower )")]
    public float coverageWeight = 1f;

    // =============================
    // UNITY
    // =============================

    void Awake()
    {
        MeshCollider mc = GetComponent<MeshCollider>();

        if (!mc || !mc.sharedMesh)
        {
            Debug.LogError($"{name} needs a MeshCollider with a mesh.");
            enabled = false;
            return;
        }

        totalTriangles = mc.sharedMesh.triangles.Length / 3;
        cellsPerTriangle = gridSize * gridSize;
        totalCells = totalTriangles * cellsPerTriangle;

        if (logDebug)
        {
            Debug.Log(
                $"{name} | {totalTriangles} triangles | " +
                $"{cellsPerTriangle} cells/tri | {totalCells} total cells"
            );
        }
    }

    // =============================
    // PAINT REGISTRATION (OPTION B)
    // =============================

    public void RegisterPaintHit(RaycastHit hit)
    {
        if (IsComplete)
            return;

        int triIndex = hit.triangleIndex;
        if (triIndex < 0)
            return;

        if (!triangleCells.TryGetValue(triIndex, out var cells))
        {
            cells = new bool[cellsPerTriangle];
            triangleCells[triIndex] = cells;
        }

        Vector3 bc = hit.barycentricCoordinate;

        float x = bc.y;
        float y = bc.z;

        int gx = Mathf.Clamp(Mathf.FloorToInt(x * gridSize), 0, gridSize - 1);
        int gy = Mathf.Clamp(Mathf.FloorToInt(y * gridSize), 0, gridSize - 1);

        int cellIndex = gy * gridSize + gx;

        if (!cells[cellIndex])
        {
            cells[cellIndex] = true;
            UpdateCoverage();
        }
    }

    // =============================
    // COVERAGE
    // =============================

    void UpdateCoverage()
    {
        int painted = 0;

        foreach (var kvp in triangleCells)
        {
            var cells = kvp.Value;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i])
                    painted++;
            }
        }

        CoveragePercent =
     Mathf.Clamp01(((float)painted / totalCells) * coverageWeight) * 100f;


        if (logDebug)
        {
            Debug.Log(
                $"[PaintCoverage] {name} | " +
                $"{CoveragePercent:F1}% | {painted}/{totalCells} cells"
            );
        }

        if (!IsComplete && CoveragePercent >= completionThreshold)
        {
            IsComplete = true;
            OnPaintCompleted();
        }
    }

    // =============================
    // COMPLETION
    // =============================

    void OnPaintCompleted()
    {
        Renderer r = GetComponent<Renderer>();

        if (r)
        {
            r.material.color = Color.black;
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        Debug.Log($"{name} FULLY PAINTED");
    }

    // =============================
    // INTERFACE
    // =============================

    public float CoveragePercent { get; private set; }
    public float DisplayPercent => CoveragePercent;
}
