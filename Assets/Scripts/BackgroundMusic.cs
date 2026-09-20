using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;

    [Range(0f, 1f)]
    public float volume = 0.25f;

    AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = musicClip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.priority = 0;

        if (musicClip != null)
        {
            source.Play();
            Debug.Log("[Music] Playing: " + musicClip.name);
        }
        else
        {
            Debug.LogWarning("[Music] No clip assigned!");
        }
    }

    public void SetVolume(float v)
    {
        volume = v;
        if (source != null) source.volume = v;
    }
}
