using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ToggleSunFlare
{
    [MenuItem("Tools/Sun/Enable Native Lens Flare")]
    public static void Enable()
    {
        var go = GameObject.Find("SunDisc");
        var lightObject = GameObject.Find("Directional Light");
        var data = AssetDatabase.LoadAssetAtPath<LensFlareDataSRP>(
            "Assets/URP_Flares_Pack/Prefabs/Sun/Sun_Flare_2.asset");
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyBox/SunDiscOcclusionSafe.mat");
        if (go == null || lightObject == null || data == null || material == null)
        {
            Debug.LogError("Sun disc, directional light, flare data, or occlusion-safe material is missing.");
            return;
        }

        var comp = go.GetComponent<LensFlareComponentSRP>();
        if (comp == null)
        {
            comp = Undo.AddComponent<LensFlareComponentSRP>(go);
            comp.intensity = 0.63f;
            comp.scale = 0.84f;
        }
        Undo.RecordObject(comp, "Enable native sun flare");

        comp.lensFlareData = data;
        comp.lightOverride = lightObject.GetComponent<Light>();
        comp.allowOffScreen = true;
        comp.useOcclusion = true;
        comp.sampleCount = 32;
        comp.occlusionOffset = 1f;
        // The sun stays bright at its visual distance; local-light falloff would hide it.
        comp.distanceAttenuationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        comp.scaleByDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        var mesh = go.GetComponent<MeshFilter>();
        var scale = go.transform.lossyScale;
        comp.occlusionRadius = mesh != null && mesh.sharedMesh != null
            ? Vector3.Scale(mesh.sharedMesh.bounds.extents, scale).magnitude / Mathf.Sqrt(3f)
            : 80f;
        comp.enabled = true;

        var renderer = go.GetComponent<Renderer>();
        Undo.RecordObject(renderer, "Prevent sun disc self-occlusion");
        renderer.sharedMaterial = material;
        DisableFlare(lightObject);
        SetFallback(go, false);
        foreach (var root in go.scene.GetRootGameObjects())
        foreach (var camera in root.GetComponentsInChildren<Camera>(true))
        {
            var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null) continue;
            Undo.RecordObject(cameraData, "Enable sun flare camera requirements");
            cameraData.renderPostProcessing = true;
            cameraData.requiresDepthTexture = true;
            EditorUtility.SetDirty(cameraData);
        }
        EditorUtility.SetDirty(comp);
        EditorUtility.SetDirty(renderer);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("[Lunar] Native flare follows SunDisc with terrain occlusion enabled.");
    }

    [MenuItem("Tools/Sun/Use Image Fallback")]
    public static void Disable()
    {
        var go = GameObject.Find("SunDisc");
        if (go == null) return;
        DisableFlare(go);
        DisableFlare(GameObject.Find("Directional Light"));
        SetFallback(go, true);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("[Lunar] Native flare disabled; retained image fallback enabled.");
    }

    static void DisableFlare(GameObject go)
    {
        if (go == null) return;
        var comp = go.GetComponent<LensFlareComponentSRP>();
        if (comp == null) return;
        Undo.RecordObject(comp, "Disable alternate sun flare");
        comp.enabled = false;
        EditorUtility.SetDirty(comp);
    }

    static void SetFallback(GameObject sun, bool active)
    {
        var fallback = sun.transform.Find("SunFlareQuad");
        if (fallback == null) return;
        Undo.RecordObject(fallback.gameObject, "Toggle sun image fallback");
        fallback.gameObject.SetActive(active);
        EditorUtility.SetDirty(fallback.gameObject);
    }
}
