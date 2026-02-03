using UnityEngine;

public class LineDrawingScript : MonoBehaviour
{
    public float attachDistance;
    public GameObject placeMarker;
    private int placedPointCount = 0;
    private GameObject firstMarker;
    private GameObject secondMarker;
    public LineRenderer line;
    private bool isPreviewingLine = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        line.positionCount = 2;
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdatePreviewLine();
    }

    void HandleInput()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        if (placedPointCount >= 2)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        PlaceMarkerForLineDrawing(ray.origin, ray.direction);
    }

    void UpdatePreviewLine()
    {
        if (!isPreviewingLine || firstMarker == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, attachDistance))
        {
            line.SetPosition(0, firstMarker.transform.position);
            line.SetPosition(1, hit.point);
        }
    }

    public void PlaceMarkerForLineDrawing(Vector3 origin, Vector3 dir)
    {
        if (!Physics.Raycast(origin, dir.normalized, out RaycastHit hit, attachDistance))
            return;

        GameObject marker = Instantiate(placeMarker, hit.point, Quaternion.identity);

        if (placedPointCount == 0)
        {
            firstMarker = marker;
            line.enabled = true;
            isPreviewingLine = true;
        }
        else if (placedPointCount == 1)
        {
            secondMarker = marker;
            isPreviewingLine = false;
            UpdateLine(); // lock the line
        }

        placedPointCount++;
    }

    void UpdateLine()
    {
        line.SetPosition(0, firstMarker.transform.position);
        line.SetPosition(1, secondMarker.transform.position);
    }

    float GetLineLength()
    {
        float length = 0f;

        for (int i = 0; i < line.positionCount - 1; i++)
        {
            length += Vector3.Distance(
                line.GetPosition(i),
                line.GetPosition(i + 1)
            );
        }

        return length;
    }


}
