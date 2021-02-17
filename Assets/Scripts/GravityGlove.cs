using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Random;
using Valve.VR;
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
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private float travelTime;
    private Vector3 center;
    private Vector3 direction;
    private Vector3 scale;
    private Quaternion rotation;

    private bool isGrabbing;

    private Hand hand;
    private SteamVR_Action_Boolean grabPinch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "grabPinch");
    private SteamVR_Action_Boolean grabGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "grabGrip");

    private GameObject targettedThrowable;
    private GameObject previousThrowable;
    private GameObject activeThrowable;
    
    private void Start()
    {
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        mStarted = true;

        hand = GetComponent<Hand>();
    }

    private void Update()
    {
        isGrabbing = hand.grabGripAction.state || hand.grabPinchAction.state;
        // isGrabbing = Input.GetKey(KeyCode.Mouse0);
    }

    private void FixedUpdate()
    {
        targettedThrowable = SelectThrowable();

        if (null != targettedThrowable)
        {
            targettedThrowable.GetComponent<MeshRenderer>().material.color = ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        if (isGrabbing && null != targettedThrowable && targettedThrowable != activeThrowable)
        {
            targettedThrowable.GetComponent<Rigidbody>().velocity = GetLaunchVelocity(targettedThrowable.transform.position, transform.position, travelTime);
            activeThrowable = targettedThrowable;
            StartCoroutine(DeactiveThrowable(travelTime + 0.5f));
        }
        isGrabbing = false;
    }

    private IEnumerator DeactiveThrowable(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeThrowable = null;
    }

    private GameObject SelectThrowable()
    {
        // TODO: This `direction` will probably have to change when attached to hands
        direction = Quaternion.Euler(rotationOffset) * transform.forward; 
        center = transform.position + direction * distance / 2;
        scale = new Vector3(size, size, distance);
        //rotation = transform.rotation * Quaternion.Euler(rotationOffset);
        rotation = transform.rotation;
        
        // Get all colliders within the box we've created and loop over each
        Collider[] hitColliders = Physics.OverlapBox(center, scale / 2, rotation, mLayerMask, QueryTriggerInteraction.Ignore);
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
        // Calculate horizontal launch velocity
        Vector3 horizontalOffset = new Vector3(target.x, 0f, target.z) - new Vector3(start.x, 0f, start.z);
        float speed = horizontalOffset.magnitude / time; // Required speed in units/s
        Vector3 horizontalVelocity = horizontalOffset.normalized * speed;

        // Calculate Vertical launch velocity
        float verticalOffset = target.y - start.y;
        Debug.Log(verticalOffset);
        float gravity = Physics.gravity.y;
        float verticalSpeed = Math.Abs(gravity) * (time / 2) + verticalOffset;

        Vector3 verticalVelocity = new Vector3(0f, verticalSpeed, 0f);

        return horizontalVelocity + verticalVelocity;
    }

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (mStarted)
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireMesh(drawMesh, center, rotation, scale);
    }
}
