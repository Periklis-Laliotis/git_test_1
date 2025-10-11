using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; // χρόνος μεταξύ βημάτων
    public float minSpeed = 0.1f; // ελάχιστη ταχύτητα για να μετράει ως κίνηση

    private CharacterController controller;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f; // 3D ήχος
        audioSource.volume = 0.6f;
    }

    void Update()
    {
        if (controller != null && controller.velocity.magnitude > minSpeed)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer > stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[index]);
        }
    }
}
