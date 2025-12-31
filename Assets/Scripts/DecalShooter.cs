using System.Collections.Generic;
using UnityEngine;

public class DecalShooter : MonoBehaviour
{
    [Header("Raycast")]
    public float range = 100f;
    public float paintRadius = 0.5f;

    [Header("Spray")]
    public int sprayRays = 1;
    public float sprayAngle = 4f;

    // ===== Shader Paint =====
    const int MAX_PAINT_POINTS = 150;

    class PaintState
    {
        public Material mat;
        public List<Vector4> points = new List<Vector4>();
    }

    private Dictionary<Renderer, PaintState> paintStates =
        new Dictionary<Renderer, PaintState>();


    // ===== Coverage gating =====
    private HashSet<Collider> hitColliders = new HashSet<Collider>();

    void Update()
    {
      //  if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
      //  {
      //      Shoot();
      //  }
    }

    void Shoot()
    {
        // Reset mesh paint lock ONCE per click
        foreach (var mesh in Object.FindObjectsByType<PaintCoverageMesh>(
                     FindObjectsSortMode.None))
        {
            mesh.ResetPaintClick();
        }

        hitColliders.Clear();

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        // Center ray
        TryShootRay(origin, forward);

        // Spray rays
        for (int i = 0; i < sprayRays; i++)
        {
            Vector2 rand = Random.insideUnitCircle;
            Vector3 sprayDir =
                Quaternion.AngleAxis(rand.x * sprayAngle, transform.up) *
                Quaternion.AngleAxis(rand.y * sprayAngle, transform.right) *
                forward;

            TryShootRay(origin, sprayDir);
        }
    }

    void TryShootRay(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            Collider col = hit.collider;

            // ===== MESH COVERAGE (GATED) =====
            var meshCoverage = col.GetComponentInParent<PaintCoverageMesh>();
            if (meshCoverage != null)
            {
                if (hitColliders.Add(col))
                {
                    meshCoverage.RegisterPaintHit(hit);
                }
            }

            // ===== CUBE COVERAGE (UNGATED) =====
            var cubeCoverage = col.GetComponentInParent<PaintCoverage>();
            if (cubeCoverage != null)
            {
                cubeCoverage.RegisterPaint(hit.point, hit.normal, paintRadius);
            }

            // ===== SHADER PAINT (PER OBJECT) =====
            Renderer rend = col.GetComponentInParent<Renderer>();
            AddPaintPoint(rend, hit.point);
        }
    }

    void AddPaintPoint(Renderer rend, Vector3 worldPos)
    {
        if (rend == null)
            return;

        if (!paintStates.TryGetValue(rend, out PaintState state))
        {
            state = new PaintState
            {
                mat = rend.material
            };

            //  IMPORTANT: pre-allocate FULL array size ONCE
            var emptyArray = new Vector4[MAX_PAINT_POINTS];
            state.mat.SetVectorArray("_PaintPoints", emptyArray);
            state.mat.SetInt("_PaintCount", 0);

            paintStates[rend] = state;
        }

        if (state.points.Count >= MAX_PAINT_POINTS)
            return;

        state.points.Add(new Vector4(worldPos.x, worldPos.y, worldPos.z, 0));

        state.mat.SetInt("_PaintCount", state.points.Count);

        // Update ONLY values, not array size
        state.mat.SetVectorArray("_PaintPoints", state.points);
    }


    public int GetBlobCount(Renderer rend)
    {
        if (rend == null)
            return -1;

        if (paintStates.TryGetValue(rend, out var state))
            return state.points.Count;

        return 0;
    }

    public void ClearBlobs(Renderer rend)
    {
        if (rend == null)
            return;

        if (paintStates.TryGetValue(rend, out var state))
        {
            int before = state.points.Count;

            state.points.Clear();
            state.mat.SetInt("_PaintCount", 0);

            Debug.Log(
                $"[PaintComplete] {rend.name} blobs before: {before}  blobs after: 0"
            );
        }
    }

}
