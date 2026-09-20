using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class PlaceSurfaceLamps
{
    static readonly Vector3[] lampOffsets = new Vector3[]
    {
        new Vector3( 12,  0,   4),
        new Vector3( -8,  0,  10),
        new Vector3(-14,  0,  -3),
        new Vector3(  6,  0, -12),
        new Vector3( 18,  0, -10),
        new Vector3( -4,  0,  16),
    };

    [MenuItem("Tools/Mission/Place Surface Lamps")]
    public static void Place()
    {
        GameObject landerGO = GameObject.Find("EagleLander");
        if (landerGO == null) { Debug.LogError("[PlaceSurfaceLamps] EagleLander not found."); return; }
        Vector3 landerPos = landerGO.transform.position;

        // Materials
        Material poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        poleMat.color = new Color(0.7f, 0.7f, 0.72f);
        AssetDatabase.CreateAsset(poleMat, "Assets/Materials/LampPoleMat.mat");

        Material headMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        headMat.color = new Color(0.85f, 0.82f, 0.7f);
        AssetDatabase.CreateAsset(headMat, "Assets/Materials/LampHeadMat.mat");

        Material lightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        lightMat.color = Color.white;
        lightMat.EnableKeyword("_EMISSION");
        lightMat.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.8f) * 3f);
        AssetDatabase.CreateAsset(lightMat, "Assets/Materials/LampLightMat.mat");

        // Container
        GameObject container = new GameObject("SurfaceLamps");
        Undo.RegisterCreatedObjectUndo(container, "Place Lamps");

        for (int i = 0; i < lampOffsets.Length; i++)
        {
            Vector3 worldPos = SnapToTerrain(landerPos + lampOffsets[i]);

            GameObject lamp = new GameObject("SurfaceLamp_" + i);
            lamp.transform.parent = container.transform;
            lamp.transform.position = worldPos;

            // Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.parent = lamp.transform;
            pole.transform.localPosition = new Vector3(0, 1.5f, 0);
            pole.transform.localScale    = new Vector3(0.06f, 1.5f, 0.06f);
            pole.GetComponent<Renderer>().sharedMaterial = poleMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.parent = lamp.transform;
            head.transform.localPosition = new Vector3(0, 3.1f, 0);
            head.transform.localScale    = new Vector3(0.35f, 0.12f, 0.25f);
            head.GetComponent<Renderer>().sharedMaterial = headMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Lens
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lens.name = "Lens";
            lens.transform.parent = lamp.transform;
            lens.transform.localPosition = new Vector3(0, 3.04f, 0.13f);
            lens.transform.localScale    = new Vector3(0.28f, 0.08f, 0.02f);
            lens.GetComponent<Renderer>().sharedMaterial = lightMat;
            Object.DestroyImmediate(lens.GetComponent<Collider>());

            // Point light
            GameObject lightGO = new GameObject("SpotLight");
            lightGO.transform.parent = lamp.transform;
            lightGO.transform.localPosition = new Vector3(0, 3.0f, 0.15f);
            lightGO.transform.localEulerAngles = new Vector3(70, 0, 0);
            Light lt = lightGO.AddComponent<Light>();
            lt.type      = LightType.Spot;
            lt.color     = new Color(1f, 0.97f, 0.85f);
            lt.intensity = 3.5f;
            lt.range     = 14f;
            lt.spotAngle = 80f;
            lt.shadows   = LightShadows.Hard;
        }

        // Seismometer prop
        Vector3 seisPos = SnapToTerrain(landerPos + new Vector3(4, 0, 2));
        seisPos.y += 0.3f;

        GameObject seis = new GameObject("Seismometer");
        Undo.RegisterCreatedObjectUndo(seis, "Create Seismometer");
        seis.transform.position = seisPos;

        Material baseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        baseMat.color = new Color(0.6f, 0.55f, 0.4f);
        AssetDatabase.CreateAsset(baseMat, "Assets/Materials/SeisBaseMat.mat");

        Material domeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        domeMat.color = new Color(0.85f, 0.8f, 0.65f);
        AssetDatabase.CreateAsset(domeMat, "Assets/Materials/SeisDomeMat.mat");

        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "BasePlate";
        plate.transform.parent = seis.transform;
        plate.transform.localPosition = Vector3.zero;
        plate.transform.localScale    = new Vector3(0.4f, 0.06f, 0.4f);
        plate.GetComponent<Renderer>().sharedMaterial = baseMat;
        Object.DestroyImmediate(plate.GetComponent<Collider>());

        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Dome";
        dome.transform.parent = seis.transform;
        dome.transform.localPosition = new Vector3(0, 0.15f, 0);
        dome.transform.localScale    = new Vector3(0.22f, 0.18f, 0.22f);
        dome.GetComponent<Renderer>().sharedMaterial = domeMat;
        Object.DestroyImmediate(dome.GetComponent<Collider>());

        for (int i = 0; i < 3; i++)
        {
            float a = i * 120f * Mathf.Deg2Rad;
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg_" + i;
            leg.transform.parent = seis.transform;
            leg.transform.localPosition = new Vector3(Mathf.Sin(a) * 0.18f, 0.08f, Mathf.Cos(a) * 0.18f);
            leg.transform.localScale    = new Vector3(0.025f, 0.08f, 0.025f);
            leg.GetComponent<Renderer>().sharedMaterial = baseMat;
            Object.DestroyImmediate(leg.GetComponent<Collider>());
        }

        BoxCollider bc = seis.AddComponent<BoxCollider>();
        bc.size   = new Vector3(0.45f, 0.35f, 0.45f);
        bc.center = new Vector3(0, 0.12f, 0);

        Rigidbody rb = seis.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[PlaceSurfaceLamps] Done. Next: add tag 'MissionProp' in Project Settings > Tags, then assign it to the Seismometer GameObject.");
    }

    static Vector3 SnapToTerrain(Vector3 pos)
    {
        Vector3 origin = pos;
        origin.y = pos.y + 300f;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 600f))
            return new Vector3(pos.x, hit.point.y, pos.z);
        return pos;
    }
}
