using UnityEngine;

public static class PaintMixer
{
    public static PaintColour Mix(PaintColour a, PaintColour b, float t)
    {
        Color mixed = Color.Lerp(a.value, b.value, t);

        return new PaintColour(
            mixed,
            GenerateName(a, b)
        );
    }

    static string GenerateName(PaintColour a, PaintColour b)
    {
        return $"{a.name}-{b.name} Mix";
    }
}
