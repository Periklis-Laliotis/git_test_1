using UnityEngine;
using System.Collections;

public class WindPhysics : MonoBehaviour
{
    [Header("Wind Setup")]
    public WindZone windZone;
    [Tooltip("Multiplier for wind strength applied to rigidbodies.")]
    public float forceMultiplier = 0.001f;

    private Rigidbody[] targetBodies;
    private Vector3 baseWindDir;

    IEnumerator Start()
    {
        if (!windZone)
            windZone = FindFirstObjectByType<WindZone>();

        yield return new WaitForSeconds(0.2f); // Wait for bridge to finish building

        targetBodies = GetComponentsInChildren<Rigidbody>();
        baseWindDir = windZone.transform.forward.normalized;

        // ✅ Subscribe to ConfigPoller updates
        ConfigPoller.OnConfigUpdated += ApplyConfig;
    }

    void OnDestroy()
    {
        ConfigPoller.OnConfigUpdated -= ApplyConfig;
    }

    void FixedUpdate()
    {
        ApplyWind();
    }

    void ApplyWind()
    {
        if (!windZone || targetBodies == null || targetBodies.Length == 0)
            return;

        Vector3 windDir = windZone.transform.forward.normalized;
        float strength = windZone.windMain * 5f * forceMultiplier;
        strength += Mathf.PerlinNoise(Time.time * windZone.windPulseFrequency, 0f) * windZone.windTurbulence * 20f;
        float pulse = 1f + Mathf.Sin(Time.time * windZone.windPulseFrequency) * windZone.windPulseMagnitude;
        Vector3 windForce = windDir * strength * pulse;

        foreach (var rb in targetBodies)
        {
            if (rb && !rb.isKinematic)
                rb.AddForce(windForce, ForceMode.Acceleration);
        }
    }

    // ✅ When ConfigPoller updates values
    void ApplyConfig(ConfigData config)
    {
        if (!windZone) return;

        // --- Convert 1–5 range into meaningful wind behavior ---
        float normalizedWind = Mathf.Clamp01((config.wind_speed - 1f) / 4f);  // maps 1–5 → 0–1
        float normalizedSway = Mathf.Clamp01((config.sway_effect - 1f) / 4f);

        // Base wind strength (main)
        float targetWindMain = Mathf.Lerp(0.2f, 2.0f, normalizedWind);  // gentle → strong
        float targetTurbulence = Mathf.Lerp(0.1f, 1.5f, normalizedSway);

        // Pulse magnitude logic:
        //  - Levels 1–2 → 0 (steady wind)
        //  - Level 3 → start feeling light gusts
        //  - Level 5 → up to 0.5 pulse magnitude
        float targetPulse = Mathf.Lerp(0f, 0.5f, Mathf.InverseLerp(3f, 5f, config.wind_speed));

        // Apply smoothly
        windZone.windMain = Mathf.Lerp(windZone.windMain, targetWindMain, 0.3f);
        windZone.windTurbulence = Mathf.Lerp(windZone.windTurbulence, targetTurbulence, 0.3f);
        windZone.windPulseMagnitude = Mathf.Lerp(windZone.windPulseMagnitude, targetPulse, 0.3f);

        // Optional: adjust bridge force multiplier to follow same feel
        forceMultiplier = Mathf.Lerp(0.01f, 3f, normalizedWind);

        Debug.Log($"[WindPhysics] Config Applied | Level: {config.wind_speed} | " +
                $"Main: {windZone.windMain:F2} | Turb: {windZone.windTurbulence:F2} | Pulse: {windZone.windPulseMagnitude:F2}");
    }
}