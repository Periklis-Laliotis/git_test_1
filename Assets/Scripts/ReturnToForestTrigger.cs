using UnityEngine;
using System.Collections;

public class ReturnToForestTrigger : MonoBehaviour
{
    [Header("Ambient Wind Sounds")]
    public AudioSource forestWind;
    public AudioSource mountainWind;

    [Header("Footstep Clips")]
    public AudioClip[] forestFootsteps;
    public AudioClip[] mountainFootsteps;

    [Header("Transition Settings")]
    public float fadeDuration = 2f;

    private FootstepManager footstepManager;
    private ForestAudioManager forestAudioManager;
    private bool hasReturned = false;

    void Start()
    {
        footstepManager = FindFirstObjectByType<FootstepManager>();
        forestAudioManager = FindFirstObjectByType<ForestAudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasReturned)
        {
            hasReturned = true;
            Debug.Log("Returned to forest area!");

            // ✅ Άλλαξε ξανά τα footsteps στο δάσος
            if (footstepManager != null)
                footstepManager.ChangeFootstepClips(forestFootsteps);

            // ✅ Fade από mountain -> forest wind
            StartCoroutine(FadeAudio(mountainWind, forestWind, fadeDuration));

            // ✅ Ξεκίνα ξανά τα πουλιά και τα ambiance του δάσους
            if (forestAudioManager != null)
                forestAudioManager.RestartBirds();
        }
    }

    private IEnumerator FadeAudio(AudioSource from, AudioSource to, float duration)
    {
        float time = 0;
        to.volume = 0;
        to.Play();

        float startVolFrom = from.volume;

        while (time < duration)
        {
            from.volume = Mathf.Lerp(startVolFrom, 0, time / duration);
            to.volume = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        from.Stop();
        to.volume = 1;
    }
}
