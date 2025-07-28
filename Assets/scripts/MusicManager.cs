using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Necesario para SceneManager

public class MusicManagerScript : MonoBehaviour
{
    // Una instancia est�tica para hacer el MusicManager accesible globalmente (Singleton pattern)
    public static MusicManagerScript instance;

    // Referencia al AudioSource que reproducir� la m�sica
    private AudioSource musicAudioSource;

    [Header("Configuraci�n de M�sica")]
    // Grupo del AudioMixer para la m�sica (as� puedes controlar su volumen globalmente)
    public AudioMixerGroup musicMixerGroup;

    // Clips de audio para las diferentes escenas
    public AudioClip menuMusicClip;          // M�sica para el men� principal y escenas relacionadas
    public AudioClip level1MusicClip;        // M�sica para el nivel de juego principal
    public AudioClip topDownMusicClip;       // M�sica espec�fica para el modo Top-Down (si lo usas)
    public AudioClip defeatMusicClip;        // �NUEVO! M�sica para la escena de derrota

    void Awake()
    {
        // Implementaci�n del patr�n Singleton para asegurar que solo haya una instancia de MusicManager
        if (instance == null)
        {
            instance = this;
            // No destruyas este GameObject al cargar nuevas escenas para que la m�sica persista
            DontDestroyOnLoad(gameObject);

            // Obtener o a�adir el AudioSource al GameObject
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configurar el AudioSource
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup; // Conecta al grupo de m�sica del mixer
            musicAudioSource.loop = true;                             // La m�sica suele repetirse
            musicAudioSource.playOnAwake = false;                     // Lo gestionaremos manualmente

            // Reproducir la m�sica inicial (del men�) si a�n no hay nada sonando
            if (!musicAudioSource.isPlaying && menuMusicClip != null)
            {
                musicAudioSource.clip = menuMusicClip;
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Reproduciendo m�sica inicial del men�: {menuMusicClip.name}.");
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
        // Suscribirse al evento de carga de escena para cambiar la m�sica autom�ticamente
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Desuscribirse del evento cuando el GameObject se deshabilita o destruye
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Este m�todo se llama cada vez que se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicManager: Escena '{scene.name}' cargada. Verificando m�sica.");

        // Asegurarse de que el AudioSource est� disponible
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                Debug.LogError("MusicManager no tiene un AudioSource para controlar la m�sica. Aseg�rate de que est� adjunto.");
                return;
            }
        }

        AudioClip nextClipToPlay = null; // Variable para almacenar el clip que se va a reproducir

        // L�gica para determinar qu� clip de m�sica debe sonar seg�n el nombre de la escena
        if (scene.name == "Level1")
        {
            nextClipToPlay = level1MusicClip;
            if (level1MusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'level1MusicClip' en el Inspector.");
            }
        }
        else if (scene.name == "Defeat") // �NUEVA CONDICI�N para la escena de derrota!
        {
            nextClipToPlay = defeatMusicClip;
            if (defeatMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'defeatMusicClip' en el Inspector de MusicManager.");
            }
        }
        else if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            // Si es una escena de men�/opciones, reproduce la m�sica del men�
            nextClipToPlay = menuMusicClip;
            if (menuMusicClip == null)
            {
                Debug.LogWarning("MusicManager: No se ha asignado un AudioClip para 'menuMusicClip' en el Inspector.");
            }
        }
        else
        {
            // Si la escena no tiene m�sica espec�fica asignada, det�n cualquier m�sica que est� sonando
            Debug.Log($"MusicManager: La escena '{scene.name}' no tiene m�sica espec�fica. Deteniendo la m�sica actual.");
            if (musicAudioSource.isPlaying)
            {
                musicAudioSource.Stop();
            }
            return; // Sale del m�todo ya que no hay m�sica para esta escena
        }

        // Si se determin� un clip para reproducir:
        if (nextClipToPlay != null)
        {
            // Si el clip actual es diferente al que se va a reproducir, o no hay nada sonando
            if (musicAudioSource.clip != nextClipToPlay)
            {
                musicAudioSource.Stop();       // Detiene la reproducci�n actual
                musicAudioSource.clip = nextClipToPlay; // Asigna el nuevo clip
                musicAudioSource.Play();       // Comienza a reproducir el nuevo clip
                Debug.Log($"MusicManager: Cambiando m�sica a: {nextClipToPlay.name}.");
            }
            else if (!musicAudioSource.isPlaying)
            {
                // Si es el mismo clip pero est� pausado/detenido, reanuda la reproducci�n
                musicAudioSource.Play();
                Debug.Log($"MusicManager: Continuando reproducci�n de m�sica: {nextClipToPlay.name}.");
            }
        }
        else if (musicAudioSource.isPlaying)
        {
            // Si nextClipToPlay es nulo (es decir, no se asign� un clip para esta escena)
            // pero algo sigue sonando, det�n la m�sica.
            musicAudioSource.Stop();
            Debug.Log("MusicManager: Deteniendo m�sica porque no hay clip para esta escena.");
        }
    }

    // --- M�TODOS PARA CONTROLAR LA M�SICA DE FORMA EXPL�CITA (llamados desde otros scripts) ---

    // Pausa la m�sica actual
    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
            Debug.Log("MusicManager: M�sica pausada.");
        }
    }

    // Reanuda la m�sica pausada
    public void UnpauseMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.UnPause();
            Debug.Log("MusicManager: M�sica reanudada.");
        }
    }

    // Detiene completamente la m�sica
    public void StopMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("MusicManager: M�sica detenida.");
        }
    }

    // Establece un nuevo clip de m�sica y lo reproduce (�til para eventos espec�ficos)
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
            // Si es el mismo clip pero est� pausado/detenido, reanudar
            musicAudioSource.Play();
            Debug.Log($"MusicManager: Reanudando reproducci�n de clip existente: {newClip.name}");
        }
    }

    // M�todo para obtener el clip de m�sica actual del nivel (si es necesario)
    public AudioClip GetCurrentLevelMusicClip()
    {
        // En tu caso, si siempre es "Level1", puedes devolver directamente level1MusicClip.
        // Si tuvieras m�s niveles con m�sica espec�fica, necesitar�as una l�gica m�s compleja aqu�.
        return level1MusicClip;
    }
}