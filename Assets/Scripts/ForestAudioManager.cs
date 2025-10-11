using System.Collections;
using UnityEngine;

public class ForestAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource windSource;
    public AudioSource birdsSource;

    [Header("Audio Clips")]
    public AudioClip[] birdClips;
    public AudioClip windLoop;

    [Header("Settings")]
    public Vector2 birdDelayRange = new Vector2(5f, 15f);
    public Vector2 birdVolumeRange = new Vector2(0.4f, 0.8f);
    public float windVolume = 0.5f;

    void Start()
    {
        // Start looping wind
        if (windSource && windLoop)
        {
            windSource.clip = windLoop;
            windSource.loop = true;
            windSource.volume = windVolume;
            windSource.Play();
        }

        // Start bird routine
        if (birdsSource && birdClips.Length > 0)
        {
            StartCoroutine(PlayBirdSounds());
        }
    }

    IEnumerator PlayBirdSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(birdDelayRange.x, birdDelayRange.y));

            AudioClip clip = birdClips[Random.Range(0, birdClips.Length)];
            birdsSource.pitch = Random.Range(0.9f, 1.1f);
            birdsSource.volume = Random.Range(birdVolumeRange.x, birdVolumeRange.y);
            birdsSource.PlayOneShot(clip);
        }
    }
}
