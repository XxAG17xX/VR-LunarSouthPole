using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Tools > Fix Light Limits (Nav Lamps + Floodlights)
/// Bumps BOTH per-object AND total additional light limits in ALL URP assets.
/// Also widens and boosts the 6 surface floodlights around the lander.
/// </summary>
public class FixLightLimits
{
    [MenuItem("Tools/Fix Light Limits (Nav Lamps + Floodlights)")]
    static void Run()
    {
        // ── 1. Bump ALL URP light limits ──
        var urpGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (string guid in urpGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UniversalRenderPipelineAsset urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (urp == null) continue;

            SerializedObject so = new SerializedObject(urp);

            // Per-object additional light limit
            SerializedProperty maxLights = so.FindProperty("m_AdditionalLightsPerObjectLimit");
            if (maxLights != null)
            {
                maxLights.intValue = 48;
                Debug.Log("[FixLights] " + path + " per-object lights = 48");
            }

            // Shadow atlas
            SerializedProperty shadowAtlas = so.FindProperty("m_AdditionalLightsShadowmapResolution");
            if (shadowAtlas != null)
            {
                shadowAtlas.intValue = 4096;
            }

            // Try to find the max additional lights count (total in view)
            // In Unity 6 URP this might be "m_MaxAdditionalLightsCount" or similar
            var iter = so.GetIterator();
            bool found = false;
            while (iter.NextVisible(true))
            {
                string propName = iter.name.ToLower();
                if (propName.Contains("additional") && propName.Contains("light") && 
                    (propName.Contains("max") || propName.Contains("count") || propName.Contains("limit")))
                {
                    if (iter.propertyType == SerializedPropertyType.Integer && iter.intValue < 48)
                    {
                        Debug.Log("[FixLights] Found: " + iter.name + " = " + iter.intValue + " -> 48");
                        iter.intValue = 48;
                        found = true;
                    }
                }
            }

            if (!found)
                Debug.Log("[FixLights] No total light limit property found in " + path + " (may be unlimited by default)");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urp);
        }

        // ── 2. Boost surface floodlights ──
        GameObject landerGO = GameObject.Find("EagleLander");
        if (landerGO != null)
        {
            Vector3 landerPos = landerGO.transform.position;
            GameObject surfaceLamps = GameObject.Find("SurfaceLamps");
            if (surfaceLamps != null)
            {
                int count = surfaceLamps.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform lamp = surfaceLamps.transform.GetChild(i);

                    // Find light component anywhere in children
                    Light[] lights = lamp.GetComponentsInChildren<Light>();
                    foreach (Light lt in lights)
                    {
                        lt.intensity = 25f;
                        lt.range = 80f;
                        lt.shadows = LightShadows.None;
                        
                        if (lt.type == LightType.Spot)
                        {
                            lt.spotAngle = 130f;
                            lt.innerSpotAngle = 80f;
                        }

                        // Aim at lander
                        lt.transform.LookAt(landerPos);

                        EditorUtility.SetDirty(lt);
                        Debug.Log("[FixLights] Boosted " + lt.gameObject.name + " in " + lamp.name 
                            + " intensity=25 range=80 angle=130");
                    }
                }
            }
        }

        // ── 3. Also increase the forward rendering path additional lights ──
        // Find all UniversalRenderer assets and bump their light limits
        var rendererGuids = AssetDatabase.FindAssets("t:ScriptableRendererData");
        foreach (string guid in rendererGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (renderer == null) continue;

            SerializedObject so = new SerializedObject(renderer);
            var iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                string propName = iter.name.ToLower();
                if (propName.Contains("light") && iter.propertyType == SerializedPropertyType.Integer)
                {
                    if (iter.intValue > 0 && iter.intValue < 48)
                    {
                        Debug.Log("[FixLights] Renderer " + path + ": " + iter.name + " = " + iter.intValue + " -> 48");
                        iter.intValue = 48;
                    }
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(renderer);
        }

        EditorUtility.DisplayDialog("Fix Light Limits", 
            "Done!\n\n"
            + "- All URP assets: per-object lights = 48\n"
            + "- Searched renderer assets for additional limits\n"
            + "- Surface floodlights: intensity 25, range 80m, angle 130\n"
            + "- All aimed at lander\n\n"
            + "Ctrl+S to save!", "OK");
    }
}
