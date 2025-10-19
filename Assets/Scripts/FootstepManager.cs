using UnityEngine;
using System.Collections;

public class FootstepManager : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; // χρόνος μεταξύ βημάτων
    public float stopThreshold = 0.01f; // κατώφλι για πλήρη στάση

    private CharacterController controller;
    private bool isMoving = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f;
        audioSource.volume = 0.01f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        StartCoroutine(StepLoop());
    }

    private IEnumerator StepLoop()
    {
        Debug.Log("[FootstepManager] StepLoop STARTED");

        while (true)
        {
            float speed = controller != null ? controller.velocity.magnitude : 0f;

            if (speed > stopThreshold)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    Debug.Log("[FootstepManager] Player moving...");
                }

                PlayFootstep();
                yield return new WaitForSeconds(stepInterval);
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    Debug.Log("[FootstepManager] Player stopped.");
                }

                yield return null;
            }
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[index]);
        }
    }

    public void ChangeFootstepClips(AudioClip[] newClips)
    {
        footstepClips = newClips;
    }
}