using UnityEngine;

public class MaterialTransparencyController : MonoBehaviour
{
    [Header("References")]
    public ConfigPoller configPoller;  // Assign your existing ConfigPoller
    public Material targetMaterial;    // The material you want to control directly

    [Header("Smoothness Settings")]
    [Tooltip("Smoothness value when transparency = 1 (least transparent)")]
    public float maxSmoothness = 0.35f;
    [Tooltip("Smoothness value when transparency = 5 (most transparent)")]
    public float minSmoothness = 0.0f;

    private int lastTransparency = -1;

    void Update()
    {
        if (configPoller == null || configPoller.CurrentConfig == null || targetMaterial == null)
            return;

        int transparency = Mathf.Clamp(configPoller.CurrentConfig.transparency, 1, 5);

        // Only update when it changes
        if (transparency == lastTransparency)
            return;

        lastTransparency = transparency;

        // Map 1→5 to 0.35→0.0
        float smoothness = Mathf.Lerp(maxSmoothness, minSmoothness, (transparency - 1) / 4f);

        targetMaterial.SetFloat("_Smoothness", smoothness);
        Debug.Log($"🪞 Updated material '{targetMaterial.name}' smoothness → {smoothness:F2} (transparency {transparency})");
    }
}
