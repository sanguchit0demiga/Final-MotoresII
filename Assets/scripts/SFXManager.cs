using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioClip hoverSound;
    public AudioClip clickSound;

    private AudioSource audioSource;

    [Header("Opcional")]
    public AudioMixerGroup sfxMixerGroup;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; 

            if (sfxMixerGroup != null)
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayHover()
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void PlayCustom(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}