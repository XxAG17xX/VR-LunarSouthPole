using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LunarSunRotation : MonoBehaviour
{
    [Header("Sun Orbit")]
    [Range(0f, 10f)] public float sunElevationDegrees = 1.5f;
    public float orbitPeriodSeconds = 120f;
    [Range(0f, 360f)] public float startAzimuth = 0f;

    [Header("Sun Disc")]
    public GameObject sunDiscObject;
    public float sunDiscDistance = 8000f;

    [Header("Skybox Sync")]
    public bool rotateSkybox = true;
    [Range(-360f, 360f)] public float skyboxRotationOffset = 0f;

    float _az;
    static readonly int _RotID = Shader.PropertyToID("_Rotation");

    void OnEnable() { _az = startAzimuth; ApplySunTransform(); }

    void Update()
    {
        _az = (_az + (360f / orbitPeriodSeconds) * Time.deltaTime) % 360f;
        ApplySunTransform();
    }

    void ApplySunTransform()
    {
        transform.rotation = Quaternion.Euler(sunElevationDegrees, _az, 0f);

        if (sunDiscObject != null)
        {
            // KEY FIX: -transform.forward = where light comes FROM (the sky/horizon).
            // transform.forward points where rays travel TO (the ground/opposite side).
            // The LensFlare (SRP) on a Directional Light renders at -forward,
            // so now the disc and flare are co-located on the same horizon point.
            Vector3 camPos = Vector3.zero;
            Camera cam = Camera.main;
            if (cam != null) camPos = cam.transform.position;
            sunDiscObject.transform.position =camPos + (-transform.forward * sunDiscDistance);
            sunDiscObject.transform.LookAt(camPos);;
        }

        if (rotateSkybox && RenderSettings.skybox != null)
        {
            float r = ((-_az + skyboxRotationOffset) % 360f + 360f) % 360f;
            RenderSettings.skybox.SetFloat(_RotID, r);
        }
    }
}
