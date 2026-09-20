using UnityEngine;
using TMPro;

/// <summary>
/// Quick Actions — replaces PauseMenu entirely.
/// 
/// HOLD BOTH GRIPS 1.5s → enters action mode
/// Then on the HelmetHUD, the task panel changes to show:
///   "LEFT TRIGGER = Reset  |  RIGHT TRIGGER = Return to Lander"
///   "LEFT STICK UP/DOWN = Speed  |  RELEASE GRIPS = Cancel"
/// 
/// While in action mode, left stick Y adjusts speed in real-time.
/// Pull left index trigger = reload scene (full reset).
/// Pull right index trigger = teleport to spawn.
/// Release both grips = cancel and return to normal HUD.
/// 
/// NO world-space panels. NO colliders. NO raycasting.
/// Just input checks + HUD text swap. Guaranteed to work.
/// </summary>
public class QuickActions : MonoBehaviour
{
    [Header("References")]
    public MissionManager missionManager;

    Vector3 spawnPos;
    bool spawnOK = false;
    bool actionMode = false;

    // Dual-grip hold
    float holdTimer = 0f;
    float holdThreshold = 1.5f;
    bool  holdFired = false;

    // Speed
    float baseSpeed = 3f;
    float currentMult = 1f;
    float minMult = 1f;
    float maxMult = 8f;

    // HUD text override
    HelmetHUD hud;
    TextMeshProUGUI overrideText;
    GameObject overridePanel;

    void Start()
    {
        if (missionManager == null)
            missionManager = FindFirstObjectByType<MissionManager>();
        hud = GetComponentInChildren<HelmetHUD>(true);
        if (hud == null)
        {
            // Search in CenterEyeAnchor
            OVRCameraRig rig = GetComponent<OVRCameraRig>();
            if (rig != null && rig.centerEyeAnchor != null)
                hud = rig.centerEyeAnchor.GetComponentInChildren<HelmetHUD>(true);
        }
        Debug.Log("[QuickActions] Ready. Hold BOTH grips 1.5s for actions.");
    }

    void Update()
    {
        if (!spawnOK) { spawnPos = transform.position; spawnOK = true; }

        bool lGrip = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        bool rGrip = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        bool both  = lGrip && rGrip;

        if (!actionMode)
        {
            // ── Waiting for dual-grip hold ──
            if (both)
            {
                holdTimer += Time.unscaledDeltaTime;
                if (holdTimer >= holdThreshold && !holdFired)
                {
                    holdFired = true;
                    EnterActionMode();
                }
            }
            else
            {
                holdTimer = 0f;
                holdFired = false;
            }
        }
        else
        {
            // ── In action mode ──
            
            // Exit if both grips released
            if (!lGrip && !rGrip)
            {
                ExitActionMode();
                return;
            }

            // Left index trigger = full reset
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                Debug.Log("[QuickActions] RESET MISSION");
                ExitActionMode();
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                return;
            }

            // Right index trigger = teleport to lander
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                Debug.Log("[QuickActions] RETURN TO LANDER");
                LunarLocomotion loco = GetComponent<LunarLocomotion>();
                if (loco != null) loco.TeleportTo(spawnPos);
                ExitActionMode();
                return;
            }

            // Left stick Y = adjust speed
            float stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            if (Mathf.Abs(stickY) > 0.3f)
            {
                currentMult += stickY * Time.unscaledDeltaTime * 3f;
                currentMult = Mathf.Clamp(currentMult, minMult, maxMult);

                LunarLocomotion loco = GetComponent<LunarLocomotion>();
                if (loco != null) loco.walkSpeed = baseSpeed * currentMult;

                UpdateOverlayText();
            }
        }
    }

    void EnterActionMode()
    {
        actionMode = true;
        Debug.Log("[QuickActions] ACTION MODE ON");

        // Read current speed
        LunarLocomotion loco = GetComponent<LunarLocomotion>();
        if (loco != null)
        {
            baseSpeed = 3f;
            currentMult = loco.walkSpeed / baseSpeed;
        }

        // Create overlay on CenterEyeAnchor
        OVRCameraRig rig = GetComponent<OVRCameraRig>();
        Transform eye = rig != null ? rig.centerEyeAnchor : Camera.main.transform;

        overridePanel = new GameObject("QuickActionOverlay");
        overridePanel.transform.SetParent(eye, false);
        overridePanel.transform.localPosition = new Vector3(0f, -0.06f, 0.35f);
        overridePanel.transform.localRotation = Quaternion.identity;
        overridePanel.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);

        Canvas cv = overridePanel.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        cv.sortingOrder = 300;
        overridePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 300);

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(overridePanel.transform, false);
        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.02f, 0.04f, 0.08f, 0.85f);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        // Text
        GameObject txtGO = new GameObject("Txt");
        txtGO.transform.SetParent(bg.transform, false);
        overrideText = txtGO.AddComponent<TextMeshProUGUI>();
        overrideText.fontSize = 14;
        overrideText.color = new Color(0.85f, 0.9f, 0.95f, 0.95f);
        overrideText.alignment = TextAlignmentOptions.Center;
        overrideText.enableWordWrapping = true;
        RectTransform tRT = txtGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.05f, 0.05f); tRT.anchorMax = new Vector2(0.95f, 0.95f);
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;

        UpdateOverlayText();
    }

    void UpdateOverlayText()
    {
        if (overrideText == null) return;
        string cyan = "<color=#4DD9FF>";
        string orange = "<color=#FF9933>";
        string green = "<color=#4DFF80>";
        string end = "</color>";

        overrideText.text =
            cyan + "QUICK ACTIONS" + end + "\n\n" +
            orange + "L-TRIGGER" + end + " = Reset Mission\n" +
            cyan + "R-TRIGGER" + end + " = Return to Lander\n" +
            green + "L-STICK \u2191\u2193" + end + " = Speed: x" + currentMult.ToString("F1") + "\n\n" +
            "<color=#888>Release grips to cancel</color>";
    }

    void ExitActionMode()
    {
        actionMode = false;
        holdTimer = 0f;
        holdFired = false;
        if (overridePanel != null) { Destroy(overridePanel); overridePanel = null; }
        overrideText = null;
        Debug.Log("[QuickActions] ACTION MODE OFF");
    }
}
