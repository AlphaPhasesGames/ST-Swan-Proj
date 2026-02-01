using UnityEngine;

public class PaintColourSetter : MonoBehaviour
{
    [Header("Paint Material")]
    public Material paintMaterial;

    [Header("Colours")]
    public Color pink = new Color(1f, 0.3f, 0.7f);
    public Color black = Color.black;

    void Update()
    {
        if (paintMaterial == null) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            paintMaterial.SetColor("Paint Color", pink);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            paintMaterial.SetColor("Paint Color", black);
        }
    }
}