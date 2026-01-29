
using UnityEngine;
using TMPro;

public class PaintCoverageUI : MonoBehaviour
{
    public TMP_Text coverageText;

    [Header("Raycast")]
    public Camera playerCamera;
   public float rayDistance = 10f;
  //  public LayerMask paintableLayers;

    [Header("Display Mapping")]
    //public float realPercentForFullUI = 10f;

    private IPaintCoverage currentTarget;

    void Update()
    {
        UpdateTargetFromLook();
        UpdateUI();
    }


    void UpdateTargetFromLook()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            IPaintCoverage found =
                hit.collider.GetComponentInParent<IPaintCoverage>();

            if (found != null)
            {
                currentTarget = found;
                return;
            }
        }

        currentTarget = null;
    }

    /*
    void UpdateTargetFromLook()
    {
        Ray ray = new Ray(playerCamera.transform.position,
                          playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            IPaintCoverage found =
                hit.collider.GetComponent<IPaintCoverage>();

            if (found != null)
            {
                currentTarget = found;
                return;
            }
        }

        currentTarget = null;
    }
    */
    void UpdateUI()
    {
        if (coverageText == null)
            return;

        if (currentTarget == null)
        {
            coverageText.text = "";
            return;
        }

        coverageText.text = $"{currentTarget.DisplayPercent:F1}%";
    }
}