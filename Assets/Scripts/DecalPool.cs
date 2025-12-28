using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    public GameObject decalPrefab;
    public int poolSize = 300;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject decal = Instantiate(decalPrefab, transform);
            decal.SetActive(false);
            pool.Enqueue(decal);
        }
    }

    public GameObject GetDecal()
    {
        GameObject decal = pool.Dequeue();
        pool.Enqueue(decal);
        decal.SetActive(true);
        return decal;
    }
}
