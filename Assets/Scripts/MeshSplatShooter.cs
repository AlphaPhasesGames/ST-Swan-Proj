using UnityEngine;

public class MeshSplatShooter : MonoBehaviour
{
    [Header("Mesh Splat")]
    public GameObject splatPrefab;

    [Header("Decal")]
    public DecalShooter decalShooter;

    [Header("Settings")]
    public float range = 100f;
    public float surfaceOffset = 0.01f;

    [Tooltip("How much the surface must face the camera to use decals")]
    public float decalFacingThreshold = 35f; // degrees

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // VIEW-BASED classification (THIS IS THE KEY FIX)
            float facingAngle = Vector3.Angle(-transform.forward, hit.normal);

            // Surface facing the player  DECAL
            if (facingAngle < decalFacingThreshold)
            {
                decalShooter.SpawnDecal(hit, transform.forward);
            }
            // Surface turning away / curved  MESH SPLAT
            else
            {
                SpawnSplat(hit);
            }
        }
    }

    void SpawnSplat(RaycastHit hit)
    {
        Vector3 pos = hit.point + hit.normal * surfaceOffset;
        Quaternion rot = Quaternion.LookRotation(-hit.normal);

        GameObject splat = Instantiate(splatPrefab, pos, rot);

        splat.transform.Rotate(0f, 0f, Random.Range(0f, 360f));

        float size = Random.Range(0.25f, 0.45f);
        splat.transform.localScale = Vector3.one * size;

        splat.transform.SetParent(hit.collider.transform, true);
    }
}
