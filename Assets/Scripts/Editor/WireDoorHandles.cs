using UnityEngine;
using UnityEditor;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public static class WireDoorHandles
{
    [MenuItem("Tools/Eagle Lander/Wire Door Handle Interactions")]
    public static void Wire()
    {
        int count = 0;

        foreach (var hgi in Object.FindObjectsByType<HandGrabInteractable>(FindObjectsSortMode.None))
        {
            var rb = hgi.GetComponent<Rigidbody>();
            var grabbable = hgi.GetComponent<Grabbable>();
            var gi = hgi.GetComponent<GrabInteractable>();

            if (rb == null || grabbable == null) continue;

            SerializedObject soHGI = new SerializedObject(hgi);
            var rbPropHGI = soHGI.FindProperty("_rigidbody");
            var pointableHGI = soHGI.FindProperty("_pointableElement");

            if (rbPropHGI != null)
                rbPropHGI.objectReferenceValue = rb;
            if (pointableHGI != null)
                pointableHGI.objectReferenceValue = grabbable;

            soHGI.ApplyModifiedProperties();

            if (gi != null)
            {
                SerializedObject soGI = new SerializedObject(gi);
                var rbPropGI = soGI.FindProperty("_rigidbody");
                var pointableGI = soGI.FindProperty("_pointableElement");

                if (rbPropGI != null)
                    rbPropGI.objectReferenceValue = rb;
                if (pointableGI != null)
                    pointableGI.objectReferenceValue = grabbable;

                soGI.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(hgi.gameObject);
            count++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[WireDoorHandles] Wired " + count + " HandGrabInteractable(s).");
    }
}
