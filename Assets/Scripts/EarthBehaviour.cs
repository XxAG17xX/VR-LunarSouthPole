using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class EarthBehaviour : MonoBehaviour
{
    [Header("Sun Reference - drag Directional Light here")]
    public LunarSunRotation sunRotation;

    [Header("Earth Bobbing")]
    [Tooltip("Half-range of bob in world units")]
    public float bobAmplitude = 400f;

    [Header("Bob Tilt (degrees from vertical)")]
    [Range(0f, 45f)]
    public float bobAngle = 20f;

    [Header("Fixed Sky Position")]
    public Vector3 basePosition;

    [Header("Spin")]
    public float spinsPerOrbit = 29.5f;

    private float bobTimer = 0f;
    private Material earthMat;
    private static readonly int SunDirID = Shader.PropertyToID("_SunDirection");

#if UNITY_EDITOR
    private double lastEditorTime = 0;
#endif

    void Awake()
    {
        if (basePosition == Vector3.zero)
            basePosition = new Vector3(3500f, 1800f, 6000f);
    }

    void OnEnable()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            earthMat = Application.isPlaying ? rend.material : rend.sharedMaterial;
#if UNITY_EDITOR
        lastEditorTime = EditorApplication.timeSinceStartup;
#endif
    }

    void Update()
    {
        float dt = Time.deltaTime;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            double now = EditorApplication.timeSinceStartup;
            dt = Mathf.Min((float)(now - lastEditorTime), 0.05f);
            lastEditorTime = now;
        }
#endif
        float orbitPeriod = (sunRotation != null) ? sunRotation.orbitPeriodSeconds : 120f;

        // Spin: Earth rotates spinsPerOrbit times per orbit
        float spinDegsPerSec = 360f * spinsPerOrbit / orbitPeriod;
        transform.Rotate(Vector3.up, spinDegsPerSec * dt, Space.Self);

        // Bob: once per orbit, tilted by bobAngle degrees from vertical
        bobTimer += dt;
        float phase = Mathf.Sin((bobTimer / orbitPeriod) * 2f * Mathf.PI);
        float displacement = phase * bobAmplitude;
        float rad = bobAngle * Mathf.Deg2Rad;
        float bobX = Mathf.Sin(rad) * displacement;
        float bobY = Mathf.Cos(rad) * displacement;
        Vector3 pos = basePosition;
        pos.x += bobX;
        pos.y += bobY;
        transform.position = pos;

        // Day/night terminator
        if (earthMat != null && sunRotation != null)
            earthMat.SetVector(SunDirID, -sunRotation.transform.forward);
    }

    [ContextMenu("Reset to Script Defaults")]
    void ResetToDefaults()
    {
        bobAmplitude = 400f;
        bobAngle = 20f;
        basePosition = new Vector3(3500f, 1800f, 6000f);
        spinsPerOrbit = 29.5f;
        bobTimer = 0f;
        Debug.Log("[EarthBehaviour] Reset done");
    }
}
