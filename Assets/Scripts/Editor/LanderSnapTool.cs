using UnityEngine;
using UnityEditor;

public static class LanderSnapTool
{
    static float s_footOffset = 1.65f;

    [MenuItem("Tools/Snap Eagle Lander to Terrain")]
    static void RunSnapLander()
    {
        GameObject go = GameObject.Find("EagleLander");
        if (go == null) { Debug.LogError("[Snap] EagleLander not found."); return; }
        PlaceOnTerrain(go, s_footOffset);
    }

    [MenuItem("Tools/Eagle Foot Offset +0.1")]
    static void IncrementOffset() { s_footOffset += 0.1f; RunSnapLander(); }

    [MenuItem("Tools/Eagle Foot Offset -0.1")]
    static void DecrementOffset() { s_footOffset -= 0.1f; RunSnapLander(); }

    static void PlaceOnTerrain(GameObject go, float footOffset)
    {
        Vector3 pos = go.transform.position;
        Ray ray = new Ray(new Vector3(pos.x, pos.y + 500f, pos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
        {
            Undo.RecordObject(go.transform, "Snap to Terrain");
            go.transform.position = new Vector3(pos.x, hit.point.y + footOffset, pos.z);
            Selection.activeGameObject = go;
            Debug.Log("[Snap] Y=" + go.transform.position.y.ToString("F3")
                + " terrain=" + hit.point.y.ToString("F3")
                + " offset=" + footOffset.ToString("F2"));
        }
        else Debug.LogWarning("[Snap] No terrain collider hit below " + go.name);
    }
}