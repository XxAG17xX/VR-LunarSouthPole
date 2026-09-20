using UnityEngine;

[ExecuteAlways]
public class SunFlareBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Camera.current;
        if (cam == null) return;
        transform.LookAt(cam.transform.position);
        transform.Rotate(0, 180, 0);
    }
}
