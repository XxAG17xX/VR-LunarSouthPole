using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// VR Pause Menu v6.
/// TOGGLE: Hold BOTH grip triggers for 1 second. Nothing else uses this combo.
/// SELECT: While menu is open, squeeze ONE grip while pointing at button.
/// SLIDER: Hold one grip while pointing at slider, drag.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public MissionManager missionManager;

    Vector3 spawnPos;
    bool spawnOK = false;
    bool open = false;
    GameObject root;

    // Dual-grip hold detection
    float dualGripTimer = 0f;
    float dualGripThreshold = 1.0f;
    bool  dualGripFired = false;

    float speedMult = 1f;
    float minSpd = 1f;
    float maxSpd = 8f;
    Image fillImg;
    TextMeshProUGUI spdTxt;
    bool dragging = false;
    RectTransform trackRT;

    static readonly Color bgCol   = new Color(0.03f, 0.05f, 0.1f, 0.82f);
    static readonly Color btnCol  = new Color(0.06f, 0.1f, 0.18f, 0.9f);
    static readonly Color cyan    = new Color(0.3f, 0.85f, 1f);
    static readonly Color orange  = new Color(1f, 0.6f, 0.2f);
    static readonly Color green   = new Color(0.3f, 1f, 0.5f);
    static readonly Color wh      = new Color(0.9f, 0.93f, 0.97f, 0.9f);
    static readonly Color slBg    = new Color(0.1f, 0.13f, 0.2f, 0.9f);

    void Start()
    {
        if (missionManager == null) missionManager = FindFirstObjectByType<MissionManager>();
        Debug.Log("[PauseMenu] Ready. Hold BOTH grips 1s to toggle menu.");
    }

    void Update()
    {
        if (!spawnOK) { spawnPos = transform.position; spawnOK = true; }

        // Dual-grip hold detection
        bool leftGrip  = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        bool rightGrip = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        bool bothGrips  = leftGrip && rightGrip;

        if (bothGrips)
        {
            dualGripTimer += Time.unscaledDeltaTime;
            if (dualGripTimer >= dualGripThreshold && !dualGripFired)
            {
                dualGripFired = true;
                Debug.Log("[PauseMenu] Dual grip held! Toggling menu.");
                if (open) Close(); else Open();
            }
        }
        else
        {
            dualGripTimer = 0f;
            dualGripFired = false;
        }

        if (open)
        {
            // Single grip (not both) for menu interaction
            bool singleGripDown = (leftGrip && !rightGrip) || (rightGrip && !leftGrip);
            if (singleGripDown) HandleInteraction();
            
            bool singleGripHeld = singleGripDown;
            if (singleGripHeld) DragSlider();
            else dragging = false;
        }
    }

    void HandleInteraction()
    {
        // Only on press frame — check if grip just went down
        bool lDown = OVRInput.GetDown(OVRInput.RawButton.LHandTrigger);
        bool rDown = OVRInput.GetDown(OVRInput.RawButton.RHandTrigger);
        if (!lDown && !rDown) return;

        RaycastHit hit;
        if (!Shoot(out hit)) return;
        Transform tr = hit.collider.transform;
        while (tr != null)
        {
            if (tr.name == "ResetBtn") { DoReset(); Close(); return; }
            if (tr.name == "ReturnBtn") { DoReturn(); Close(); return; }
            tr = tr.parent;
        }
    }

    void DragSlider()
    {
        if (trackRT == null) return;
        RaycastHit hit;
        if (!Shoot(out hit)) return;
        if (hit.collider.gameObject.name != "Track" && !dragging) return;
        dragging = true;

        Vector3 loc = trackRT.InverseTransformPoint(hit.point);
        Rect r = trackRT.rect;
        float n = Mathf.InverseLerp(r.xMin, r.xMax, loc.x);
        n = Mathf.Clamp01(n);
        speedMult = Mathf.Lerp(minSpd, maxSpd, n);

        if (fillImg != null)
            fillImg.GetComponent<RectTransform>().anchorMax = new Vector2(0.01f + 0.98f * n, 0.9f);
        if (spdTxt != null) spdTxt.text = "x" + speedMult.ToString("F1");

        LunarLocomotion l = GetComponent<LunarLocomotion>();
        if (l != null) l.walkSpeed = 3f * speedMult;
    }

    void Open()
    {
        if (root != null) return;
        open = true;

        Transform eye = null;
        OVRCameraRig rig = GetComponent<OVRCameraRig>();
        if (rig != null) eye = rig.centerEyeAnchor;
        if (eye == null) eye = Camera.main.transform;

        root = new GameObject("PausePanel");
        root.transform.position = eye.position + eye.forward * 0.9f;
        root.transform.rotation = Quaternion.LookRotation(eye.forward, Vector3.up);

        GameObject cGO = new GameObject("C");
        cGO.transform.SetParent(root.transform, false);
        cGO.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
        Canvas cv = cGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace; cv.sortingOrder = 200;
        cGO.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 500);

        RectTransform bg = MakeBG(cGO.GetComponent<RectTransform>());
        MakeTMP("T", bg, "MISSION PAUSED", 28, cyan, 0.05f, 0.87f, 0.95f, 0.97f, TextAlignmentOptions.Center, FontStyles.Bold);
        MakeBtn(bg, "ResetBtn", "RESET MISSION", orange, 0.04f, 0.66f, 0.48f, 0.83f);
        MakeBtn(bg, "ReturnBtn", "RETURN TO LANDER", cyan, 0.52f, 0.66f, 0.96f, 0.83f);
        MakeTMP("SL", bg, "WALK SPEED", 16, wh, 0.04f, 0.52f, 0.4f, 0.63f, TextAlignmentOptions.Left, FontStyles.Bold);

        LunarLocomotion loco = GetComponent<LunarLocomotion>();
        speedMult = loco != null ? loco.walkSpeed / 3f : 1f;
        spdTxt = MakeTMP("SV", bg, "x" + speedMult.ToString("F1"), 16, green, 0.72f, 0.52f, 0.96f, 0.63f, TextAlignmentOptions.Right, FontStyles.Normal);

        GameObject trk = UI("Track", bg);
        trk.AddComponent<Image>().color = slBg;
        trackRT = trk.GetComponent<RectTransform>();
        trackRT.anchorMin = new Vector2(0.04f, 0.38f); trackRT.anchorMax = new Vector2(0.96f, 0.49f);
        trackRT.offsetMin = trackRT.offsetMax = Vector2.zero;
        BoxCollider bc = trk.AddComponent<BoxCollider>();
        bc.size = new Vector3(720f*0.92f*0.0007f, 500f*0.11f*0.0007f, 0.02f);

        GameObject fl = UI("Fill", trackRT);
        fillImg = fl.AddComponent<Image>(); fillImg.color = cyan;
        RectTransform frt = fl.GetComponent<RectTransform>();
        float t = Mathf.InverseLerp(minSpd, maxSpd, speedMult);
        frt.anchorMin = new Vector2(0.01f, 0.1f); frt.anchorMax = new Vector2(0.01f+0.98f*t, 0.9f);
        frt.offsetMin = frt.offsetMax = Vector2.zero;

        MakeTMP("L1", bg, "x1", 10, new Color(0.5f,0.6f,0.7f,0.6f), 0.04f, 0.31f, 0.12f, 0.38f, TextAlignmentOptions.Center, FontStyles.Normal);
        MakeTMP("L4", bg, "x4", 10, new Color(0.5f,0.6f,0.7f,0.6f), 0.46f, 0.31f, 0.54f, 0.38f, TextAlignmentOptions.Center, FontStyles.Normal);
        MakeTMP("L8", bg, "x8", 10, new Color(0.5f,0.6f,0.7f,0.6f), 0.90f, 0.31f, 0.98f, 0.38f, TextAlignmentOptions.Center, FontStyles.Normal);
        MakeTMP("H", bg, "Hold BOTH grips 1s = toggle  |  Point + ONE grip = select/drag",
            10, new Color(0.45f,0.5f,0.55f,0.6f), 0.04f, 0.02f, 0.96f, 0.10f, TextAlignmentOptions.Center, FontStyles.Normal);

        Debug.Log("[PauseMenu] Opened.");
    }

    void Close()
    {
        open = false; dragging = false;
        if (root != null) { Destroy(root); root = null; }
        Debug.Log("[PauseMenu] Closed.");
    }

    bool Shoot(out RaycastHit hit)
    {
        hit = default;
        OVRCameraRig rig = GetComponent<OVRCameraRig>();
        if (rig == null) return false;
        if (rig.rightHandAnchor != null)
        { Ray r = new Ray(rig.rightHandAnchor.position, rig.rightHandAnchor.forward); if (Physics.Raycast(r, out hit, 3f)) return true; }
        if (rig.leftHandAnchor != null)
        { Ray r = new Ray(rig.leftHandAnchor.position, rig.leftHandAnchor.forward); if (Physics.Raycast(r, out hit, 3f)) return true; }
        return false;
    }

    void DoReset()
    {
        Debug.Log("[PauseMenu] Reset!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    void DoReturn()
    {
        Debug.Log("[PauseMenu] Return.");
        LunarLocomotion l = GetComponent<LunarLocomotion>();
        if (l != null) l.TeleportTo(spawnPos);
    }

    // ── UI ──
    RectTransform MakeBG(RectTransform p)
    {
        GameObject g = UI("BG", p); g.AddComponent<Image>().color = bgCol;
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }
    void MakeBtn(RectTransform p, string id, string lbl, Color accent, float x0, float y0, float x1, float y1)
    {
        GameObject g = UI(id, p); g.AddComponent<Image>().color = btnCol;
        RectTransform brt = g.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(x0,y0); brt.anchorMax = new Vector2(x1,y1); brt.offsetMin = brt.offsetMax = Vector2.zero;
        MakeTMP(id+"_L", brt, lbl, 16, wh, 0.05f, 0.05f, 0.95f, 0.95f, TextAlignmentOptions.Center, FontStyles.Bold);
        GameObject b = UI(id+"_A", brt); b.AddComponent<Image>().color = accent;
        RectTransform aRT = b.GetComponent<RectTransform>();
        aRT.anchorMin = new Vector2(0,0.08f); aRT.anchorMax = new Vector2(0.012f,0.92f); aRT.offsetMin = aRT.offsetMax = Vector2.zero;
        BoxCollider bc2 = g.AddComponent<BoxCollider>();
        bc2.size = new Vector3((x1-x0)*720f*0.0007f, (y1-y0)*500f*0.0007f, 0.02f);
    }
    TextMeshProUGUI MakeTMP(string n, RectTransform p, string txt, float sz, Color c,
        float x0, float y0, float x1, float y1, TextAlignmentOptions a, FontStyles fs)
    {
        GameObject g = UI(n, p); TextMeshProUGUI t2 = g.AddComponent<TextMeshProUGUI>();
        t2.text = txt; t2.fontSize = sz; t2.color = c; t2.alignment = a; t2.fontStyle = fs;
        t2.enableWordWrapping = true; t2.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0,y0); rt.anchorMax = new Vector2(x1,y1); rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t2;
    }
    GameObject UI(string n, RectTransform p)
    {
        GameObject g = new GameObject(n); g.transform.SetParent(p, false);
        g.AddComponent<RectTransform>().localScale = Vector3.one; return g;
    }
}
