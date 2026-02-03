using UnityEngine;
using UnityEngine.UI;

public class PaintPaletteSavedColoursUI : MonoBehaviour
{
    public PaintPalette palette;
    public Transform buttonContainer;
    public GameObject colorButtonPrefab;

    void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < palette.maxSlots; i++)
        {
            GameObject go = Instantiate(colorButtonPrefab, buttonContainer);

            PaintColour color = i < palette.colors.Count
                ? palette.colors[i]
                : null;

            go.GetComponent<SavedPaintColourButton>()
              .Init(color, palette, i);
        }
    }
}


