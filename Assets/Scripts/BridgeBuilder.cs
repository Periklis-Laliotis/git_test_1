using UnityEngine;

public class BridgeBuilder : MonoBehaviour
{
    [Header("Bridge Setup")]
    public Transform startAnchor;
    public Transform endAnchor;
    public GameObject plankPrefab;
    [Range(2, 50)] public int plankCount = 15;
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
            Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90f, 0);
            GameObject plank = Instantiate(plankPrefab, pos, rot, transform);
            Rigidbody rb = plank.GetComponent<Rigidbody>();
            rb.mass = plankMass;

            // connect to previous
            if (prevBody != null)
            {
                if (useConfigurableJoints)
                {
                    var cj = plank.AddComponent<ConfigurableJoint>();
                    cj.connectedBody = prevBody;
                    cj.xMotion = ConfigurableJointMotion.Locked;
                    cj.yMotion = ConfigurableJointMotion.Limited;
                    cj.zMotion = ConfigurableJointMotion.Locked;
                    cj.angularXMotion = ConfigurableJointMotion.Limited;
                    cj.angularYMotion = ConfigurableJointMotion.Free;
                    cj.angularZMotion = ConfigurableJointMotion.Limited;
                }
                else
                {
                    var hj = plank.AddComponent<HingeJoint>();
                    hj.connectedBody = prevBody;
                    hj.anchor = new Vector3(0, 0, -0.2f);
                    hj.axis = Vector3.right;
                    hj.useLimits = true;
                    JointLimits limits = new JointLimits { min = -10f, max = 10f };
                    hj.limits = limits;
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
