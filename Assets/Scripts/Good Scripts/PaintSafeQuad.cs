using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PaintSafeQuad : MonoBehaviour
{
    [Tooltip("Must match the paint RenderTexture resolution")]
    public int textureSize = 512;

    void Awake()
    {
        Generate();
    }



    void Generate()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "PaintSafeQuad";

        float eps = 0.5f / textureSize;

        // Vertices (unit quad)
        Vector3[] verts =
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
        };

        // UVs inset by half a texel
        Vector2[] uvs =
        {
            new Vector2(eps, eps),
            new Vector2(1f - eps, eps),
            new Vector2(eps, 1f - eps),
            new Vector2(1f - eps, 1f - eps),
        };

        int[] tris =
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
    }
}
