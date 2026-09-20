using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public static class ToggleSunFlare
{
    // Adds SRP Lens Flare for fancy laptop demo (dynamic ghosts + starburst)
    // The billboard quad flare stays untouched
    [MenuItem("Tools/Enable Full Lens Flare (Laptop Demo)")]
    public static void Enable()
    {
        var go = GameObject.Find("Directional Light");
        if (go == null) { Debug.LogError("Directional Light not found"); return; }

        var comp = go.GetComponent<LensFlareComponentSRP>();
        if (comp == null)
            comp = go.AddComponent<LensFlareComponentSRP>();

        var data = AssetDatabase.LoadAssetAtPath<LensFlareDataSRP>(
            "Assets/URP_Flares_Pack/Prefabs/Sun/Sun_Flare_2.asset");
        if (data == null) { Debug.LogError("Sun_Flare_2.asset not found"); return; }

        comp.lensFlareData = data;
        comp.intensity = 3.0f;
        comp.allowOffScreen = true;
        comp.useOcclusion = false;
        comp.enabled = true;
        EditorUtility.SetDirty(comp);
        Debug.Log("[Lunar] Full SRP Lens Flare ON (laptop demo)");
    }

    // Removes SRP Lens Flare component entirely
    // The billboard quad flare stays untouched
    [MenuItem("Tools/Remove Full Lens Flare (Back to VR safe)")]
    public static void Disable()
    {
        var go = GameObject.Find("Directional Light");
        if (go == null) return;

        var comp = go.GetComponent<LensFlareComponentSRP>();
        if (comp != null)
        {
            Object.DestroyImmediate(comp);
            EditorUtility.SetDirty(go);
            Debug.Log("[Lunar] SRP Lens Flare REMOVED (billboard flare still active)");
        }
    }
}
