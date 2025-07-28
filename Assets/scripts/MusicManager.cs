using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Necesario para SceneManager

public class MusicManagerScript : MonoBehaviour
{
    // Una instancia estática para hacer el MusicManager accesible globalmente (Singleton pattern)
    public static MusicManagerScript instance;

    // Referencia al AudioSource que reproducirá la música
    private AudioSource musicAudioSource;

    [Header("Configuración de Música")]
    // Grupo del AudioMixer para la música (así puedes controlar su volumen globalmente)
    public AudioMixerGroup musicMixerGroup;

    // Clips de audio para las diferentes escenas
    public AudioClip menuMusicClip;          // Música para el menú principal y escenas relacionadas
    public AudioClip level1MusicClip;        // Música para el nivel de juego principal
    public AudioClip topDownMusicClip;       // Música específica para el modo Top-Down (si lo usas)
    public AudioClip defeatMusicClip;        // ¡NUEVO! Música para la escena de derrota

    void Awake()
    {
        // Implementación del patrón Singleton para asegurar que solo haya una instancia de MusicManager
        if (instance == null)
        {
            instance = this;
            // No destruyas este GameObject al cargar nuevas escenas para que la música persista
            DontDestroyOnLoad(gameObject);

            // Obtener o añadir el AudioSource al GameObject
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configurar el AudioSource
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup; // Conecta al grupo de música del mixer
            musicAudioSource.loop = true;                             // La música suele repetirse
            musicAudioSource.playOnAwake = false;                     // Lo gestionaremos manualmente

            // Reproducir la música inicial (del menú) si aún no hay nada sonando
            if (!musicAudioSource.isPlaying && menuMusicClip != null)
            {
                musicAudioSource.clip = menuMusicClip;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Reproduciendo música inicial del menú: {menuMusicClip.name}.");
            }
        }
        else
        {
            // Si ya existe otra instancia, destruye esta nueva para evitar duplicados
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Suscribirse al evento de carga de escena para cambiar la música automáticamente
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Desuscribirse del evento cuando el GameObject se deshabilita o destruye
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Este método se llama cada vez que se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicManager: Escena '{scene.name}' cargada. Verificando música.");

        // Asegurarse de que el AudioSource esté disponible
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                Debug.LogError("MusicManager no tiene un AudioSource para controlar la música. Asegúrate de que esté adjunto.");
                return;
            }
        }

        AudioClip nextClipToPlay = null; // Variable para almacenar el clip que se va a reproducir

        // Lógica para determinar qué clip de música debe sonar según el nombre de la escena
        if (scene.name == "Level1")
        {
            nextClipToPlay = level1MusicClip;
            if (level1MusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'level1MusicClip' en el Inspector.");
            }
        }
        else if (scene.name == "Defeat") // ¡NUEVA CONDICIÓN para la escena de derrota!
        {
            nextClipToPlay = defeatMusicClip;
            if (defeatMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'defeatMusicClip' en el Inspector de MusicManager.");
            }
        }
        else if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            // Si es una escena de menú/opciones, reproduce la música del menú
            nextClipToPlay = menuMusicClip;
            if (menuMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'menuMusicClip' en el Inspector.");
            }
        }
        else
        {
            // Si la escena no tiene música específica asignada, detén cualquier música que esté sonando
            Debug.Log($"MusicManager: La escena '{scene.name}' no tiene música específica. Deteniendo la música actual.");
            if (musicAudioSource.isPlaying)
            {
                musicAudioSource.Stop();
            }
            return; // Sale del método ya que no hay música para esta escena
        }

        // Si se determinó un clip para reproducir:
        if (nextClipToPlay != null)
        {
            // Si el clip actual es diferente al que se va a reproducir, o no hay nada sonando
            if (musicAudioSource.clip != nextClipToPlay)
            {
                musicAudioSource.Stop();       // Detiene la reproducción actual
                musicAudioSource.clip = nextClipToPlay; // Asigna el nuevo clip
                musicAudioSource.Play();       // Comienza a reproducir el nuevo clip
                Debug.Log($"MusicManager: Cambiando música a: {nextClipToPlay.name}.");
            }
            else if (!musicAudioSource.isPlaying)
            {
                // Si es el mismo clip pero está pausado/detenido, reanuda la reproducción
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Continuando reproducción de música: {nextClipToPlay.name}.");
            }
        }
        else if (musicAudioSource.isPlaying)
        {
            // Si nextClipToPlay es nulo (es decir, no se asignó un clip para esta escena)
            // pero algo sigue sonando, detén la música.
            musicAudioSource.Stop();
            Debug.Log("MusicManager: Deteniendo música porque no hay clip para esta escena.");
        }
    }

    // --- MÉTODOS PARA CONTROLAR LA MÚSICA DE FORMA EXPLÍCITA (llamados desde otros scripts) ---

    // Pausa la música actual
    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
            Debug.Log("MusicManager: Música pausada.");
        }
    }

    // Reanuda la música pausada
    public void UnpauseMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.UnPause();
            Debug.Log("MusicManager: Música reanudada.");
        }
    }

    // Detiene completamente la música
    public void StopMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("MusicManager: Música detenida.");
        }
    }

    // Establece un nuevo clip de música y lo reproduce (útil para eventos específicos)
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
            // Si es el mismo clip pero está pausado/detenido, reanudar
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Reanudando reproducción de clip existente: {newClip.name}");
        }
    }

    // Método para obtener el clip de música actual del nivel (si es necesario)
    public AudioClip GetCurrentLevelMusicClip()
    {
        // En tu caso, si siempre es "Level1", puedes devolver directamente level1MusicClip.
        // Si tuvieras más niveles con música específica, necesitarías una lógica más compleja aquí.
        return level1MusicClip;
    }
}