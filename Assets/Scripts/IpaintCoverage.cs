using UnityEngine;

public interface IPaintCoverage
{
    float CoveragePercent { get; }   // internal / real
    float DisplayPercent { get; }    // what UI should show
    bool IsComplete { get; }
}
