using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Tools > Mission > Setup V3
/// 
/// 1. Deletes old floating MissionHUD_Canvas
/// 2. Creates HelmetHUD on CenterEyeAnchor
/// 3. Moves Seismometer inside lander door compartment
/// 4. Adds ProximityHighlight to Seismometer
/// 5. Wires MissionManager + LampChainNavigator references
/// </summary>
public class MissionSetupV3
{
    [MenuItem("Tools/Mission/Setup V3 (Helmet HUD + Fix All)")]
    static void Run()
    {
        // ──────────────────────────────────────────────────────────
        // 0. FIND CORE OBJECTS
        // ──────────────────────────────────────────────────────────
        GameObject cameraRig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (cameraRig == null)
        {
            EditorUtility.DisplayDialog("Error", "Camera Rig not found!", "OK");
            return;
        }

        Transform centerEye = FindDeep(cameraRig.transform, "CenterEyeAnchor");
        if (centerEye == null)
        {
            EditorUtility.DisplayDialog("Error", "CenterEyeAnchor not found!", "OK");
            return;
        }

        GameObject eagleLander = GameObject.Find("EagleLander");
        GameObject seisGO = GameObject.FindWithTag("MissionProp");
        if (seisGO == null)
        {
            // Try by name
            seisGO = GameObject.Find("Seismometer");
        }

        // ──────────────────────────────────────────────────────────
        // 1. DELETE OLD HUD
        // ──────────────────────────────────────────────────────────
        Transform oldCanvas = cameraRig.transform.Find("MissionHUD_Canvas");
        if (oldCanvas != null)
        {
            Undo.DestroyObjectImmediate(oldCanvas.gameObject);
            Debug.Log("[SetupV3] Deleted old MissionHUD_Canvas");
        }

        // ──────────────────────────────────────────────────────────
        // 2. ADD HelmetHUD TO CenterEyeAnchor
        // ──────────────────────────────────────────────────────────
        // Check if already exists
        HelmetHUD existingHUD = centerEye.GetComponentInChildren<HelmetHUD>();
        if (existingHUD != null)
        {
            Undo.DestroyObjectImmediate(existingHUD.gameObject);
        }

        GameObject hudGO = new GameObject("HelmetHUD");
        Undo.RegisterCreatedObjectUndo(hudGO, "Create HelmetHUD");
        hudGO.transform.SetParent(centerEye, false);
        hudGO.transform.localPosition = Vector3.zero;
        hudGO.transform.localRotation = Quaternion.identity;
        HelmetHUD hud = hudGO.AddComponent<HelmetHUD>();
        Debug.Log("[SetupV3] Created HelmetHUD on CenterEyeAnchor");

        // ──────────────────────────────────────────────────────────
        // 3. MOVE SEISMOMETER INTO DOOR COMPARTMENT
        // ──────────────────────────────────────────────────────────
        if (seisGO != null && eagleLander != null)
        {
            // Find door_L to use as reference for the compartment
            Transform doorL = FindDeep(eagleLander.transform, "door_L");
            if (doorL != null)
            {
                // Position seismometer inside the left door bay
                // The bay is behind/below the door panel
                // Use door_L world position, offset slightly into the lander body
                Vector3 doorWorld = doorL.position;
                
                // Offset: into the lander (along lander's -right),
                // slightly below door height, and centered on the bay
                Vector3 bayCenter = doorWorld
                    + eagleLander.transform.up * (-0.3f)       // slightly below door hinge
                    + eagleLander.transform.forward * 0.2f;     // slightly into the bay

                seisGO.transform.position = bayCenter;
                seisGO.transform.rotation = eagleLander.transform.rotation;

                // Disable gravity so it stays put until picked up
                Rigidbody rb = seisGO.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                Debug.Log("[SetupV3] Moved Seismometer into door_L bay at " + bayCenter);
            }
            else
            {
                Debug.LogWarning("[SetupV3] door_L not found — seismometer position unchanged. Move it manually.");
            }

            // ──────────────────────────────────────────────────────
            // 4. ADD ProximityHighlight TO SEISMOMETER
            // ──────────────────────────────────────────────────────
            ProximityHighlight ph = seisGO.GetComponent<ProximityHighlight>();
            if (ph == null)
            {
                ph = Undo.AddComponent<ProximityHighlight>(seisGO);
            }
            ph.highlightRange = 5f;
            Debug.Log("[SetupV3] ProximityHighlight added to Seismometer");
        }

        // ──────────────────────────────────────────────────────────
        // 5. WIRE MISSION MANAGER
        // ──────────────────────────────────────────────────────────
        MissionManager mm = Object.FindFirstObjectByType<MissionManager>();
        if (mm == null)
        {
            GameObject mmGO = GameObject.Find("MissionManager");
            if (mmGO == null)
            {
                mmGO = new GameObject("MissionManager");
                Undo.RegisterCreatedObjectUndo(mmGO, "Create MissionManager");
            }
            mm = mmGO.GetComponent<MissionManager>();
            if (mm == null)
                mm = Undo.AddComponent<MissionManager>(mmGO);
        }
        mm.playerRig = cameraRig.transform;
        mm.seismometer = seisGO;
        GameObject dzGO = GameObject.FindWithTag("DeploymentZone");
        if (dzGO != null)
            mm.deploymentZone = dzGO.transform;

        // Wire HUD -> MissionManager
        hud.missionManager = mm;

        // Wire LampChainNavigator -> MissionManager
        LampChainNavigator lcn = cameraRig.GetComponent<LampChainNavigator>();
        if (lcn != null)
            lcn.missionManager = mm;

        Debug.Log("[SetupV3] MissionManager wired.");

        // ──────────────────────────────────────────────────────────
        // 6. MARK DIRTY + SAVE
        // ──────────────────────────────────────────────────────────
        EditorUtility.SetDirty(mm);
        EditorUtility.SetDirty(hud);
        if (seisGO != null) EditorUtility.SetDirty(seisGO);

        string msg = "Setup V3 Complete!\n\n"
            + "- Old floating HUD deleted\n"
            + "- HelmetHUD created on CenterEyeAnchor\n"
            + "- Seismometer moved into door bay\n"
            + "- ProximityHighlight added (cyan pulse within 5m)\n"
            + "- MissionManager + LampChainNavigator wired\n\n"
            + "CONTROLS:\n"
            + "  Grip = pick up / drop seismometer\n"
            + "  B (hold 2s) = deploy at zone\n"
            + "  Menu (left) = toggle navigation lamps\n\n"
            + "If seismometer position isn't perfect inside the bay,\n"
            + "just drag it manually in Scene view.\n\n"
            + "Press Ctrl+S to save!";

        EditorUtility.DisplayDialog("Mission Setup V3", msg, "OK");
    }

    static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
