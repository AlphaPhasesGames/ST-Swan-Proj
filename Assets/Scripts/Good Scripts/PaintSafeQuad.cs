using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PaintSafeQuad : MonoBehaviour
{
    void Awake()
    {
        Generate();
    }

    void Generate()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "PaintQuad";

        Vector3[] verts =
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
        };

        //  FULL UV RANGE — corners EXIST again
        Vector2[] uvs =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
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
