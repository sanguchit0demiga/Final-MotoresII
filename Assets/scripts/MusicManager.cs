// MusicManagerScript.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    private AudioSource musicAudioSource;

    [Header("Configuraci�n de M�sica")]
    public AudioMixerGroup musicMixerGroup; // Asigna tu grupo 'Music' aqu� en el Inspector
    public AudioClip menuMusicClip;         // Asigna el clip de m�sica del men� aqu�
    public AudioClip level1MusicClip;       // Asigna el clip de m�sica del Nivel 1 aqu�

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
            musicAudioSource.loop = true; // La m�sica suele repetirse
            musicAudioSource.playOnAwake = false; // Lo gestionaremos manualmente

            // Reproducir la m�sica inicial (del men�) si no est� sonando ya.
            if (!musicAudioSource.isPlaying && menuMusicClip != null)
            {
                musicAudioSource.clip = menuMusicClip;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Reproduciendo m�sica inicial del men�: {menuMusicClip.name}.");
            }
        }
        else
        {
            Destroy(gameObject); // Si ya existe otra instancia, destr�yete
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
                musicAudioSource.Stop(); // Detener si no hay m�sica espec�fica para esta escena
            }
            return;
        }

        // Si el nuevo clip es diferente al actual, o si no hay nada sonando, c�mbialo y reprod�celo
        if (nextClipToPlay != null)
        {
            if (musicAudioSource.clip != nextClipToPlay)
            {
                musicAudioSource.Stop();
                musicAudioSource.clip = nextClipToPlay;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Cambiando m�sica a: {nextClipToPlay.name}.");
            }
            else if (!musicAudioSource.isPlaying)
            {
                // Si el mismo clip debe sonar pero no est� sonando, iniciar
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Continuando reproducci�n de m�sica: {nextClipToPlay.name}.");
            }
        }
        else if (musicAudioSource.isPlaying) // Si nextClipToPlay es nulo pero algo est� sonando
        {
            musicAudioSource.Stop(); // Detener la m�sica
            Debug.Log("MusicManager: Deteniendo m�sica porque no hay clip para esta escena.");
        }
    }

    // --- NUEVO: M�todos para controlar la m�sica de forma expl�cita ---
    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
            Debug.Log("MusicManager: M�sica pausada.");
        }
    }

    public void UnpauseMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.UnPause();
            Debug.Log("MusicManager: M�sica reanudada.");
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
            musicAudioSource.Play(); // Si ya es el mismo clip pero est� pausado/detenido
            Debug.Log($"MusicManager: Reanudando reproducci�n de clip existente: {newClip.name}");
        }
    }
}