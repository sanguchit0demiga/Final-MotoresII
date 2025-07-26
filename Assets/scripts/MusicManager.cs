// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    private AudioSource musicAudioSource;

    [Header("Configuraci�n de M�sica")]
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
                Debug.Log($"MusicManager: Reproduciendo m�sica inicial del men�: {menuMusicClip.name}.");
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
        Debug.Log($"MusicManager: Escena '{scene.name}' cargada. Verificando m�sica.");

        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                Debug.LogError("MusicManager no tiene un AudioSource para controlar la m�sica. Aseg�rate de que est� adjunto.");
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
        // �CAMBIO AQU�! Agregando "Controls"
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
            Debug.Log($"MusicManager: La escena '{scene.name}' no tiene m�sica espec�fica. Deteniendo la m�sica actual.");
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
            Debug.Log($"MusicManager: Cambiando m�sica a: {nextClipToPlay.name}.");
        }
        else if (nextClipToPlay != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Continuando reproducci�n de m�sica: {nextClipToPlay.name}.");
        }
    }

    public void PlaySpecificMusic(AudioClip newClip)
    {
        if (musicAudioSource != null && newClip != null && musicAudioSource.clip != newClip)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = newClip;
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Reproduciendo m�sica espec�fica: {newClip.name}.");
        }
    }

    public void StopMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("MusicManager: M�sica detenida.");
        }
    }
}