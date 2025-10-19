using UnityEngine;
using System.Collections;

public class AreaSoundManager : MonoBehaviour
{
    public enum AreaType { Forest, Mountain }

    [Header("Trigger Settings")]
    public AreaType targetArea; // ✅ Επιλογή αν είναι δάσος ή βουνό

    [Header("Ambient Wind Sounds")]
    public AudioSource forestWind;
    public AudioSource mountainWind;

    [Header("Wind Volume Settings")]
    [Range(0f, 1f)] public float forestWindVolume = 0.5f;
    [Range(0f, 1f)] public float mountainWindVolume = 1f;

    [Header("Footstep Clips")]
    public AudioClip[] forestFootsteps;
    public AudioClip[] mountainFootsteps;

    [Header("Transition Settings")]
    public float fadeDuration = 2f;

    private FootstepManager footstepManager;
    private ForestAudioManager forestAudioManager;
    private bool isTransitioning = false;

    void Start()
    {
        footstepManager = FindFirstObjectByType<FootstepManager>();
        forestAudioManager = FindFirstObjectByType<ForestAudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(HandleAreaTransition());
        }
    }

    private IEnumerator HandleAreaTransition()
    {
        if (targetArea == AreaType.Mountain)
        {
            Debug.Log("⛰️ Entered Mountain Area");

            // Αλλαγή footstep
            if (footstepManager != null)
                footstepManager.ChangeFootstepClips(mountainFootsteps);

            // Fade forest -> mountain
            yield return StartCoroutine(FadeAudio(forestWind, mountainWind, fadeDuration, forestWindVolume, mountainWindVolume));

            // Σταμάτα πουλιά
            if (forestAudioManager != null)
                forestAudioManager.StopBirds(2f);
        }
        else if (targetArea == AreaType.Forest)
        {
            Debug.Log("🌲 Returned to Forest Area");

            // Αλλαγή footstep
            if (footstepManager != null)
                footstepManager.ChangeFootstepClips(forestFootsteps);

            // Fade mountain -> forest
            yield return StartCoroutine(FadeAudio(mountainWind, forestWind, fadeDuration, mountainWindVolume, forestWindVolume));

            // Ξεκίνα ξανά τα πουλιά
            if (forestAudioManager != null)
                forestAudioManager.RestartBirds();
        }

        yield return new WaitForSeconds(1f);
        isTransitioning = false;
    }

    private IEnumerator FadeAudio(AudioSource from, AudioSource to, float duration, float fromTargetVolume, float toTargetVolume)
    {
        float time = 0;
        to.volume = 0;
        to.Play();

        float startVolFrom = from.volume;

        while (time < duration)
        {
            from.volume = Mathf.Lerp(startVolFrom, 0, time / duration);
            to.volume = Mathf.Lerp(0, toTargetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        from.Stop();
        from.volume = fromTargetVolume;
        to.volume = toTargetVolume;
    }
}
