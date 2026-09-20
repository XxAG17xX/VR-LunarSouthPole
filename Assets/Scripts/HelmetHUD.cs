using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Visor HUD v7 — single compact minibar, bottom-right.
/// TOGGLE VISIBILITY: Press X on left controller.
/// Starts HIDDEN — press X to show.
/// </summary>
public class HelmetHUD : MonoBehaviour
{
    [Header("Auto-wired")]
    public MissionManager missionManager;

    [Header("Positioning")]
    public float dist = 0.34f;
    public float dropY = -0.04f;
    public float shiftX = 0.06f;

    Canvas canvas;
    GameObject hudRoot;
    TextMeshProUGUI taskTxt, distTxt, arrowTxt, invTxt, hintTxt, deployLbl;
    Image mainBG, invIcon, deployBar, deployBarBG;
    Transform playerRig;

    bool hudVisible = true;
    bool xWasDown = false;

    static readonly Color bgDark  = new Color(0.02f, 0.04f, 0.08f, 0.85f);
    static readonly Color cyan    = new Color(0.3f, 0.85f, 1f);
    static readonly Color green   = new Color(0.3f, 1f, 0.5f);
    static readonly Color white95 = new Color(0.95f, 0.96f, 0.98f, 1f);
    static readonly Color dimTxt  = new Color(0.6f, 0.65f, 0.7f, 0.8f);
    static readonly Color invOff  = new Color(0.08f, 0.12f, 0.2f, 0.85f);
    static readonly Color invOn   = new Color(0.06f, 0.25f, 0.15f, 0.9f);
    static readonly Color acBar   = new Color(0.3f, 0.85f, 1f, 0.6f);

    void Awake()
    {
        if (missionManager == null) missionManager = FindFirstObjectByType<MissionManager>();
        GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (rig != null) playerRig = rig.transform;

        hudRoot = new GameObject("HUD_C");
        hudRoot.transform.SetParent(transform, false);
        hudRoot.transform.localPosition = new Vector3(shiftX, dropY, dist);
        hudRoot.transform.localRotation = Quaternion.identity;
        hudRoot.transform.localScale = new Vector3(0.00065f, 0.00065f, 0.00065f);
        canvas = hudRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        hudRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 200);
        hudRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;

        RectTransform root = hudRoot.GetComponent<RectTransform>();

        mainBG = MakePanel(root, "Main", bgDark, 0f, 0f, 1f, 1f);
        RectTransform mbg = mainBG.GetComponent<RectTransform>();

        MakePanel(mbg, "AccBar", acBar, 0f, 0.05f, 0.012f, 0.95f);

        taskTxt = Txt("Task", mbg, "TASK", 16, cyan,
            0.03f, 0.62f, 0.75f, 0.95f, TextAlignmentOptions.Left, FontStyles.Bold);
        arrowTxt = Txt("Arrow", mbg, "\u2191", 22, cyan,
            0.78f, 0.55f, 0.97f, 0.95f, TextAlignmentOptions.Center, FontStyles.Bold);
        distTxt = Txt("Dist", mbg, "0 m", 13, white95,
            0.03f, 0.38f, 0.5f, 0.60f, TextAlignmentOptions.Left, FontStyles.Normal);
        invIcon = MakePanel(mbg, "InvBG", invOff, 0.52f, 0.35f, 0.75f, 0.60f);
        RectTransform ibg = invIcon.GetComponent<RectTransform>();
        invTxt = Txt("InvT", ibg, "EMPTY", 10, white95,
            0.08f, 0.05f, 0.92f, 0.95f, TextAlignmentOptions.Center, FontStyles.Bold);
        hintTxt = Txt("Hint", mbg, "", 8, dimTxt,
            0.03f, 0.05f, 0.97f, 0.32f, TextAlignmentOptions.Left, FontStyles.Normal);

