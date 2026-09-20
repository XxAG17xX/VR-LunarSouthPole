using UnityEngine;
using UnityEditor;

public static class EagleMaterialSetup
{
    const string MAT   = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_Mat.mat";
    const string BASE  = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_2_BaseColor.png";
    const string NORM  = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_2_Normal.png";
    const string METAL = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_2_Metallic.png";
    const string ROUGH = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_2_Roughness.png";
    const string PACKED = "Assets/Models/Eagle_lander/Eagle_lander/Eagle_2_MetallicSmoothness.png";

    [MenuItem("Tools/Eagle Lander/1 - Create URP Material")]
    static void BuildMaterial()
    {
        // Fix normal map import
        var ni = (TextureImporter)AssetImporter.GetAtPath(NORM);
        if (ni != null && ni.textureType != TextureImporterType.NormalMap)
        { ni.textureType = TextureImporterType.NormalMap; ni.SaveAndReimport(); }

        // Create material
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MAT);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, MAT);
        }

        var tBase  = AssetDatabase.LoadAssetAtPath<Texture2D>(BASE);
        var tNorm  = AssetDatabase.LoadAssetAtPath<Texture2D>(NORM);
        var tMetal = AssetDatabase.LoadAssetAtPath<Texture2D>(METAL);

        if (tBase  != null) mat.SetTexture("_BaseMap", tBase);
        if (tNorm  != null) { mat.SetTexture("_BumpMap", tNorm); mat.EnableKeyword("_NORMALMAP"); }
        if (tMetal != null) { mat.SetTexture("_MetallicGlossMap", tMetal); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); }
        mat.SetFloat("_Metallic", 1f);
        mat.SetFloat("_Smoothness", 0.3f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log("[Eagle] Material saved: " + MAT);

        AssignToLander(mat);
    }

    [MenuItem("Tools/Eagle Lander/2 - Pack Roughness (run after step 1)")]
    static void PackAndApply()
    {
        // Make textures readable
        SetReadable(METAL, true); SetReadable(ROUGH, true);
        AssetDatabase.Refresh();

        var tM = AssetDatabase.LoadAssetAtPath<Texture2D>(METAL);
        var tR = AssetDatabase.LoadAssetAtPath<Texture2D>(ROUGH);
        if (tM == null || tR == null) { Debug.LogError("[Eagle] Textures not found."); return; }

        int w = tM.width, h = tM.height;
        var packed = new Texture2D(w, h, TextureFormat.RGBA32, true);
        var mp = tM.GetPixels(); var rp = tR.GetPixels();
        var op = new Color[mp.Length];
        for (int i = 0; i < mp.Length; i++)
            op[i] = new Color(mp[i].r, 0, 0, 1f - rp[i].r);
        packed.SetPixels(op); packed.Apply();

        string fullPath = Application.dataPath + "/../" + PACKED;
        System.IO.File.WriteAllBytes(fullPath, packed.EncodeToPNG());
        SetReadable(METAL, false); SetReadable(ROUGH, false);
        AssetDatabase.Refresh();

        var mat = AssetDatabase.LoadAssetAtPath<Material>(MAT);
        var tP  = AssetDatabase.LoadAssetAtPath<Texture2D>(PACKED);
        if (mat != null && tP != null)
        {
            mat.SetTexture("_MetallicGlossMap", tP);
            mat.SetFloat("_Smoothness", 1f);
            EditorUtility.SetDirty(mat); AssetDatabase.SaveAssets();
            Debug.Log("[Eagle] Roughness packed. Material updated.");
        }
    }

    static void AssignToLander(Material mat)
    {
        var lander = GameObject.Find("EagleLander");
        if (lander == null) { Debug.LogError("[Eagle] EagleLander not in scene."); return; }
        int count = 0;
        foreach (var r in lander.GetComponentsInChildren<Renderer>(true))
        { r.sharedMaterial = mat; count++; }
        Debug.Log("[Eagle] Material applied to " + count + " renderers.");
    }

    static void SetReadable(string path, bool readable)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        if (ti != null && ti.isReadable != readable)
        { ti.isReadable = readable; ti.SaveAndReimport(); }
    }
}
