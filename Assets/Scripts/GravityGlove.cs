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
    [SerializeField] private SteamVR_Input_Sources inputSource;
    [SerializeField] private float distance = 10;
    [SerializeField] private float size = 4;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private float travelTime = 1;
    [SerializeField] private float autoAttachDistance = 0.15f;
    [SerializeField] private float pullActivationSpeed = 1;

    private Vector3 center;
    private Vector3 direction;
    private Vector3 scale;
    private Quaternion rotation;

    private bool isGrabbing;

    private Hand hand;

    private GameObject targettedThrowable;
    private GameObject primedThrowable;
    private GameObject activeThrowable;
    
    private void Start()
    {
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        mStarted = true;

        hand = GetComponent<Hand>();
    }

    private void Update()
    {
        isGrabbing = SteamVR_Actions._default.GrabGrip[inputSource].state || SteamVR_Actions._default.GrabPinch[inputSource].state || Input.GetKey(KeyCode.Mouse0);

        Debug.DrawRay(transform.position, direction * distance);
    }

    private void FixedUpdate()
    {
        // Determine which throwable is currently being pointed at
        if (null == hand)
        {
            targettedThrowable = SelectThrowable();
        }
        else if (null == hand.currentAttachedObject)
        {
            targettedThrowable = SelectThrowable();
        }
        else
        {
            targettedThrowable = null;
        }

        // "Prime" the targetted throwable
        if (null != primedThrowable && isGrabbing)
        {
            // If throwable is primed and player is grabbing, don't change primed throwable
        }
        else if (null != targettedThrowable && isGrabbing)
        {
            // If player is targetting a throwable and grabbing, prime the throwable
            primedThrowable = targettedThrowable;
        }
        else
        {
            primedThrowable = null;
        }

        // Debug display colors
        if (null != targettedThrowable)
        {
            targettedThrowable.GetComponent<MeshRenderer>().material.color = ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        if (null != primedThrowable)
        {
            primedThrowable.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }

        // Launch the throwable
        if (isPulling() && null != primedThrowable && null == activeThrowable)
        {
            primedThrowable.GetComponent<Rigidbody>().velocity = GetLaunchVelocity(primedThrowable.transform.position, transform.position, travelTime);
            activeThrowable = primedThrowable;
            primedThrowable = null;
            StartCoroutine(DeactiveThrowable(travelTime + 0.5f));
            if (null != hand)
            {
                hand.TriggerHapticPulse(10);
            }
        }

        if (null != activeThrowable)
        {
            if (Vector3.Distance(activeThrowable.transform.position, transform.position) <= autoAttachDistance)
            {
                if (SteamVR_Actions._default.GrabGrip[inputSource].state)
                {
                    hand.AttachObject(activeThrowable, GrabTypes.Grip);
                }
                else if (SteamVR_Actions._default.GrabPinch[inputSource].state)
                {
                    hand.AttachObject(activeThrowable, GrabTypes.Pinch);
                }
                activeThrowable = null;
            }
        }
    }

    private IEnumerator DeactiveThrowable(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeThrowable = null;
    }

    private bool isPulling()
    {
        if (null != hand)
        {
            Vector3 velocity, angularVelocity;
            hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            return isGrabbing && 
                velocity.magnitude >= pullActivationSpeed && 
                Vector3.Dot(velocity, (transform.position - primedThrowable.transform.position)) >= 0; // Moving away from throwable
        }
        else
        {
            return Input.GetKey(KeyCode.Space);
        }
    }

    private GameObject SelectThrowable()
    {
        // TODO: Figure out how to set rotation
        direction = Quaternion.AngleAxis(rotationOffset.x, transform.right) * 
            Quaternion.AngleAxis(rotationOffset.y, transform.up) * 
            Quaternion.AngleAxis(rotationOffset.z, transform.forward) * 
            transform.forward; 
        center = transform.position + direction * distance / 2;
        scale = new Vector3(size, size, distance);
        rotation = Quaternion.LookRotation(direction, transform.up);
        
        // Get all colliders within the box we've created and loop over each
        Collider[] hitColliders = Physics.OverlapBox(center, scale / 2, rotation, mLayerMask, QueryTriggerInteraction.Ignore);
        GameObject best = null;
        float bestScore = float.PositiveInfinity;
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
