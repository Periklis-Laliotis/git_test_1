using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BridgePlankMover : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 velocity;
    private Transform player; // the player standing on this plank

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // calculate plank motion delta
        velocity = (transform.position - lastPosition);
        lastPosition = transform.position;

        // move player if standing on it
        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.Move(velocity); // apply same motion to player
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }
}
