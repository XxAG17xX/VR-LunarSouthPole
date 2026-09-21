using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LensFlareComponentSRP))]
[DefaultExecutionOrder(1000)]
public class SunTerrainOcclusion : MonoBehaviour
{
    LensFlareComponentSRP flare;
    TerrainCollider[] terrainColliders;
    float unoccludedIntensity;

    void OnEnable()
    {
        flare = GetComponent<LensFlareComponentSRP>();
        unoccludedIntensity = flare.intensity;
        terrainColliders = FindObjectsByType<TerrainCollider>(FindObjectsSortMode.None);
    }

    void LateUpdate()
    {
        var camera = Camera.main;
        if (camera == null || flare == null) return;

        // A shared visibility factor keeps both eyes consistent even when XR depth
        // occlusion misses an off-screen ridge. Native depth occlusion remains on.
        float radius = flare.occlusionRadius * 0.7f;
        int visible = 0;
        for (int sample = 0; sample < 5; sample++)
        {
            Vector3 target = transform.position;
            if (sample == 1) target += camera.transform.right * radius;
            if (sample == 2) target -= camera.transform.right * radius;
            if (sample == 3) target += camera.transform.up * radius;
            if (sample == 4) target -= camera.transform.up * radius;
            Vector3 direction = target - camera.transform.position;
            float distance = direction.magnitude;
            if (distance <= Mathf.Epsilon) continue;
            var ray = new Ray(camera.transform.position, direction / distance);
            bool blocked = false;
            foreach (var terrain in terrainColliders)
            {
                if (terrain != null && terrain.enabled && terrain.gameObject.activeInHierarchy
                    && terrain.Raycast(ray, out _, distance))
                {
                    blocked = true;
                    break;
                }
            }
            if (!blocked) visible++;
        }
        flare.intensity = unoccludedIntensity * (visible / 5f);
    }

    void OnDisable()
    {
        if (flare != null) flare.intensity = unoccludedIntensity;
    }
}
