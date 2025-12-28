using UnityEngine;

public class ShootRay : MonoBehaviour
{
    public float range = 100f;
    public GameObject hitMarkerPrefab;
    public float surfaceOffset = 0.01f;
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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Vector3 pos = hit.point + hit.normal * surfaceOffset;
            Quaternion rot = Quaternion.LookRotation(hit.normal);

            Instantiate(hitMarkerPrefab, pos, rot);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * range, Color.yellow, 1f);
        }
    }
}
