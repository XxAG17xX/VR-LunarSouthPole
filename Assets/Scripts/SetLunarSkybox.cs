using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class SetLunarSkybox : MonoBehaviour
{
    public Material skyboxMaterial;

    void OnEnable()
    {
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
