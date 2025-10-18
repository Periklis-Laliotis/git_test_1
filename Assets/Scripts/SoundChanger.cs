using UnityEngine;

public class AreaSoundTrigger : MonoBehaviour
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
    private bool hasEntered = false;

    void Start()
    {
        footstepManager = FindFirstObjectByType<FootstepManager>();
        forestAudioManager = FindFirstObjectByType<ForestAudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasEntered)
        {
            hasEntered = true;
            Debug.Log("Entered mountain area!");

            if (footstepManager != null)
                footstepManager.ChangeFootstepClips(mountainFootsteps);

            StartCoroutine(FadeAudio(forestWind, mountainWind, fadeDuration));

            // ✅ Σταμάτα τους ήχους των πουλιών
        ForestAudioManager forestAudio = FindFirstObjectByType<ForestAudioManager>();
        if (forestAudio != null)
            forestAudio.StopBirds(2f);
        }
    }

    private System.Collections.IEnumerator FadeAudio(AudioSource from, AudioSource to, float duration)
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