        deployBarBG = MakePanel(root, "DepBG",
            new Color(0.04f, 0.06f, 0.12f, 0.9f), 0f, 1.05f, 1f, 1.35f);
        RectTransform dbg = deployBarBG.GetComponent<RectTransform>();
        GameObject barGO = UIgo("Bar", dbg);
        deployBar = barGO.AddComponent<Image>(); deployBar.color = green;
        RectTransform brt = barGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.02f, 0.15f);
        brt.anchorMax = new Vector2(0.02f, 0.85f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        deployLbl = Txt("DepL", dbg, "HOLD [B]", 11, white95,
            0.05f, 0.05f, 0.95f, 0.95f, TextAlignmentOptions.Center, FontStyles.Bold);
        deployBarBG.gameObject.SetActive(false);
    }

    void Update()
    {
        // Toggle HUD with X button (RawButton bypasses Input System)
        bool xDown = OVRInput.Get(OVRInput.RawButton.X);
        if (xDown && !xWasDown)
        {
            hudVisible = !hudVisible;
            hudRoot.SetActive(hudVisible);
            Debug.Log("[HUD] Toggled " + (hudVisible ? "ON" : "OFF"));
        }
        xWasDown = xDown;

        if (!hudVisible || missionManager == null) return;

        var ph = missionManager.GetPhase();
        bool carry = missionManager.IsCarrying();

        switch (ph)
        {
            case MissionManager.MissionPhase.PickUp:
                taskTxt.text = "PICK UP SEISMOMETER"; taskTxt.color = cyan;
                hintTxt.text = "R-trigger near it  |  R3 crouch  |  L3 lamps";
                break;
            case MissionManager.MissionPhase.CarryToZone:
                taskTxt.text = "CARRY TO ZONE"; taskTxt.color = cyan;
                hintTxt.text = "R-trigger drop  |  Both grips 1.5s = menu";
                break;
            case MissionManager.MissionPhase.Deploy:
                taskTxt.text = "DEPLOY"; taskTxt.color = green;
                hintTxt.text = "Hold B for 2 seconds";
                break;
            case MissionManager.MissionPhase.Complete:
                taskTxt.text = "EVA COMPLETE"; taskTxt.color = green;
                hintTxt.text = "Both grips 1.5s = menu";
                break;
        }

        Nav(ph);

        invIcon.color = carry ? invOn : invOff;
        invTxt.text = carry ? "SEIS" : (ph == MissionManager.MissionPhase.Complete ? "\u2713" : "---");
        invTxt.color = carry ? green : dimTxt;

        bool showDep = ph == MissionManager.MissionPhase.Deploy;
        if (deployBarBG.gameObject.activeSelf != showDep) deployBarBG.gameObject.SetActive(showDep);
        if (showDep)
        {
            float p = missionManager.GetDeployProgress();
            deployBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.02f + 0.96f * p, 0.85f);
            deployLbl.text = p > 0.01f ? Mathf.RoundToInt(p * 100f) + "%" : "HOLD [B]";
        }
    }

    void Nav(MissionManager.MissionPhase ph)
    {
        if (playerRig == null) return;
        Transform tgt = null;
        if (ph == MissionManager.MissionPhase.PickUp)
        { GameObject s = GameObject.FindWithTag("MissionProp"); if (s) tgt = s.transform; }
        else if (ph == MissionManager.MissionPhase.CarryToZone || ph == MissionManager.MissionPhase.Deploy)
        { GameObject z = GameObject.FindWithTag("DeploymentZone"); if (z) tgt = z.transform; }

        if (tgt == null || ph == MissionManager.MissionPhase.Complete)
        { distTxt.text = ""; arrowTxt.text = "\u2713"; arrowTxt.color = green; return; }

        Vector3 d = tgt.position - playerRig.position; d.y = 0;
        distTxt.text = Mathf.RoundToInt(d.magnitude) + " m";
        Vector3 f = playerRig.forward; f.y = 0;
        if (f.sqrMagnitude < 0.001f) f = Vector3.forward; f.Normalize();
        float a = Vector3.SignedAngle(f, d.normalized, Vector3.up);
        arrowTxt.text = Arrow(a); arrowTxt.color = cyan;
    }

    string Arrow(float a)
    {
        a = (a + 360f) % 360f;
        if (a < 22.5f) return "\u2191"; if (a < 67.5f) return "\u2197";
        if (a < 112.5f) return "\u2192"; if (a < 157.5f) return "\u2198";
        if (a < 202.5f) return "\u2193"; if (a < 247.5f) return "\u2199";
        if (a < 292.5f) return "\u2190"; if (a < 337.5f) return "\u2196";
        return "\u2191";
    }

    Image MakePanel(RectTransform p, string n, Color c, float x0, float y0, float x1, float y1)
    {
        GameObject g = UIgo(n, p); Image img = g.AddComponent<Image>(); img.color = c;
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = new Vector2(2, 2); rt.offsetMax = new Vector2(-2, -2);
        return img;
    }

    TextMeshProUGUI Txt(string n, RectTransform p, string t, float sz, Color c,
        float x0, float y0, float x1, float y1, TextAlignmentOptions al, FontStyles fs)
    {
        GameObject g = UIgo(n, p); TextMeshProUGUI tmp = g.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = sz; tmp.color = c; tmp.alignment = al; tmp.fontStyle = fs;
        tmp.enableWordWrapping = true; tmp.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return tmp;
    }

    GameObject UIgo(string n, RectTransform p)
    {
        GameObject g = new GameObject(n); g.transform.SetParent(p, false);
        g.AddComponent<RectTransform>().localScale = Vector3.one; return g;
    }
}
