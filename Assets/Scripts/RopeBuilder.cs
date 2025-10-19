using UnityEngine;
using System.Collections.Generic;

public class RopeBuilder : MonoBehaviour
{
    [Header("Rope Setup")]
    public Transform startPoint;
    public Transform endPoint;
    public GameObject segmentPrefab;

    [Tooltip("Length of each rope segment (in meters)")]
    public float segmentLength = 0.3f;

    [Header("Physics Settings")]
    public float segmentMass = 0.1f;
    public bool autoConnectEnds = true;

    private readonly List<Rigidbody> segments = new();

    void Start()
    {
        BuildRope();
    }

    void BuildRope()
    {
        if (!segmentPrefab || !startPoint || !endPoint)
        {
            Debug.LogError("RopeBuilder: Missing references!");
            return;
        }

        // Calculate rope geometry
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        int segmentCount = Mathf.Max(2, Mathf.CeilToInt(distance / segmentLength));
        Vector3 dir = (endPoint.position - startPoint.position).normalized;

        Rigidbody prevBody = null;

        for (int i = 0; i < segmentCount; i++)
        {
            // tightly spaced placement (slightly overlapping)
            Vector3 pos = startPoint.position + dir * (segmentLength * 0.98f * i);
            GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.LookRotation(dir), transform);

            Rigidbody rb = seg.GetComponent<Rigidbody>();
            if (rb == null)
                rb = seg.AddComponent<Rigidbody>();

            rb.mass = segmentMass;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 0.8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            segments.Add(rb);

            // Connect with CharacterJoint for stability
            if (prevBody != null)
            {
                CharacterJoint joint = seg.AddComponent<CharacterJoint>();
                joint.connectedBody = prevBody;

                joint.anchor = new Vector3(0, 0, -segmentLength * 0.5f);
                joint.axis = Vector3.right;
                joint.swingAxis = Vector3.up;

                SoftJointLimit limit = new SoftJointLimit { limit = 15f };
                joint.lowTwistLimit = limit;
                joint.highTwistLimit = limit;
                joint.swing1Limit = limit;
                joint.swing2Limit = limit;
            }

            prevBody = rb;
        }

        // Snap last one exactly to end point
        segments[^1].transform.position = endPoint.position;

        // Connect first and last if needed
        if (autoConnectEnds && segments.Count > 1)
        {
            CharacterJoint startJoint = segments[0].gameObject.AddComponent<CharacterJoint>();
            startJoint.connectedBody = startPoint.GetComponent<Rigidbody>();
            startJoint.anchor = Vector3.zero;

            CharacterJoint endJoint = segments[^1].gameObject.AddComponent<CharacterJoint>();
            endJoint.connectedBody = endPoint.GetComponent<Rigidbody>();
            endJoint.anchor = Vector3.zero;
        }
    }
}
