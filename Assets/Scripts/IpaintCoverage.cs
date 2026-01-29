using UnityEngine;

public interface IPaintCoverage
{
    float CoveragePercent { get; }
    float DisplayPercent { get; }
    bool IsComplete { get; }

    void RegisterPaintHit(RaycastHit hit);
}
