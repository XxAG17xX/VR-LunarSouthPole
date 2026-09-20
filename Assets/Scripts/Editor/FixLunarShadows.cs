using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FixLunarShadows : MonoBehaviour
{
    [MenuItem("Tools/Fix Lunar Shadows")]
    static void Fix()
    {
        // Get the active URP asset
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("No URP asset found!");
            return;
        }

        // Use SerializedObject to set shadow distance and cascades
        SerializedObject so = new SerializedObject(urpAsset);

        // Shadow distance - needs to cover your 1km terrain
        SerializedProperty shadowDist = so.FindProperty("m_ShadowDistance");
        if (shadowDist != null) shadowDist.floatValue = 1500f;

        // Shadow cascades - 4 cascades for better distribution
        SerializedProperty cascadeCount = so.FindProperty("m_ShadowCascadeCount");
        if (cascadeCount != null) cascadeCount.intValue = 4;

        // Main light shadow resolution
        SerializedProperty shadowRes = so.FindProperty("m_MainLightShadowmapResolution");
        if (shadowRes != null) shadowRes.intValue = 4096;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(urpAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("Lunar shadow settings applied! Distance=1500, Cascades=4, Resolution=4096");
    }
}
