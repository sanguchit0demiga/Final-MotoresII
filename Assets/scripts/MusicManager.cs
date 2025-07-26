// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio; // Asegúrate de tener esto para AudioMixerGroup

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    // Puedes hacer que el AudioSource sea público si quieres controlarlo desde fuera,
    // pero para persistencia, el mixer ya debería bastar.
    // public AudioSource musicAudioSource; 

    [Header("Configuración de Audio")]
    public AudioMixerGroup musicMixerGroup; // Asigna tu grupo 'Music' aquí en el Inspector
    public AudioClip backgroundMusicClip; // Asigna el clip de música aquí

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ¡Esto hace que persista!

            // Asegúrate de que el GameObject tenga un AudioSource
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            // Asegúrate de que el AudioSource esté configurado correctamente
            audioSource.outputAudioMixerGroup = musicMixerGroup; // Conecta al mixer
            audioSource.loop = true; // La música suele repetirse
            audioSource.playOnAwake = true; // Que empiece a sonar al inicio
            audioSource.clip = backgroundMusicClip; // Asigna el clip

            // Si ya está sonando, no la reinicies. Si no, empieza a sonar.
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Play();
            }

        }
        else
        {
            // Si ya existe otra instancia, destrúyete para evitar duplicados
            Destroy(gameObject);
        }
    }

    // Puedes añadir métodos públicos aquí si quieres cambiar la música
    // desde otros scripts:
    public void PlayNewMusic(AudioClip newClip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && newClip != null && audioSource.clip != newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.Stop();
    }
}