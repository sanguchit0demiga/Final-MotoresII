// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio; // Aseg�rate de tener esto para AudioMixerGroup

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    // Puedes hacer que el AudioSource sea p�blico si quieres controlarlo desde fuera,
    // pero para persistencia, el mixer ya deber�a bastar.
    // public AudioSource musicAudioSource; 

    [Header("Configuraci�n de Audio")]
    public AudioMixerGroup musicMixerGroup; // Asigna tu grupo 'Music' aqu� en el Inspector
    public AudioClip backgroundMusicClip; // Asigna el clip de m�sica aqu�

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �Esto hace que persista!

            // Aseg�rate de que el GameObject tenga un AudioSource
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            // Aseg�rate de que el AudioSource est� configurado correctamente
            audioSource.outputAudioMixerGroup = musicMixerGroup; // Conecta al mixer
            audioSource.loop = true; // La m�sica suele repetirse
            audioSource.playOnAwake = true; // Que empiece a sonar al inicio
            audioSource.clip = backgroundMusicClip; // Asigna el clip

            // Si ya est� sonando, no la reinicies. Si no, empieza a sonar.
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Play();
            }

        }
        else
        {
            // Si ya existe otra instancia, destr�yete para evitar duplicados
            Destroy(gameObject);
        }
    }

    // Puedes a�adir m�todos p�blicos aqu� si quieres cambiar la m�sica
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