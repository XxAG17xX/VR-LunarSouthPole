using UnityEngine;
using UnityEditor;

public class AssignSkyboxTexture : MonoBehaviour
{
    [MenuItem("Tools/Assign Skybox Texture")]
    static void Assign()
    {
        string matPath = "Assets/SkyBox/LunarSkybox.mat";
        string texPath = "Assets/SkyBox/starfield-night_9cf6f40d-4cec-4a51-bea4-6d4f5fb6c82d.exr";

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        if (mat == null) { Debug.LogError("Material not found: " + matPath); return; }
        if (tex == null) { Debug.LogError("Texture not found: " + texPath); return; }

        mat.shader = Shader.Find("Skybox/Panoramic");
        mat.SetTexture("_MainTex", tex);
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();

        RenderSettings.skybox = mat;
        Debug.Log("Skybox texture assigned and active!");
    }
}
