using UnityEngine;
using UnityEditor;

public class FixSkyboxTexture : MonoBehaviour
{
    [MenuItem("Tools/Fix Skybox EXR Import")]
    static void Fix()
    {
        string path = "Assets/SkyBox/starfield-night_9cf6f40d-4cec-4a51-bea4-6d4f5fb6c82d.exr";
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) { Debug.LogError("Importer not found at: " + path); return; }

        importer.textureType = TextureImporterType.Default;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.sRGBTexture = false;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("EXR reimported as 2D HDR texture successfully!");
    }
}
