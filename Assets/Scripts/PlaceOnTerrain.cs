using UnityEngine;

public class PlaceOnTerrain : MonoBehaviour
{
    [Tooltip("Height offset above terrain (eye height)")]
    public float eyeHeight = 1.7f;
    
    [Tooltip("Terrain to place on")]
    public Terrain terrain;
    
void Start()
    {
        // Find terrain if not assigned
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
        }
        
        if (terrain != null)
        {
            // Get current XZ position
            Vector3 currentPos = transform.position;
            
            // Sample terrain height at current XZ position
            float terrainHeight = terrain.SampleHeight(currentPos);
            
            // Place VR Player at terrain height (NOT adding eye height here)
            // The XR Origin's Camera Offset handles the eye height automatically
            transform.position = new Vector3(
                currentPos.x,
                terrainHeight,
                currentPos.z
            );
            
            Debug.Log($"Placed VR Player on terrain at Y={terrainHeight} (Camera Offset will add eye height)");
        }
        else
        {
            Debug.LogWarning("PlaceOnTerrain: No terrain found!");
        }
    }
}