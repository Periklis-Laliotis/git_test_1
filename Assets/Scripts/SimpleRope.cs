using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a physically simulated rope between two points using ConfigurableJoints.
/// </summary>
public class SimpleRope : MonoBehaviour
{
    [Header("Rope Setup")]
    public Transform startPoint;              // The first fixed point
    public Transform endPoint;                // The second fixed point
    public GameObject ropeSegmentPrefab;      // Small cylinder prefab
    public int segmentCount = 10;             // Number of rope segments
    public float ropeSlack = 1.0f;            // >1 adds sag; 1.0 = tight rope
    public float segmentMass = 0.2f;          // Rigidbody mass for each segment

    private List<Rigidbody> segments = new List<Rigidbody>();

    void Start()
    {
        BuildRope();
    }

    void BuildRope()
    {
        if (!startPoint || !endPoint || !ropeSegmentPrefab)
        {
            Debug.LogError("SimpleRope: Missing references!");
            return;
        }

        // 1️⃣ Calculate direction and segment spacing
        Vector3 ropeDir = (endPoint.position - startPoint.position).normalized;
        float totalLength = Vector3.Distance(startPoint.position, endPoint.position) * ropeSlack;
        float segmentLength = totalLength / segmentCount;

        Rigidbody prevBody = null;

        // 2️⃣ Create rope segments
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = startPoint.position + ropeDir * (segmentLength * i);

            // Create segment
            GameObject seg = Instantiate(ropeSegmentPrefab, pos, Quaternion.identity, transform);
            seg.name = $"RopeSegment_{i}";

            // Rigidbody
            Rigidbody rb = seg.GetComponent<Rigidbody>();
            if (rb == null) rb = seg.AddComponent<Rigidbody>();
            rb.mass = segmentMass;

            // Collider
            if (!seg.TryGetComponent<CapsuleCollider>(out CapsuleCollider col))
            {
                col = seg.AddComponent<CapsuleCollider>();
                col.direction = 2; // Z axis
                col.height = segmentLength;
                col.radius = 0.05f;
            }

            // 3️⃣ Connect this segment to previous one
            if (prevBody != null)
            {
                ConfigurableJoint joint = seg.AddComponent<ConfigurableJoint>();
                joint.connectedBody = prevBody;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = new Vector3(0, 0, -segmentLength * 0.5f);

                // Motion limits
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit limit = new SoftJointLimit();
                limit.limit = segmentLength * 0.25f;
                joint.linearLimit = limit;

                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Limited;
            }

            prevBody = rb;
            segments.Add(rb);
        }

        // 4️⃣ Anchor the first and last segments
        AttachToPoint(segments[0], startPoint);
        AttachToPoint(segments[segments.Count - 1], endPoint);
    }

    /// <summary>
    /// Locks the rope end to a static or moving anchor.
    /// </summary>
    void AttachToPoint(Rigidbody segmentBody, Transform anchor)
    {
        Rigidbody anchorRB = anchor.GetComponent<Rigidbody>();
        if (anchorRB == null)
        {
            // Create a static rigidbody at the anchor if it doesn’t have one
            anchorRB = anchor.gameObject.AddComponent<Rigidbody>();
            anchorRB.isKinematic = true;
        }

        ConfigurableJoint joint = segmentBody.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = anchorRB;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
    }
}
