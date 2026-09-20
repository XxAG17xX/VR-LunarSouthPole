using UnityEngine;

/// <summary>
/// Pulsing emissive ring at the deployment zone.
/// Attach to the DeploymentZone GameObject.
/// Creates a flat torus-like ring from a cylinder primitive.
/// </summary>
public class DeploymentZoneVFX : MonoBehaviour
{
    [Header("Ring Appearance")]
    public Color activeColor   = new Color(0.2f, 0.8f, 1f);
    public Color reachedColor  = new Color(0.2f, 1f, 0.4f);
    public float pulseSpeed    = 1.8f;
    public float minIntensity  = 1.5f;
    public float maxIntensity  = 4.5f;

    MissionManager mission;
    Renderer[] rings;
    Material ringMat;
    Light zoneLight;
    bool wasComplete = false;

    void Start()
    {
        mission = FindFirstObjectByType<MissionManager>();

        // Build ring visuals — two flat cylinders (outer ring + inner ring)
        rings = new Renderer[2];
        float[] radii   = { 3.0f,  2.6f  };
        float[] heights = { 0.08f, 0.08f };

        for (int i = 0; i < 2; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ZoneRing_" + i;
            ring.transform.parent        = transform;
            ring.transform.localPosition = new Vector3(0, 0.05f, 0);
            ring.transform.localScale    = new Vector3(radii[i] * 2f, heights[i], radii[i] * 2f);
            Destroy(ring.GetComponent<Collider>());

            ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            ringMat.EnableKeyword("_EMISSION");
            ringMat.color = activeColor;
            ring.GetComponent<Renderer>().material = ringMat;
            rings[i] = ring.GetComponent<Renderer>();
        }

        // Soft point light
        GameObject lightGO = new GameObject("ZoneLight");
        lightGO.transform.parent        = transform;
        lightGO.transform.localPosition = new Vector3(0, 0.5f, 0);
        zoneLight           = lightGO.AddComponent<Light>();
        zoneLight.type      = LightType.Point;
        zoneLight.color     = activeColor;
        zoneLight.intensity = 2f;
        zoneLight.range     = 8f;
        zoneLight.shadows   = LightShadows.None;
    }

    void Update()
    {
        if (mission == null) return;

        bool complete = mission.GetPhase() == MissionManager.MissionPhase.Complete;
        Color col     = complete ? reachedColor : activeColor;

        float t         = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        foreach (var r in rings)
        {
            if (r == null) continue;
            r.material.SetColor("_EmissionColor", col * intensity);
            r.material.color = col;
        }

        zoneLight.color     = col;
        zoneLight.intensity = Mathf.Lerp(1f, 3f, t);

        // Freeze pulse when complete
        if (complete && !wasComplete)
        {
            wasComplete = true;
            pulseSpeed  = 0.3f;
        }
    }
}
