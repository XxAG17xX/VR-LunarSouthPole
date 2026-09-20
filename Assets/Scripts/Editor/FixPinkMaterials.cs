using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// Tools > Fix Pink Materials
/// Creates proper URP Lit material assets and assigns them to:
/// - Surface lamp poles, heads, lenses (6 lamps x 3 parts = 18 renderers)
/// - Seismometer parts (BasePlate, Dome, 3 Legs = 5 renderers)
/// 
/// This fixes the pink (missing shader) issue caused by runtime Shader.Find()
/// not finding URP/Lit when it hasn't been referenced by any scene material.
/// </summary>
public class FixPinkMaterials
{
    [MenuItem("Tools/Fix Pink Materials (Lamps + Seismometer)")]
    static void Run()
    {
        // Ensure folder
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        // ── Create material assets ──
        Material poleMat  = CreateOrLoadMat("Assets/Materials/LampPole.mat", 
            new Color(0.55f, 0.6f, 0.65f), Color.black);
        
        Material headMat  = CreateOrLoadMat("Assets/Materials/LampHead.mat",
            new Color(0.4f, 0.9f, 1f), new Color(0.4f, 0.9f, 1f) * 3f);
        
        Material lensMat  = CreateOrLoadMat("Assets/Materials/LampLens.mat",
            Color.white, new Color(0.4f, 0.9f, 1f) * 6f);

        Material seisMat  = CreateOrLoadMat("Assets/Materials/SeismometerBody.mat",
            new Color(0.7f, 0.72f, 0.68f), Color.black);
        
        Material seisDome = CreateOrLoadMat("Assets/Materials/SeismometerDome.mat",
            new Color(0.85f, 0.85f, 0.8f), Color.black);

        // ── Assign to surface lamps ──
        GameObject surfaceLamps = GameObject.Find("SurfaceLamps");
        if (surfaceLamps != null)
        {
            int fixed_count = 0;
            for (int i = 0; i < surfaceLamps.transform.childCount; i++)
            {
                Transform lamp = surfaceLamps.transform.GetChild(i);
                
                Transform pole = lamp.Find("Pole");
                if (pole != null) { AssignMat(pole, poleMat); fixed_count++; }
                
                Transform head = lamp.Find("Head");
                if (head != null) { AssignMat(head, headMat); fixed_count++; }
                
                Transform lens = lamp.Find("Lens");
                if (lens != null) { AssignMat(lens, lensMat); fixed_count++; }
            }
            Debug.Log("[FixPink] Fixed " + fixed_count + " lamp renderer(s)");
        }

        // ── Assign to seismometer ──
        GameObject seis = GameObject.FindWithTag("MissionProp");
        if (seis == null) seis = GameObject.Find("Seismometer");
        if (seis != null)
        {
            Transform basePlate = seis.transform.Find("BasePlate");
            Transform dome      = seis.transform.Find("Dome");
            
            if (basePlate != null) AssignMat(basePlate, seisMat);
            if (dome != null)      AssignMat(dome, seisDome);

            // Legs
            for (int i = 0; i < 3; i++)
            {
                Transform leg = seis.transform.Find("Leg_" + i);
                if (leg != null) AssignMat(leg, seisMat);
            }
            Debug.Log("[FixPink] Fixed seismometer materials");
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Fix Pink Materials",
            "Created and assigned proper URP material assets:\n\n"
            + "- LampPole.mat (grey metallic)\n"
            + "- LampHead.mat (cyan emissive)\n"
            + "- LampLens.mat (bright cyan emissive)\n"
            + "- SeismometerBody.mat (grey)\n"
            + "- SeismometerDome.mat (light grey)\n\n"
            + "All in Assets/Materials/\n"
            + "Pink should be gone now. Ctrl+S to save!",
            "OK");
    }

    static Material CreateOrLoadMat(string path, Color baseColor, Color emission)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            // Find URP Lit shader properly
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                // Fallback: find any URP shader via search
                string[] guids = AssetDatabase.FindAssets("Lit t:Shader");
                foreach (string guid in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    Shader s = AssetDatabase.LoadAssetAtPath<Shader>(p);
                    if (s != null && s.name.Contains("Universal Render Pipeline/Lit"))
                    {
                        shader = s;
                        break;
                    }
                }
            }

            if (shader == null)
            {
                Debug.LogError("[FixPink] Could not find URP Lit shader! Using Standard.");
                shader = Shader.Find("Standard");
            }

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", baseColor);
        
        if (emission != Color.black)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void AssignMat(Transform obj, Material mat)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            Undo.RecordObject(r, "Fix pink material");
            r.sharedMaterial = mat;
            EditorUtility.SetDirty(r);
        }
    }
}
