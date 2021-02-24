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
        // Need to adjust this further; should always be a curve somewhat resembling the path
        Vector3 midpoint = source + direction.normalized * Vector3.Distance(source, destination) / 2;
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
