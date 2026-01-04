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
    const int MAX_PAINT_POINTS = 1000; // max amount of paint blobs possible before we stop firing

    class PaintState // create class to manage memory for each painted object
    {
        public Material mat; // materials so we can talk to shaders
        // this code remembers where paint dots are in the world. We use vector 4, X Y Z and W axis. The W component is "how much" the object
        // is rotated around that vector. Can be positive or negative and is usually defined in radians instead of degrees.
        public List<Vector4> points = new List<Vector4>(); 
    }
    // we do this code so each object paints independatly and no paint leaks between objects
    private Dictionary<Renderer, PaintState> paintStates =
        new Dictionary<Renderer, PaintState>();


    // ===== Coverage gating ===== //  we do this so we dont get multiple hits per object. Instead one hit per collider
    private HashSet<Collider> hitColliders = new HashSet<Collider>(); 

    void Update() // 
    {
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")) // mouse lmb of X button
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Reset mesh paint lock ONCE per click - Important for coverage counting.
        foreach (var mesh in Object.FindObjectsByType<PaintCoverageMesh>(
                     FindObjectsSortMode.None))
        {
            mesh.ResetPaintClick();
        }

        hitColliders.Clear();// Each new click means fresh rules.

        Vector3 origin = transform.position;//paint comes from The object’s position
        Vector3 forward = transform.forward; // paint comes from the forward direction

        // Center ray
        TryShootRay(origin, forward); // fire TryShootRay Function

        // Spray rays
        for (int i = 0; i < sprayRays; i++) // messy non uniform paint
        {
            Vector2 rand = Random.insideUnitCircle; // This adds a random wobble to the paint
            // This gently rotates the forward direction.
            Vector3 sprayDir =
                Quaternion.AngleAxis(rand.x * sprayAngle, transform.up) *
                Quaternion.AngleAxis(rand.y * sprayAngle, transform.right) *
                forward;

            //Each spray ray tries to paint something.
            TryShootRay(origin, sprayDir);
        }
    }

    void TryShootRay(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range)) // Did we hit something within range?
        {
            Collider col = hit.collider; //  what the paint hits

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
