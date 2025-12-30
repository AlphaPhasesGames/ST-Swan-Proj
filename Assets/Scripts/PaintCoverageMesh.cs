

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
[RequireComponent(typeof(MeshCollider))]
public class PaintCoverageMesh : MonoBehaviour, IPaintCoverage
{

    // ===== Coverage =====
    public float CoveragePercent { get; private set; }

    [Header("Completion")]
    [Range(0f, 100f)]
    public float completionThreshold = 80f;
    private bool paintedThisClick;
    public bool IsComplete { get; private set; }
    public Material shader;
    [Header("Tuning")]
    [SerializeField] private float coverageMultiplier = 230f;
    /*
    public float DisplayCoveragePercent
    {
        get
        {
            return IsComplete ? 100f : CoveragePercent;
        }
    }
    */

    // ===== Internal =====
    private HashSet<int> paintedTriangles = new HashSet<int>();
    private int totalTriangles;


    private void Awake()
    {
        MeshCollider meshCol = GetComponentInParent<MeshCollider>();

        if (!meshCol || !meshCol.sharedMesh)
        {
            Debug.LogError($"{name} needs a MeshCollider with a valid mesh.");
            enabled = false;
            return;
        }

        totalTriangles = meshCol.sharedMesh.triangles.Length / 3;
        shader = GetComponent<Renderer>().material;
        Debug.Log($"{name} totalTriangles = {totalTriangles}");
    }

    private void Start()
    {
        Debug.Log($"PaintCoverageMesh STARTED on {name}");
       
    }


    private void Update()
    {
        Debug.Log("paint material is at " + shader.GetFloat("_PaintStength"));
    }

    public void RegisterPaintHit(RaycastHit hit)
    {
        if (paintedThisClick)
            return;

        int triIndex = hit.triangleIndex;
        if (triIndex < 0)
            return;

        if (paintedTriangles.Add(triIndex))
        {
            paintedThisClick = true;
            UpdateCoverage();
            Debug.Log($"Coverage now: {CoveragePercent:F1}%");
        }
    }


    private void UpdateCoverage()
    {
        CoveragePercent =
            (float)paintedTriangles.Count / totalTriangles * coverageMultiplier;

        if (!IsComplete && CoveragePercent >= completionThreshold)
        {
            IsComplete = true;
           
            OnPaintCompleted();
        }
    }


    private void OnPaintCompleted()
    {
        // Force full black
        shader.SetFloat("_PaintStength", 1);
        shader.SetInt("_PaintCount", 0);

        //  DEBUG blob counts
        DecalShooter shooter = FindObjectOfType<DecalShooter>();
        Renderer rend = GetComponent<Renderer>();


        if (shooter != null && rend != null)
        {
            int before = shooter.GetBlobCount(rend);
            shooter.ClearBlobs(rend);
            int after = shooter.GetBlobCount(rend);

            Debug.Log(
                $"[PaintComplete] {name} blobs before: {before}  blobs after: {after}"
            );
        }
        else
        {
            Debug.LogWarning("[PaintComplete] Shooter or Renderer missing");
        }
    }


    public float DisplayPercent
    {
        get
        {
            // 10% real coverage == 100% UI (by design)
            const float fullAt = 10f;
            return Mathf.Clamp01(CoveragePercent / fullAt) * 100f;
        }
    }
    /*
    public float DisplayPercent
    {
        get
        {
            // Example: 10% real coverage == 100% UI
            const float fullAt = 10f;

            return Mathf.Clamp01(CoveragePercent / fullAt) * 100f;
        }
    }
    */
    public void ResetPaintClick()
    {
        paintedThisClick = false;
    }




}

