using UnityEngine;

public class PaintBallObject : MonoBehaviour
{
    public Texture2D brushTex;
    public float worldBrushSize = 0.25f;
    public int textureSize = 512;

    [Header("Paint")]
    public Color paintColor = Color.black;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision col)
    {
        ContactPoint contact = col.contacts[0];

        PaintSurfaceBase surface =
            col.collider.GetComponent<PaintSurfaceBase>()
            ?? col.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface)
        {
            Destroy(gameObject);
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(
                contact.point - contact.normal * 0.01f,
                contact.normal,
                out hit,
                0.05f))
        {
            Destroy(gameObject);
            return;
        }

        if (!surface.CanPaintHit(hit, -contact.normal))
        {
            Destroy(gameObject);
            return;
        }

        if (!surface.TryGetPaintUV(hit, out Vector2 uv))
        {
            Destroy(gameObject);
            return;
        }

        float pixelSize = CalculatePixelSize(surface, hit);

        surface.PaintAtUV(uv, brushTex, pixelSize, paintColor);

        Destroy(gameObject);
    }

    float CalculatePixelSize(PaintSurfaceBase surface, RaycastHit hit)
    {
        MeshCollider mc = hit.collider as MeshCollider;
        if (!mc || !mc.sharedMesh)
            return 32f;

        Mesh m = mc.sharedMesh;
        int tri = hit.triangleIndex * 3;
        if (tri + 2 >= m.triangles.Length)
            return 32f;

        Transform t = hit.collider.transform;

        Vector3 v0 = t.TransformPoint(m.vertices[m.triangles[tri]]);
        Vector3 v1 = t.TransformPoint(m.vertices[m.triangles[tri + 1]]);
        Vector3 v2 = t.TransformPoint(m.vertices[m.triangles[tri + 2]]);

        Vector2 uv0 = m.uv[m.triangles[tri]];
        Vector2 uv1 = m.uv[m.triangles[tri + 1]];
        Vector2 uv2 = m.uv[m.triangles[tri + 2]];

        float worldArea = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
        float uvArea = Mathf.Abs(
            (uv1.x - uv0.x) * (uv2.y - uv0.y) -
            (uv2.x - uv0.x) * (uv1.y - uv0.y)
        ) * 0.5f;

        if (uvArea < 0.00001f)
            return 32f;

        float worldPerUV = Mathf.Sqrt(worldArea / uvArea);
        return Mathf.Clamp(
            (worldBrushSize / worldPerUV) * textureSize,
            4f,
            textureSize * 0.5f
        );
    }
}