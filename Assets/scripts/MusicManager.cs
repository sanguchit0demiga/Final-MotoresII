using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; 

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript instance;

    private AudioSource musicAudioSource;


  
    public AudioMixerGroup musicMixerGroup;

    
    public AudioClip menuMusicClip;           
    public AudioClip level1MusicClip;        
    public AudioClip topDownMusicClip;       
    public AudioClip defeatMusicClip;       
    public AudioClip victoryMusicClip;       

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
                Debug.LogError("MusicManager no tiene un AudioSource para controlar la música. Asegúrate de que esté adjunto.");
                return;
            }
        }

        AudioClip nextClipToPlay = null; 

        if (scene.name == "Level1")
        {
            nextClipToPlay = level1MusicClip;
            if (level1MusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'level1MusicClip' en el Inspector.");
            }
        }
        else if (scene.name == "Defeat")
        {
            nextClipToPlay = defeatMusicClip;

            musicAudioSource.loop = false; 
            if (defeatMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'defeatMusicClip' en el Inspector de MusicManager.");
            }
        }
        else if (scene.name == "Win") 
        {
            nextClipToPlay = victoryMusicClip;
            musicAudioSource.loop = false; 
            if (victoryMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'victoryMusicClip' en el Inspector de MusicManager.");
            }
        }
        else if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            nextClipToPlay = menuMusicClip;
            musicAudioSource.loop = true; 
            if (menuMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'menuMusicClip' en el Inspector.");
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
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Continuando reproducción de música: {nextClipToPlay.name}.");
            }
        }
        else if (musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("MusicManager: Deteniendo música porque no hay clip para esta escena.");
        }
    }
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
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Reanudando reproducción de clip existente: {newClip.name}");
        }
    }

    public AudioClip GetCurrentLevelMusicClip()
    {
        return level1MusicClip;
    }
}