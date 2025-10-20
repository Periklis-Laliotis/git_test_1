using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlatformMotionProvider : MonoBehaviour
{
    [Header("Probe")]
    public float probeRadius = 0.18f;
    public float probeDistance = 0.4f;
    public LayerMask groundMask = ~0;

    private CharacterController cc;
    private Rigidbody rbUnderfoot;
    private Vector3 lastFootPointWS;
    private bool hadHitLastFrame;

    // Accumulated delta for this frame
    private Vector3 frameDelta;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        frameDelta = Vector3.zero;

        // Position at soles of CC
        Vector3 feet = transform.position + Vector3.down * (cc.height * 0.5f - cc.skinWidth + 0.02f);

        if (Physics.SphereCast(feet, probeRadius, Vector3.down, out RaycastHit hit, probeDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            var rb = hit.rigidbody;
            if (rb != null)
            {
                rbUnderfoot = rb;

                // World-space contact point (slightly lifted to avoid ground jitter)
                Vector3 footPoint = hit.point + Vector3.up * 0.001f;

                // Velocity at that world point (includes angular velocity)
                Vector3 v = rb.GetPointVelocity(footPoint);

                // Convert velocity to delta for this frame (FixedUpdate rate)
                Vector3 delta = v * Time.fixedDeltaTime;

                // If this is the first frame we detected the platform, skip the big jump
                if (!hadHitLastFrame) delta = Vector3.zero;

                frameDelta = delta;
                lastFootPointWS = footPoint;
                hadHitLastFrame = true;
                return;
            }
        }

        // No rigidbody platform beneath
        rbUnderfoot = null;
        hadHitLastFrame = false;
    }

    // Called by your movement script ONCE per frame to fetch the delta
    public Vector3 ConsumeDelta()
    {
        Vector3 d = frameDelta;
        frameDelta = Vector3.zero;
        return d;
    }
}
