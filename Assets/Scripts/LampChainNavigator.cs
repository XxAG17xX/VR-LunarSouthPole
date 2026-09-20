using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// L3 (left thumbstick click) toggles navigation lamp chain.
/// FIX: light range boosted to 60m (was 18, spacing is 25 — had dead zones).
/// </summary>
public class LampChainNavigator : MonoBehaviour
{
    [Header("Chain Settings")]
    public float lampSpacing    = 25f;
    public float lampHeight     = 3.2f;
    public Color guideColor     = new Color(0.4f, 0.9f, 1f);
    public float lightIntensity = 12f;
    public float lightRange     = 60f;

    [Header("References")]
    public MissionManager missionManager;

    bool chainActive = false;
    List<GameObject> chainLamps = new List<GameObject>();

    Material poleMat;
    Material headMat;
    Material lightMat;

    void Start()
    {
        if (missionManager == null)
            missionManager = FindFirstObjectByType<MissionManager>();

        poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        poleMat.color = new Color(0.6f, 0.65f, 0.7f);

        headMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        headMat.color = guideColor;
        headMat.EnableKeyword("_EMISSION");
        headMat.SetColor("_EmissionColor", guideColor * 3f);

        lightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        lightMat.color = Color.white;
        lightMat.EnableKeyword("_EMISSION");
        lightMat.SetColor("_EmissionColor", guideColor * 6f);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
            ToggleChain();
    }

    public void ToggleChain()
    {
        if (chainActive) DespawnChain();
        else             SpawnChain();
    }

    public void SpawnChain()
    {
        DespawnChain();

        if (missionManager == null) return;
        MissionManager.MissionPhase phase = missionManager.GetPhase();
        if (phase == MissionManager.MissionPhase.Complete) return;

        Transform target = null;
        if (phase == MissionManager.MissionPhase.PickUp)
        {
            GameObject seis = GameObject.FindWithTag("MissionProp");
            if (seis != null) target = seis.transform;
        }
        else
        {
            GameObject zone = GameObject.FindWithTag("DeploymentZone");
            if (zone != null) target = zone.transform;
        }

        if (target == null) return;

        Vector3 start = transform.position;
        Vector3 end   = target.position;
        float totalDist = Vector3.Distance(start, end);
        int count = Mathf.FloorToInt(totalDist / lampSpacing);

        for (int i = 1; i <= count; i++)
        {
            float t = (float)i / (count + 1);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos = SnapToTerrain(pos);
            chainLamps.Add(BuildLamp(pos, i));
        }

        chainActive = true;
        Debug.Log("[LampChain] Spawned " + count + " lamps toward " + target.name);
    }

    public void DespawnChain()
    {
        foreach (var lamp in chainLamps)
            if (lamp != null) Destroy(lamp);
        chainLamps.Clear();
        chainActive = false;
    }

    GameObject BuildLamp(Vector3 pos, int index)
    {
        GameObject lamp = new GameObject("GuideLamp_" + index);
        lamp.transform.position = pos;

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.parent        = lamp.transform;
        pole.transform.localPosition = new Vector3(0, lampHeight * 0.5f, 0);
        pole.transform.localScale    = new Vector3(0.05f, lampHeight * 0.5f, 0.05f);
        pole.GetComponent<Renderer>().sharedMaterial = poleMat;
        Destroy(pole.GetComponent<Collider>());

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.parent        = lamp.transform;
        head.transform.localPosition = new Vector3(0, lampHeight + 0.1f, 0);
        head.transform.localScale    = new Vector3(0.3f, 0.1f, 0.22f);
        head.GetComponent<Renderer>().sharedMaterial = headMat;
        Destroy(head.GetComponent<Collider>());

        GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lens.name = "Lens";
        lens.transform.parent        = lamp.transform;
        lens.transform.localPosition = new Vector3(0, lampHeight + 0.04f, 0.12f);
        lens.transform.localScale    = new Vector3(0.22f, 0.07f, 0.02f);
        lens.GetComponent<Renderer>().sharedMaterial = lightMat;
        Destroy(lens.GetComponent<Collider>());

        // POINT light instead of spot — illuminates all around, much more visible
        GameObject lightGO = new GameObject("Light");
        lightGO.transform.parent        = lamp.transform;
        lightGO.transform.localPosition = new Vector3(0, lampHeight + 0.15f, 0);
        Light lt = lightGO.AddComponent<Light>();
        lt.type      = LightType.Point;
        lt.color     = guideColor;
        lt.intensity = lightIntensity;
        lt.range     = lightRange;
        lt.shadows   = LightShadows.None;  // no shadows = cheaper + brighter ground

        return lamp;
    }

    Vector3 SnapToTerrain(Vector3 pos)
    {
        Vector3 origin = pos;
        origin.y = pos.y + 300f;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 600f))
            return new Vector3(pos.x, hit.point.y, pos.z);
        return pos;
    }
}
