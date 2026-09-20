using UnityEngine;

/// <summary>
/// EVA Mission v5.
/// RIGHT INDEX TRIGGER ONLY for pickup/drop.
/// Left hand trigger does nothing for mission (reserved for doors + menu).
/// </summary>
public class MissionManager : MonoBehaviour
{
    public enum MissionPhase { PickUp, CarryToZone, Deploy, Complete }

    [Header("References")]
    public Transform  playerRig;
    public GameObject seismometer;
    public Transform  deploymentZone;

    [Header("Settings")]
    public float pickupRange     = 4f;
    public float deployRange     = 5f;
    public float speedMultiplier = 2f;
    public float deployHoldTime  = 2f;

    MissionPhase phase       = MissionPhase.PickUp;
    bool         carrying    = false;
    float        deployTimer = 0f;
    float        startCooldown = 2.5f;

    bool rightIndexWas = false;
    Renderer[] seisRenderers;
    Collider   seisCollider;

    void Start()
    {
        if (playerRig == null)
        {
            GameObject r = GameObject.Find("[BuildingBlock] Camera Rig");
            if (r != null) playerRig = r.transform;
        }
        if (seismometer == null)
            seismometer = GameObject.FindWithTag("MissionProp");
        if (deploymentZone == null)
        {
            GameObject d = GameObject.FindWithTag("DeploymentZone");
            if (d != null) deploymentZone = d.transform;
        }
        if (seismometer != null)
        {
            seisRenderers = seismometer.GetComponentsInChildren<Renderer>();
            seisCollider  = seismometer.GetComponent<Collider>();
        }
        if (deploymentZone != null) SnapToTerrain(deploymentZone);
        Debug.Log("[Mission] Started. Phase=PickUp");
    }

    void Update()
    {
        if (phase == MissionPhase.Complete) return;
        if (playerRig == null || seismometer == null || deploymentZone == null) return;

        if (startCooldown > 0f) { startCooldown -= Time.deltaTime; return; }

        // RIGHT hand index trigger ONLY — edge detect
        bool ri = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool riPressed = ri && !rightIndexWas;
        rightIndexWas = ri;

        float dSeis = Vector3.Distance(playerRig.position, seismometer.transform.position);
        float dZone = Vector3.Distance(playerRig.position, deploymentZone.position);

        switch (phase)
        {
            case MissionPhase.PickUp:
                if (dSeis < pickupRange && riPressed) DoPickUp();
                break;

            case MissionPhase.CarryToZone:
                FollowPlayer();
                if (riPressed) { DoDrop(); break; }
                if (dZone < deployRange) { phase = MissionPhase.Deploy; deployTimer = 0f; }
                break;

            case MissionPhase.Deploy:
                FollowPlayer();
                if (dZone > deployRange + 2f) { phase = MissionPhase.CarryToZone; deployTimer = 0f; break; }
                if (OVRInput.Get(OVRInput.Button.Two))
                {
                    deployTimer += Time.deltaTime;
                    if (deployTimer >= deployHoldTime) DoDeployComplete();
                }
                else deployTimer = Mathf.Max(0f, deployTimer - Time.deltaTime * 2f);
                break;
        }
    }

    void FollowPlayer()
    {
        seismometer.transform.position = playerRig.position + playerRig.right * 0.35f + Vector3.down * 0.3f;
    }

    void DoPickUp()
    {
        carrying = true;
        SetVis(false);
        if (seisCollider != null) seisCollider.enabled = false;
        Rigidbody rb = seismometer.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        LunarLocomotion l = playerRig.GetComponent<LunarLocomotion>();
        if (l != null) l.SetSpeedMultiplier(speedMultiplier);
        phase = MissionPhase.CarryToZone;
        Debug.Log("[Mission] Picked up. Speed x" + speedMultiplier);
    }

    void DoDrop()
    {
        carrying = false;
        SetVis(true);
        if (seisCollider != null) seisCollider.enabled = true;
        Rigidbody rb = seismometer.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        LunarLocomotion l = playerRig.GetComponent<LunarLocomotion>();
        if (l != null) l.SetSpeedMultiplier(1f);
        phase = MissionPhase.PickUp;
        Debug.Log("[Mission] Dropped.");
    }

    void DoDeployComplete()
    {
        carrying = false;
        SetVis(true);
        if (seisCollider != null) seisCollider.enabled = true;
        Vector3 p = deploymentZone.position;
        RaycastHit hit; if (Physics.Raycast(p + Vector3.up * 10f, Vector3.down, out hit, 50f)) p = hit.point;
        p.y += 0.3f;
        seismometer.transform.position = p;
        seismometer.transform.rotation = Quaternion.identity;
        Rigidbody rb = seismometer.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        LunarLocomotion l = playerRig.GetComponent<LunarLocomotion>();
        if (l != null) l.SetSpeedMultiplier(1f);
        phase = MissionPhase.Complete;
        Debug.Log("[Mission] DEPLOYED!");
    }

    void SetVis(bool v) { if (seisRenderers != null) foreach (Renderer r in seisRenderers) if (r != null) r.enabled = v; }
    void SnapSeisToGround()
    {
        Vector3 p = seismometer.transform.position;
        RaycastHit h; if (Physics.Raycast(p + Vector3.up * 10f, Vector3.down, out h, 50f)) p = h.point;
        p.y += 0.3f; seismometer.transform.position = p; seismometer.transform.rotation = Quaternion.identity;
    }
    void SnapToTerrain(Transform o)
    {
        Vector3 p = o.position;
        RaycastHit h; if (Physics.Raycast(p + Vector3.up * 200f, Vector3.down, out h, 500f)) { p.y = h.point.y + 0.1f; o.position = p; }
    }

    public MissionPhase GetPhase() { return phase; }
    public bool IsCarrying() { return carrying; }
    public float GetDeployProgress() { return phase != MissionPhase.Deploy ? 0f : Mathf.Clamp01(deployTimer / deployHoldTime); }
}
