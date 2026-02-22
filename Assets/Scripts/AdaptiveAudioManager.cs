using UnityEngine;

public class AdaptiveAudioManager : MonoBehaviour
{
    [Header("Audio Layers")]
    public AudioClip[] audioLayers = new AudioClip[5];
    
    private AudioSource[] audioSources = new AudioSource[5];
    private int currentActiveLayer = 0;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;
    [Range(0f, 1f)]
    public float fadeSpeed = 2f;
    
    void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = audioLayers[i];
            audioSources[i].loop = true;
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = (i == 0) ? maxVolume : 0f;
            audioSources[i].spatialBlend = 0f; // 2D audio
            audioSources[i].priority = 128;
        }
    }
    
    void Start()
    {
        foreach (var source in audioSources)
        {
            if (source.clip != null)
                source.Play();
        }
    }
    
    void Update()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            float targetVolume = (i == currentActiveLayer) ? maxVolume : 0f;
            audioSources[i].volume = Mathf.Lerp(audioSources[i].volume, targetVolume, fadeSpeed * Time.deltaTime);
        }
    }
    
    public void SwitchToLayer(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < 5)
        {
            currentActiveLayer = layerIndex;
        }
    }
    
    public void PauseAll()
    {
        foreach (var source in audioSources)
            source.Pause();
    }
    
    public void ResumeAll()
    {
        foreach (var source in audioSources)
            source.UnPause();
    }
    
    public void SetMasterVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
    }
}
