using UnityEngine;
using UnityEditor;

public class SetupBackgroundMusic
{
    [MenuItem("Tools/Setup Background Music")]
    static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("Meanwhile t:AudioClip");
        if (guids.Length == 0)
        {
            guids = AssetDatabase.FindAssets("t:AudioClip");
        }

        AudioClip clip = null;
        string clipPath = "";
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            clipPath = path;
            break;
        }

        if (clip == null)
        {
            EditorUtility.DisplayDialog("Error", "No audio clip found!", "OK");
            return;
        }

        GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (rig == null)
        {
            EditorUtility.DisplayDialog("Error", "Camera Rig not found!", "OK");
            return;
        }

        BackgroundMusic bgm = rig.GetComponent<BackgroundMusic>();
        if (bgm == null)
            bgm = Undo.AddComponent<BackgroundMusic>(rig);

        bgm.musicClip = clip;
        bgm.volume = 0.25f;

        EditorUtility.SetDirty(bgm);
        EditorUtility.SetDirty(rig);

        EditorUtility.DisplayDialog("Background Music",
            "Clip: " + clipPath + "\nVolume: 0.25\nLoops forever, 2D audio.\n\nCtrl+S to save!", "OK");
    }
}
