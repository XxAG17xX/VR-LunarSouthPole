using UnityEngine;

public class DoorHandleInteractable : MonoBehaviour
{
    [Tooltip("How close the controller must be to trigger")]
    public float interactRange = 1.5f;

    DoorController door;
    Transform leftHand;
    Transform rightHand;
    bool triggered;

    void Awake()
    {
        door = GetComponentInParent<DoorController>();
    }

    void Start()
    {
        OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            leftHand = rig.leftHandAnchor;
            rightHand = rig.rightHandAnchor;
        }
    }

    void Update()
    {
        if (door == null) return;
        if (leftHand == null && rightHand == null) return;

        Vector3 centre = GetComponent<Collider>().bounds.center;

        float dL = leftHand != null ? Vector3.Distance(leftHand.position, centre) : 999f;
        float dR = rightHand != null ? Vector3.Distance(rightHand.position, centre) : 999f;

        bool inRange = (dL <= interactRange) || (dR <= interactRange);

        bool grip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)
                 || OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

        if (inRange && grip && !triggered)
        {
            door.ToggleDoor();
            triggered = true;
        }

        if (!grip)
            triggered = false;
    }
}
