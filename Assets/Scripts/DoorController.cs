using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Hinge")]
    public Vector3 hingeAxis = Vector3.forward;
    public float closeAngle = 100f;
    public float animDuration = 0.8f;

    Quaternion openRot;
    Quaternion closedRot;
    bool isOpen;
    bool animating;
    float animT;
    Quaternion animFrom;
    Quaternion animTo;

    void Awake()
    {
        openRot = transform.localRotation;
        closedRot = openRot * Quaternion.AngleAxis(closeAngle, hingeAxis);
        isOpen = false;
        transform.localRotation = closedRot;
    }

    public void ToggleDoor()
    {
        if (animating) return;
        isOpen = !isOpen;
        animFrom = transform.localRotation;
        animTo = isOpen ? openRot : closedRot;
        animT = 0f;
        animating = true;
    }

    void Update()
    {
        if (!animating) return;
        animT += Time.deltaTime / animDuration;
        float s = animT * animT * (3f - 2f * animT);
        transform.localRotation = Quaternion.Lerp(animFrom, animTo, s);
        if (animT >= 1f)
        {
            transform.localRotation = animTo;
            animating = false;
        }
    }
}
