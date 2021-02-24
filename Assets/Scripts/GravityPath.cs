using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(LineRenderer))]
public class GravityPath : MonoBehaviour
{
    private PathCreator pathCreator;
    private LineRenderer lineRenderer;
    
    [SerializeField] private int lineSegments = 50;
    [SerializeField] private float curveHeight = 0.1f;
    [SerializeField] private Vector3 handOffset;
    [SerializeField] private Material dimHighlight;
    [SerializeField] private Material brightHighlight;
    
    void Start()
    {
        pathCreator = GetComponent<PathCreator>();
        
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = lineSegments;
    }

    public void SetPath(Vector3 source, Vector3 destination, Vector3 direction, GravityStatus status)
    {
        // TODO: handOffset needs to be adjusted using controller rotation (or just local transform?) so that relative position is maintained.
        source += handOffset;

        Vector3 midpoint = source + (destination - source) / 2;
        float curveAdjustment = Vector3.Distance(source, destination) / (1 / curveHeight);
        midpoint += Vector3.up * curveAdjustment;
        midpoint += direction * curveAdjustment;

        Vector3[] points = {source, midpoint, destination};
        pathCreator.bezierPath = new BezierPath(points);

        lineRenderer.enabled = true;
        if (status == GravityStatus.PRIMED)
        {
            lineRenderer.material = brightHighlight;
        }
        else if (status == GravityStatus.TARGETTED)
        {
            lineRenderer.material = dimHighlight;
        }
        else
        {
            lineRenderer.enabled = false;
            return;
        }

        for (int i = 0; i < lineSegments; i++)
        {
            lineRenderer.SetPosition(i, pathCreator.path.GetPointAtDistance(i / (float) lineSegments * pathCreator.path.length));
        }
    }
}

public enum GravityStatus
{
    NONE,
    TARGETTED,
    PRIMED
}
