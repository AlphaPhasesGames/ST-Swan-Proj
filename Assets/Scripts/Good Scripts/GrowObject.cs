using UnityEngine;

public class GrowObject : MonoBehaviour
{
    public float growAmount = 0.1f;

    public void Grow()
    {
        transform.localScale += new Vector3(growAmount, growAmount, growAmount);
    }
}
