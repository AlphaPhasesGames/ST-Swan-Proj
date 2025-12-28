using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LookAtPaintable : MonoBehaviour
{
    public float lookDistance = 10f;

    public GameObject cubeTextObj;


    private PaintCoverage currentTarget;

    public System.Action<PaintCoverage> OnTargetChanged;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        PaintCoverage newTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, lookDistance))
        {
            newTarget =
                hit.collider.GetComponentInParent<PaintCoverage>();
        }

        if (newTarget != currentTarget)
        {
            //  Looked AWAY from something
            if (currentTarget != null && newTarget == null)
            {
                OnLookAway(currentTarget);
              
            }

            //  Looked AT something
            if (newTarget != null)
            {
                OnLookAt(newTarget);
               
            }

            currentTarget = newTarget;
            OnTargetChanged?.Invoke(currentTarget);
        }
    }


    void OnLookAt(PaintCoverage target)
    {
        Debug.Log($"Started looking at {target.name}");

        //  PUT YOUR CUSTOM CODE HERE
        cubeTextObj.gameObject.SetActive(true);
    }

    void OnLookAway(PaintCoverage target)
    {
        Debug.Log($"Stopped looking at {target.name}");

        //  PUT YOUR CUSTOM CODE HERE
        cubeTextObj.gameObject.SetActive(false);
    }

}
