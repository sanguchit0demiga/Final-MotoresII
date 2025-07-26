// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    private AudioSource musicAudioSource;

    [Header("Configuración de Música")]
    public AudioMixerGroup musicMixerGroup;
    public AudioClip menuMusicClip;
    public AudioClip level1MusicClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;

            if (!musicAudioSource.isPlaying && menuMusicClip != null)
            {
                musicAudioSource.clip = menuMusicClip;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Reproduciendo música inicial del menú: {menuMusicClip.name}.");
            }
        }
        else
        {
            Destroy(gameObject);
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
        // ¡CAMBIO AQUÍ! Agregando "Controls"
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
                musicAudioSource.Stop();
            }
            return;
        }

        if (nextClipToPlay != null && musicAudioSource.clip != nextClipToPlay)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = nextClipToPlay;
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Cambiando música a: {nextClipToPlay.name}.");
        }
        else if (nextClipToPlay != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Continuando reproducción de música: {nextClipToPlay.name}.");
        }
    }

    public void PlaySpecificMusic(AudioClip newClip)
    {
        if (musicAudioSource != null && newClip != null && musicAudioSource.clip != newClip)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = newClip;
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Reproduciendo música específica: {newClip.name}.");
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
}