using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StickyPlatformCC : MonoBehaviour
{
    private Transform platform;
    private CharacterController targetCC;
    private Vector3 lastPlatformPos;
    private bool following;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        platform = transform.parent != null ? transform.parent : transform;
        lastPlatformPos = platform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        var cc = other.GetComponentInParent<CharacterController>();
        if (cc != null)
        {
            targetCC = cc;
            following = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == targetCC)
        {
            following = false;
            targetCC = null;
        }
    }

    void LateUpdate()
    {
        if (following && targetCC != null)
        {
            // Move the player by the delta movement of the platform this frame
            Vector3 delta = platform.position - lastPlatformPos;
            if (delta.sqrMagnitude > 0.0001f)
                targetCC.Move(delta);
        }

        // Always remember the current platform position
        lastPlatformPos = platform.position;
    }
}
