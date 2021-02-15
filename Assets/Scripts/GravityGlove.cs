using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.VR.InteractionSystem;

public class GravityGlove : MonoBehaviour
{
    bool mStarted;
    
    [Header("Probably don't change these")]
    [SerializeField] private LayerMask mLayerMask;
    [SerializeField] private Mesh drawMesh;
    
    [Header("You can change these")]
    [SerializeField] private float distance;
    [SerializeField] private float size;
    private Vector3 center;
    private Vector3 direction;
    private Vector3 scale;

    private GameObject currentThrowable;
    private GameObject previousThrowable;
    private GameObject activeThrowable;
    
    private void Start()
    {
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        mStarted = true;
    }

    private void FixedUpdate()
    {
        currentThrowable = SelectThrowable();
        Debug.Log(currentThrowable.name);
    }

    private GameObject SelectThrowable()
    {
        // TODO: This `direction` will probably have to change when attached to hands
        direction = transform.forward; 
        center = transform.position + direction * distance / 2;
        scale = new Vector3(size, size, distance);
        
        // Get all colliders within the box we've created and loop over each
        Collider[] hitColliders = Physics.OverlapBox(center, scale / 2, transform.rotation, mLayerMask, QueryTriggerInteraction.Ignore);
        GameObject best = null;
        float bestScore = Single.PositiveInfinity;
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject.GetComponent<Throwable>())
            {
                float distanceToDirection = GetDistanceToLine(collider.transform.position, direction);
                if (distanceToDirection <= bestScore)
                {
                    best = collider.gameObject;
                    bestScore = distanceToDirection;
                }
            }
        }

        return best; // Return the found Throwable GameObject here
    }

    private float GetDistanceToLine(Vector3 position, Vector3 line)
    {
        Vector3 vector = position - transform.position;
        Vector3 projection = Vector3.Project(vector, line);
        Vector3 projectionPosition = transform.position + projection;
        return (projectionPosition - position).magnitude;
    }
    
    private Vector3 GetLaunchVelocity(Vector3 start, Vector3 target, float time)
    {
        // This is simpler than I though it would be.
        // We'll know distance between the two points, and we'll know time to get there
        // Horizontal velocity can be determined from that and vertical velocity doesn't matter
        //  as long as it doesn't hit the ground before it reaches the target
        return Vector3.zero;
    }

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (mStarted)
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireMesh(drawMesh, center, transform.rotation, scale);
    }
}
