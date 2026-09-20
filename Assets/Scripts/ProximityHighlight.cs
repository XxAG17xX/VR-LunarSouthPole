using UnityEngine;

/// <summary>
/// Pulses an emissive glow on all renderers of this GameObject
/// when the player rig is within range.
/// Uses MaterialPropertyBlock — no extra material instances created.
/// </summary>
public class ProximityHighlight : MonoBehaviour
{
    [Header("Settings")]
    public float highlightRange = 5f;
    public float pulseSpeed     = 2f;
    public float pulseMin       = 0.5f;
    public float pulseMax       = 3f;
    public Color glowColor      = new Color(0.3f, 0.9f, 1f, 1f);

    Transform playerRig;
    Renderer[] renderers;
    MaterialPropertyBlock propBlock;
    bool isGlowing = false;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (rig != null) playerRig = rig.transform;

        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Ensure emission keyword is enabled on all materials
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
                m.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        if (playerRig == null || renderers == null) return;

        float dist = Vector3.Distance(playerRig.position, transform.position);
        bool inRange = dist < highlightRange;

        // Check if mission manager says we already picked it up
        MissionManager mm = FindFirstObjectByType<MissionManager>();
        if (mm != null && mm.IsCarrying())
            inRange = false;

        if (inRange)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            float intensity = Mathf.Lerp(pulseMin, pulseMax, t);

            // Fade in as player approaches
            float proximityFade = 1f - Mathf.Clamp01(dist / highlightRange);
            intensity *= proximityFade;

            Color emission = glowColor * intensity;

            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor(EmissionColor, emission);
                r.SetPropertyBlock(propBlock);
            }
            isGlowing = true;
        }
        else if (isGlowing)
        {
            // Clear emission
            Color black = Color.black;
            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor(EmissionColor, black);
                r.SetPropertyBlock(propBlock);
            }
            isGlowing = false;
        }
    }
}
