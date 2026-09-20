using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Run Tools > Mission > Final Setup
/// - Snaps DeploymentZone to terrain
/// - Moves Seismometer inside lander door area
/// - Adds NavigationArrow to HUD
/// - Adds LampChainNavigator to Camera Rig
/// </summary>
public static class MissionSetupFinal
{
    [MenuItem("Tools/Mission/Final Setup")]
    public static void Run()
    {
        // ── 1. Snap DeploymentZone to terrain ──────────────────────
        GameObject zone = GameObject.FindWithTag("DeploymentZone");
        if (zone != null)
        {
            Vector3 snapped = SnapToTerrain(zone.transform.position);
            zone.transform.position = snapped;
            Debug.Log("[MissionSetupFinal] DeploymentZone snapped to Y=" + snapped.y);
        }

        // ── 2. Move Seismometer to just inside lander door ─────────
        GameObject seis   = GameObject.FindWithTag("MissionProp");
        GameObject lander = GameObject.Find("EagleLander");
        if (seis != null && lander != null)
        {
            // Place it on the ground just in front of the lander door
            Vector3 doorOffset = lander.transform.position
                               + lander.transform.forward * 2.5f
                               + lander.transform.right  * 0.5f;
            doorOffset = SnapToTerrain(doorOffset);
            doorOffset.y += 0.3f;
            seis.transform.position = doorOffset;
            Debug.Log("[MissionSetupFinal] Seismometer moved to lander door: " + doorOffset);
        }

        // ── 3. Add NavigationArrow to HUD canvas ───────────────────
        GameObject hudCanvas = GameObject.Find("MissionHUD_Canvas");
        if (hudCanvas != null)
        {
            // Expand canvas height to fit arrow row
            RectTransform crt = hudCanvas.GetComponent<RectTransform>();
            if (crt != null) crt.sizeDelta = new Vector2(600, 200);

            // Arrow row container
            GameObject arrowRow = new GameObject("ArrowRow");
            arrowRow.transform.parent        = hudCanvas.transform;
            arrowRow.transform.localPosition = Vector3.zero;
            arrowRow.transform.localRotation = Quaternion.identity;
            arrowRow.transform.localScale    = Vector3.one;

            // Big arrow character
            GameObject arrowGO = new GameObject("ArrowLabel");
            arrowGO.transform.parent = arrowRow.transform;
            TextMeshProUGUI arrowTMP = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowTMP.text      = "↑";
            arrowTMP.fontSize  = 72;
            arrowTMP.color     = new Color(0.3f, 0.9f, 1f);
            arrowTMP.alignment = TextAlignmentOptions.Center;
            RectTransform art = arrowGO.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.75f, 0f);
            art.anchorMax = new Vector2(1f,    1f);
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;

            // Distance label for arrow
            GameObject distGO = new GameObject("ArrowDistLabel");
            distGO.transform.parent = arrowRow.transform;
            TextMeshProUGUI distTMP = distGO.AddComponent<TextMeshProUGUI>();
            distTMP.text      = "";
            distTMP.fontSize  = 20;
            distTMP.color     = new Color(0.6f, 0.85f, 1f);
            distTMP.alignment = TextAlignmentOptions.BottomRight;
            RectTransform drt = distGO.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0.75f, 0f);
            drt.anchorMax = new Vector2(1f,    0.35f);
            drt.offsetMin = new Vector2(0, 4);
            drt.offsetMax = new Vector2(-8, -4);

            // Wire NavigationArrow script
            NavigationArrow nav = hudCanvas.AddComponent<NavigationArrow>();
            nav.arrowLabel = arrowTMP;
            nav.distLabel  = distTMP;
        }
        else Debug.LogWarning("[MissionSetupFinal] MissionHUD_Canvas not found — run Setup Mission HUD first.");

        // ── 4. Add LampChainNavigator to Camera Rig ────────────────
        GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (rig != null)
        {
            if (rig.GetComponent<LampChainNavigator>() == null)
            {
                LampChainNavigator lcn = rig.AddComponent<LampChainNavigator>();
                // missionManager auto-found at runtime
                Debug.Log("[MissionSetupFinal] LampChainNavigator added to Camera Rig.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[MissionSetupFinal] Done. Press Left Controller Menu button in-game to toggle guide lamp chain.");
    }

    static Vector3 SnapToTerrain(Vector3 pos)
    {
        Vector3 origin = pos;
        origin.y = pos.y + 300f;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 600f))
            return new Vector3(pos.x, hit.point.y, pos.z);
        return pos;
    }
}
