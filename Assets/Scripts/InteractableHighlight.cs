using UnityEngine;

/// <summary>
/// Visual feedback for interactable objects on the Eagle lander.
/// Attach to any GameObject that has a collider the player can interact with.
///
/// Uses a simple approach: when the player's hand/controller is within
/// highlightRange, the object's material tints to show it's interactable.
///
/// Works with Meta XR Interaction SDK — does NOT require HandGrabInteractable
/// to be present (acts independently as a proximity indicator).
/// </summary>
public class InteractableHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("Distance in metres at which highlight activates")]
    public float highlightRange = 1.5f;

    [Tooltip("Colour to tint the material when in range")]
    public Color highlightColor = new Color(0.3f, 0.7f, 1f, 1f); // soft blue

    [Tooltip("How fast the highlight pulses (0 = no pulse, solid tint)")]
    public float pulseSpeed = 2f;

    [Tooltip("Minimum emission intensity during pulse")]
    public float pulseMin = 0.0f;

    [Tooltip("Maximum emission intensity during pulse")]
    public float pulseMax = 0.4f;

    // Internal
    Renderer targetRenderer;
    Material matInstance;
    Color originalEmission;
    bool hasEmission;
    bool isHighlighted;
    Transform leftHand;
    Transform rightHand;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Get renderer — check self first, then parent (for DoorHandle on door mesh)
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
            targetRenderer = GetComponentInParent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning("[Highlight] No Renderer found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Create instance so we don't modify the shared material
        matInstance = targetRenderer.material;

        // Store original emission
        if (matInstance.HasProperty(EmissionColor))
        {
            originalEmission = matInstance.GetColor(EmissionColor);
            hasEmission = true;
            // Enable emission keyword if not already
            matInstance.EnableKeyword("_EMISSION");
        }

        // Find hand anchors from OVRCameraRig
        OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            leftHand = rig.leftHandAnchor;
            rightHand = rig.rightHandAnchor;
        }

        if (leftHand == null || rightHand == null)
            Debug.LogWarning("[Highlight] Could not find hand anchors — highlight won't track hands.");
    }

    void Update()
    {
        if (leftHand == null && rightHand == null) return;

        // Check distance from either hand to this collider's centre
        Vector3 centre = GetComponent<Collider>() != null
            ? GetComponent<Collider>().bounds.center
            : transform.position;

        float distL = leftHand != null ? Vector3.Distance(leftHand.position, centre) : 999f;
        float distR = rightHand != null ? Vector3.Distance(rightHand.position, centre) : 999f;
        float dist = Mathf.Min(distL, distR);

        bool shouldHighlight = dist <= highlightRange;

        if (shouldHighlight && !isHighlighted)
            SetHighlight(true);
        else if (!shouldHighlight && isHighlighted)
            SetHighlight(false);

        // Pulse effect while highlighted
        if (isHighlighted && hasEmission)
        {
            float pulse = Mathf.Lerp(pulseMin, pulseMax,
                (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);
            matInstance.SetColor(EmissionColor, highlightColor * pulse);
        }
    }

    void SetHighlight(bool on)
    {
        isHighlighted = on;
        if (!hasEmission) return;

        if (!on)
            matInstance.SetColor(EmissionColor, originalEmission);
    }

    void OnDestroy()
    {
        // Restore original if we have an instance
        if (matInstance != null && hasEmission)
            matInstance.SetColor(EmissionColor, originalEmission);
    }
}
