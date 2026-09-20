using UnityEngine;

public class LunarLocomotion : MonoBehaviour
{
    [Header("Walk")]
    public float walkSpeed = 3f;

    [Header("Turn")]
    public float turnSpeed = 90f;

    [Header("Jump")]
    public float jumpForce = 2.5f;

    [Header("Gravity")]
    public float gravity = -1.625f;

    [Header("Terrain Follow / Height")]
    [Tooltip("Extra height above terrain. 4.5 puts seated player at astronaut standing eye-height.")]
    public float heightBoost = 4.5f;

    [Header("Crouch (R3 = right stick click)")]
    [Tooltip("How much heightBoost is reduced when crouching.")]
    public float crouchDrop = 3.5f;
    [Tooltip("How fast crouch lerps (seconds).")]
    public float crouchSpeed = 6f;

    public LayerMask groundLayers = -1;

    Transform camT;
    float yVel;
    float terrainY;
    float rigY;
    bool  airborne;
    float speedMult = 1f;
    bool  crouching = false;
    float crouchT   = 0f;   // 0 = standing, 1 = fully crouched

    void Awake()
    {
        Camera cam = Camera.main;
        if (cam != null) camT = cam.transform;
    }

    void Start()
    {
        terrainY = SampleTerrain(transform.position);
        rigY     = terrainY + heightBoost;
        SetRigY(rigY);
    }

    void Update()
    {
        // ── Crouch toggle (R3 = right thumbstick press) ──────────
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
            crouching = !crouching;

        // Smooth lerp toward target crouch state
        float crouchTarget = crouching ? 1f : 0f;
        crouchT = Mathf.MoveTowards(crouchT, crouchTarget, crouchSpeed * Time.deltaTime);

        float effectiveHeight = heightBoost - (crouchDrop * crouchT);

        // ── Turning (right stick X) ──────────────────────────────
        float tx = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
        if (Mathf.Abs(tx) > 0.1f)
            transform.Rotate(Vector3.up, tx * turnSpeed * Time.deltaTime, Space.World);

        // ── Walking (left stick) ─────────────────────────────────
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (stick.magnitude < 0.1f) stick = Vector2.zero;

        Vector3 fwd = transform.forward;
        Vector3 rgt = transform.right;
        if (camT != null)
        {
            fwd = camT.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.001f) fwd.Normalize(); else fwd = transform.forward;
            rgt = camT.right;   rgt.y = 0f;
            if (rgt.sqrMagnitude > 0.001f) rgt.Normalize(); else rgt = transform.right;
        }

        // Crouch slows walk to 60%
        float crouchSpeedMod = Mathf.Lerp(1f, 0.6f, crouchT);
        float speed = walkSpeed * speedMult * crouchSpeedMod;
        Vector3 hMove = (fwd * stick.y + rgt * stick.x) * speed * Time.deltaTime;
        Vector3 pos   = transform.position;
        pos.x += hMove.x;
        pos.z += hMove.z;

        terrainY = SampleTerrain(pos);

        // ── Vertical (gravity + jump) ────────────────────────────
        if (airborne)
        {
            yVel += gravity * Time.deltaTime;
            rigY += yVel    * Time.deltaTime;

            if (rigY <= terrainY + effectiveHeight)
            {
                rigY     = terrainY + effectiveHeight;
                yVel     = 0f;
                airborne = false;
            }
        }
        else
        {
            rigY = terrainY + effectiveHeight;
            yVel = 0f;

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                yVel     = jumpForce;
                airborne = true;
                crouching = false;  // stand up when jumping
            }
        }

        pos.y = rigY;
        transform.position = pos;
    }

    /// <summary>Called by MissionManager to boost/reset walk speed.</summary>
    public void SetSpeedMultiplier(float mult) { speedMult = mult; }

    /// <summary>Called by PauseMenu to teleport back to spawn.</summary>
    public void TeleportTo(Vector3 worldPos)
    {
        terrainY = SampleTerrain(worldPos);
        float effectiveHeight = heightBoost - (crouchDrop * crouchT);
        rigY = terrainY + effectiveHeight;
        worldPos.y = rigY;
        transform.position = worldPos;
        yVel = 0f;
        airborne = false;
    }

    float SampleTerrain(Vector3 pos)
    {
        Vector3 origin = pos;
        origin.y = pos.y + 200f;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 500f, groundLayers, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return pos.y;
    }

    void SetRigY(float y)
    {
        Vector3 p = transform.position;
        p.y = y;
        transform.position = p;
    }
}
