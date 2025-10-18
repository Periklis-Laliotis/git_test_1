using UnityEngine;

public class BridgeBuilder : MonoBehaviour
{
    [Header("Bridge Setup")]
    public Transform startAnchor;
    public Transform endAnchor;
    public GameObject plankPrefab;
    [Range(2, 100)] public int plankCount = 15;
    public float plankSpacing = 0.6f;

    [Header("Physics")]
    public float plankMass = 5f;
    public bool useConfigurableJoints = false;
    public bool autoConnectEnds = true;

    void Start()
    {
        if (!plankPrefab || !startAnchor || !endAnchor)
        {
            Debug.LogError("BridgeBuilder: Missing references!");
            return;
        }

        BuildBridge();
    }

    void BuildBridge()
    {
        Vector3 dir = (endAnchor.position - startAnchor.position).normalized;
        float totalLength = Vector3.Distance(startAnchor.position, endAnchor.position);
        float spacing = totalLength / plankCount;

        Rigidbody prevBody = null;

        for (int i = 0; i < plankCount; i++)
        {
            Vector3 pos = startAnchor.position + dir * spacing * i;
            Quaternion rot = Quaternion.LookRotation(dir);
            GameObject plank = Instantiate(plankPrefab, pos, rot, transform);
            Rigidbody rb = plank.GetComponent<Rigidbody>();
            rb.mass = plankMass;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.8f;

            // connect to previous
            if (prevBody != null)
            {
                if (useConfigurableJoints)
                {
                    // Left-side configurable joint
                    var cjLeft = plank.AddComponent<ConfigurableJoint>();
                    cjLeft.connectedBody = prevBody;
                    cjLeft.anchor = new Vector3(-0.3f, 0, -0.2f);
                    cjLeft.xMotion = ConfigurableJointMotion.Locked;
                    cjLeft.yMotion = ConfigurableJointMotion.Limited;
                    cjLeft.zMotion = ConfigurableJointMotion.Locked;
                    cjLeft.angularXMotion = ConfigurableJointMotion.Limited;
                    cjLeft.angularYMotion = ConfigurableJointMotion.Free;
                    cjLeft.angularZMotion = ConfigurableJointMotion.Limited;

                    // Right-side configurable joint
                    var cjRight = plank.AddComponent<ConfigurableJoint>();
                    cjRight.connectedBody = prevBody;
                    cjRight.anchor = new Vector3(0.3f, 0, -0.2f);
                    cjRight.xMotion = ConfigurableJointMotion.Locked;
                    cjRight.yMotion = ConfigurableJointMotion.Limited;
                    cjRight.zMotion = ConfigurableJointMotion.Locked;
                    cjRight.angularXMotion = ConfigurableJointMotion.Limited;
                    cjRight.angularYMotion = ConfigurableJointMotion.Free;
                    cjRight.angularZMotion = ConfigurableJointMotion.Limited;
                }
                else
                {
                    // Left hinge joint
                    var hjLeft = plank.AddComponent<HingeJoint>();
                    hjLeft.connectedBody = prevBody;
                    hjLeft.anchor = new Vector3(-0.3f, 0, -0.2f);
                    hjLeft.axis = Vector3.right;
                    hjLeft.useLimits = true;
                    hjLeft.limits = new JointLimits { min = -10f, max = 10f };

                    // Right hinge joint
                    var hjRight = plank.AddComponent<HingeJoint>();
                    hjRight.connectedBody = prevBody;
                    hjRight.anchor = new Vector3(0.3f, 0, -0.2f);
                    hjRight.axis = Vector3.right;
                    hjRight.useLimits = true;
                    hjRight.limits = new JointLimits { min = -10f, max = 10f };
                }
            }

            prevBody = rb;
        }

        // connect first and last to anchors
        if (autoConnectEnds)
        {
            var first = transform.GetChild(0).GetComponent<Rigidbody>();
            var last = transform.GetChild(transform.childCount - 1).GetComponent<Rigidbody>();

            // first joint to start anchor
            var hj1 = first.gameObject.AddComponent<HingeJoint>();
            hj1.connectedBody = startAnchor.GetComponent<Rigidbody>();
            hj1.anchor = new Vector3(0, 0, -0.2f);
            hj1.axis = Vector3.right;

            // last joint to end anchor
            var hj2 = last.gameObject.AddComponent<HingeJoint>();
            hj2.connectedBody = endAnchor.GetComponent<Rigidbody>();
            hj2.anchor = new Vector3(0, 0, 0.2f);
            hj2.axis = Vector3.right;
        }
    }
}
