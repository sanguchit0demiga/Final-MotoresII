// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    private AudioSource musicAudioSource;

    [Header("Configuración de Música")]
    public AudioMixerGroup musicMixerGroup; // Asigna tu grupo 'Music' aquí en el Inspector
    public AudioClip menuMusicClip;         // Asigna el clip de música del menú aquí
    public AudioClip level1MusicClip;       // Asigna el clip de música del Nivel 1 aquí

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Esto hace que persista

            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup; // Conecta al mixer
            musicAudioSource.loop = true; // La música suele repetirse
            musicAudioSource.playOnAwake = false; // Lo gestionaremos manualmente

            // Reproducir la música inicial (del menú) si no está sonando ya.
            if (!musicAudioSource.isPlaying && menuMusicClip != null)
            {
                musicAudioSource.clip = menuMusicClip;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Reproduciendo música inicial del menú: {menuMusicClip.name}.");
            }
        }
        else
        {
            Destroy(gameObject); // Si ya existe otra instancia, destrúyete
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicManager: Escena '{scene.name}' cargada. Verificando música.");

        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                Debug.LogError("MusicManager no tiene un AudioSource para controlar la música. Asegúrate de que está adjunto.");
                return;
            }
        }

        AudioClip nextClipToPlay = null;

        if (scene.name == "Level1")
        {
            nextClipToPlay = level1MusicClip;
            if (level1MusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'Level1MusicClip' en el Inspector.");
            }
        }
        else if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            nextClipToPlay = menuMusicClip;
            if (menuMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'MenuMusicClip' en el Inspector.");
            }
        }
        else
        {
            Debug.Log($"MusicManager: La escena '{scene.name}' no tiene música específica. Deteniendo la música actual.");
            if (musicAudioSource.isPlaying)
            {
                musicAudioSource.Stop(); // Detener si no hay música específica para esta escena
            }
            return;
        }

        // Si el nuevo clip es diferente al actual, o si no hay nada sonando, cámbialo y reprodúcelo
        if (nextClipToPlay != null)
        {
            if (musicAudioSource.clip != nextClipToPlay)
            {
                musicAudioSource.Stop();
                musicAudioSource.clip = nextClipToPlay;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Cambiando música a: {nextClipToPlay.name}.");
            }
            else if (!musicAudioSource.isPlaying)
            {
                // Si el mismo clip debe sonar pero no está sonando, iniciar
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Continuando reproducción de música: {nextClipToPlay.name}.");
            }
        }
        else if (musicAudioSource.isPlaying) // Si nextClipToPlay es nulo pero algo está sonando
        {
            musicAudioSource.Stop(); // Detener la música
            Debug.Log("MusicManager: Deteniendo música porque no hay clip para esta escena.");
        }
    }

    // --- NUEVO: Métodos para controlar la música de forma explícita ---
    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
            Debug.Log("MusicManager: Música pausada.");
        }
    }

    public void UnpauseMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.UnPause();
            Debug.Log("MusicManager: Música reanudada.");
        }
    }

    public void StopMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("MusicManager: Música detenida.");
        }
    }

    public void SetMusicClipAndPlay(AudioClip newClip)
    {
        if (musicAudioSource != null && newClip != null && musicAudioSource.clip != newClip)
        {
            musicAudioSource.clip = newClip;
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Estableciendo y reproduciendo nuevo clip: {newClip.name}");
        }
        else if (musicAudioSource != null && newClip != null && musicAudioSource.clip == newClip && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play(); // Si ya es el mismo clip pero está pausado/detenido
            Debug.Log($"MusicManager: Reanudando reproducción de clip existente: {newClip.name}");
        }
    }
}