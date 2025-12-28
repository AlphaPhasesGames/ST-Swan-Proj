using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalShooter : MonoBehaviour
{
    public GameObject decalPrefab;
    public float range = 100f;
    public float surfaceOffset = 0.01f;
    public float angleOffset = 6f; // degrees left/right
    public DecalPool decalPool;
    private HashSet<Collider> hitColliders = new HashSet<Collider>();
    float sharedRotation;
    public float paintRadius = 0.2f;

    void Update()
    {
        sharedRotation = Random.Range(0f, 360f);

        if (Input.GetMouseButtonDown(0))
           Shoot();
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // RESET all mesh paint locks ONCE per click
        foreach (var mesh in Object.FindObjectsByType<PaintCoverageMesh>(
                     FindObjectsSortMode.None))
        {
            mesh.ResetPaintClick();
        }

        hitColliders.Clear();

        int sprayRays = 3;
        float sprayAngle = 4f;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        TryShootRay(origin, forward);

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


    /*
    void Shoot()
    {
        hitColliders.Clear();

        int sprayRays = 3;          // try 6–10
        float sprayAngle = 4f;      // degrees

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        // Always fire the center ray
        TryShootRay(origin, forward);

        // Spray cone
        for (int i = 0; i < sprayRays; i++)
        {
            Vector2 rand = Random.insideUnitCircle;
            Vector3 sprayDir =
                Quaternion.AngleAxis(rand.x * sprayAngle, transform.up) *
                Quaternion.AngleAxis(rand.y * sprayAngle, transform.right) *
                forward;

            TryShootRay(origin, sprayDir);
        }

        foreach (var mesh in Object.FindObjectsByType<PaintCoverageMesh>(FindObjectsSortMode.None))
        {
            mesh.ResetPaintClick();
        }
    }
    */


    void TryShootRay(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            var col = hit.collider;

            // ===== MESH COVERAGE (GATED) =====
            var meshCoverage =
                col.GetComponentInParent<PaintCoverageMesh>();

            if (meshCoverage != null)
            {
                // Only allow ONE mesh registration per click
                if (hitColliders.Add(col))
                {
                    meshCoverage.RegisterPaintHit(hit);
                }
            }

            // ===== CUBE COVERAGE (UNGATED) =====
            var cubeCoverage =
                col.GetComponentInParent<PaintCoverage>();

            if (cubeCoverage != null)
            {
                cubeCoverage.RegisterPaint(hit.point, hit.normal, paintRadius);
            }

            // ===== VISUALS =====
            SpawnDecal(hit, direction);
        }
    }




    /*
        void TryShootRay(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
            {
                // Mesh coverage (already correct)
                var meshCoverage =
                    hit.collider.GetComponentInParent<PaintCoverageMesh>();
                if (meshCoverage != null)
                    meshCoverage.RegisterPaintHit(hit);

                // Cube coverage (ADD HERE)
                var cubeCoverage =
                    hit.collider.GetComponentInParent<PaintCoverage>();
                if (cubeCoverage != null)
                    cubeCoverage.RegisterPaint(hit.point, hit.normal, paintRadius);

                SpawnDecal(hit, direction);
            }
        }


            void TryShootRay(Vector3 origin, Vector3 direction)
            {
                if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
                {
                    SpawnDecal(hit, direction);
                }
            }
        */

    public void SpawnDecal(RaycastHit hit, Vector3 rayDir)
    {
        // Main decal
        SpawnSingleDecal(hit, rayDir);

        // Build surface axes
        Vector3 right = Vector3.Cross(hit.normal, Vector3.up);
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(hit.normal, Vector3.right);

        Vector3 up = Vector3.Cross(right, hit.normal);

        float edgeRayOffset = 0.012f;
        float edgeRayDistance = 0.15f;

        // Fire 4 surface-following rays
        TryEdgeRay(hit, right, edgeRayOffset, edgeRayDistance);
        TryEdgeRay(hit, -right, edgeRayOffset, edgeRayDistance);
        TryEdgeRay(hit, up, edgeRayOffset, edgeRayDistance);
        TryEdgeRay(hit, -up, edgeRayOffset, edgeRayDistance);
    }



    void TryEdgeRay(RaycastHit baseHit, Vector3 dir, float offset, float distance)
    {
        Vector3 origin =
            baseHit.point +
            baseHit.normal * 0.02f +
            dir * offset;

        if (Physics.Raycast(origin, dir, out RaycastHit edgeHit, distance))
        {
            SpawnSingleDecal(edgeHit, dir);
        }
    }


    void SpawnSingleDecal(RaycastHit hit, Vector3 rayDir)
    {
        Vector3 tangent = Vector3.ProjectOnPlane(-rayDir, hit.normal).normalized;

        Vector3 pos =
            hit.point +
            hit.normal * surfaceOffset +
            tangent * 0.01f;

        Quaternion rot = Quaternion.LookRotation(-hit.normal);

        GameObject decal = decalPool.GetDecal();
        decal.transform.SetPositionAndRotation(pos, rot);

        var projector = decal.GetComponent<DecalProjector>();
        projector.fadeFactor = 1f;
        float size = Random.Range(1.1f, 1.5f);
        projector.size = new Vector3(size, size, 0.05f);

        decal.transform.Rotate(0f, 0f, sharedRotation);

        //  THIS IS THE IMPORTANT PART 

    }
}
