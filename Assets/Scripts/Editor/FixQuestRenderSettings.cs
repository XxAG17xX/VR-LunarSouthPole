using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FixQuestRenderSettings : MonoBehaviour
{
    [MenuItem("Tools/Fix Quest 3 Render Settings")]
    static void Fix()
    {
        // Fix both Mobile and PC URP assets
        string[] assetPaths = {
            "Assets/Settings/Mobile_RPAsset.asset",
            "Assets/Settings/PC_RPAsset.asset"
        };

        foreach (string path in assetPaths)
        {
            UniversalRenderPipelineAsset urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (urp == null) { Debug.LogWarning("Not found: " + path); continue; }

            SerializedObject so = new SerializedObject(urp);

            // Shadow distance - covers full 1km terrain
            var shadowDist = so.FindProperty("m_ShadowDistance");
            if (shadowDist != null) shadowDist.floatValue = 1500f;

            // 4 cascades
            var cascades = so.FindProperty("m_ShadowCascadeCount");
            if (cascades != null) cascades.intValue = 4;

            // Max shadow resolution
            var shadowRes = so.FindProperty("m_MainLightShadowmapResolution");
            if (shadowRes != null) shadowRes.intValue = 4096;

            // MSAA x4 for antialiasing
            var msaa = so.FindProperty("m_MsaaSampleCount");
            if (msaa != null) msaa.intValue = 4;

            // Render scale 1.0 = full resolution
            var renderScale = so.FindProperty("m_RenderScale");
            if (renderScale != null) renderScale.floatValue = 1.0f;

            // HDR on
            var hdr = so.FindProperty("m_SupportsHDR");
            if (hdr != null) hdr.boolValue = true;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urp);
            Debug.Log("Fixed: " + path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Quest 3 render settings applied to both URP assets!");
    }
}
