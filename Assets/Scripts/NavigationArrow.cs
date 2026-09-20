using UnityEngine;
using TMPro;

/// <summary>
/// World-space HUD arrow that always points toward the active mission objective.
/// Attach to a child of MissionHUD_Canvas.
/// Shows distance + a rotating arrow indicator.
/// </summary>
public class NavigationArrow : MonoBehaviour
{
    [Header("References - auto-found if blank")]
    public MissionManager missionManager;
    public Transform playerRig;

    [Header("UI")]
    public TextMeshProUGUI arrowLabel;   // the big arrow character
    public TextMeshProUGUI distLabel;

    void Start()
    {
        if (missionManager == null)
            missionManager = FindFirstObjectByType<MissionManager>();
        if (playerRig == null)
        {
            GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
            if (rig != null) playerRig = rig.transform;
        }
    }

    void Update()
    {
        if (missionManager == null || playerRig == null) return;

        MissionManager.MissionPhase phase = missionManager.GetPhase();

        // Pick the target based on phase
        Transform target = null;
        if (phase == MissionManager.MissionPhase.PickUp)
        {
            GameObject seis = GameObject.FindWithTag("MissionProp");
            if (seis != null) target = seis.transform;
        }
        else if (phase == MissionManager.MissionPhase.CarryToZone ||
                 phase == MissionManager.MissionPhase.Deploy)
        {
            GameObject zone = GameObject.FindWithTag("DeploymentZone");
            if (zone != null) target = zone.transform;
        }

        if (target == null || phase == MissionManager.MissionPhase.Complete)
        {
            if (arrowLabel != null) arrowLabel.text = "✓";
            if (distLabel  != null) distLabel.text  = "DEPLOYED";
            return;
        }

        // Flat direction (ignore Y)
        Vector3 toTarget = target.position - playerRig.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        // Angle between player forward and direction to target (flat)
        Vector3 playerFwd = playerRig.forward;
        playerFwd.y = 0f;
        if (playerFwd.sqrMagnitude < 0.001f) playerFwd = Vector3.forward;
        playerFwd.Normalize();

        float angle = Vector3.SignedAngle(playerFwd, toTarget.normalized, Vector3.up);

        // Map angle to an 8-direction arrow character
        string arrow = AngleToArrow(angle);

        if (arrowLabel != null) arrowLabel.text = arrow;
        if (distLabel  != null) distLabel.text  = Mathf.RoundToInt(dist) + " m";
    }

    string AngleToArrow(float angle)
    {
        // Normalise to 0..360
        float a = (angle + 360f) % 360f;
        if (a <  22.5f) return "↑";
        if (a <  67.5f) return "↗";
        if (a < 112.5f) return "→";
        if (a < 157.5f) return "↘";
        if (a < 202.5f) return "↓";
        if (a < 247.5f) return "↙";
        if (a < 292.5f) return "←";
        if (a < 337.5f) return "↖";
        return "↑";
    }
}
