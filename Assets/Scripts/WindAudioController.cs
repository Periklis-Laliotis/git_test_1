using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class WindAudioController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public ConfigPoller configPoller;

    [Header("Audio Settings")]
    public float baseVolume = 0.1f;            // Quiet ambient level
    public float maxVolumeAtWind5 = 1.0f;      // Max volume when wind_speed = 5
    public float fadeSpeed = 2f;

    [Header("Wind Zones (louder near these)")]
    public List<Transform> windyObjects;       // e.g. bridge, mountain, cliffs
    public float influenceRadius = 25f;

    private AudioSource windSource;
    private float targetVolume;

    void Start()
    {
        windSource = GetComponent<AudioSource>();
        if (!windSource.isPlaying)
        {
            windSource.loop = true;
            windSource.Play();
        }

        if (configPoller == null)
            configPoller = FindFirstObjectByType<ConfigPoller>();

        if (player == null && Camera.main != null)
            player = Camera.main.transform;
    }

    void Update()
    {
        if (player == null || windSource == null)
            return;

        float distanceFactor = 0f;

        // Find closest windy object
        foreach (var obj in windyObjects)
        {
            if (obj == null) continue;
            float dist = Vector3.Distance(player.position, obj.position);
            distanceFactor = Mathf.Max(distanceFactor, Mathf.InverseLerp(influenceRadius, 0f, dist));
        }

        // Get current wind speed (1–5)
        float windFactor = 1f;
        if (configPoller != null && configPoller.CurrentConfig != null)
            windFactor = Mathf.Clamp(configPoller.CurrentConfig.wind_speed / 5f, 0f, 1f);

        // Combine effects
        float desiredMaxVolume = Mathf.Lerp(baseVolume, maxVolumeAtWind5, windFactor);
        targetVolume = Mathf.Lerp(baseVolume, desiredMaxVolume, distanceFactor);

        // Smooth fade
        windSource.volume = Mathf.Lerp(windSource.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }
}
