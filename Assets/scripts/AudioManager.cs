using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Asegúrate de tener esta línea
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance

    public AudioMixer mixer;

    // AHORA SON PRIVADOS: Los sliders se encontrarán dinámicamente
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    // Variables para los clips de audio de SFX
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource sfxAudioSource; // AudioSource para reproducir los SFX


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ¡Este GameObject persistirá!

            // Inicialización del AudioSource para SFX
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            // Asegúrate de que el Output de este sfxAudioSource está en el grupo SFX del AudioMixer en el Inspector
        }
        else
        {
            Destroy(gameObject); // Destruye duplicados
            return;
        }
    }

    void OnEnable()
    {
        // Suscribirse al evento de carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Desuscribirse para evitar fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Aplicar los volúmenes guardados inmediatamente al inicio del juego.
        // Esto asegura que los niveles de audio sean correctos incluso antes de cargar la escena de UI.
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 1f));
    }

    // Este método se llamará cada vez que se carga una escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo intentamos encontrar y configurar los sliders si estamos en la escena de "Audio"
        // ¡CAMBIA "Audio" por el nombre exacto de tu escena de opciones de audio!
        if (scene.name == "Audio" || scene.name == "Options" || scene.name == "AudioOptions") // Agrega todos los nombres posibles de tu escena de opciones
        {
            Debug.Log($"Scene '{scene.name}' loaded. Attempting to find sliders.");

            // Encontrar los sliders por nombre. Asegúrate de que los nombres de tus GameObjects Slider
            // en la jerarquía de la escena de Audio coincidan exactamente con estos nombres.
            GameObject masterSliderGO = GameObject.Find("MasterSlider"); // Asume que el GameObject se llama "MasterSlider"
            GameObject musicSliderGO = GameObject.Find("MusicSlider");   // Asume que el GameObject se llama "MusicSlider"
            GameObject sfxSliderGO = GameObject.Find("VFXSlider");       // Basado en tu imagen anterior, el de SFX se llamaba "VFXSlider"


            if (masterSliderGO != null) masterSlider = masterSliderGO.GetComponent<Slider>();
            if (musicSliderGO != null) musicSlider = musicSliderGO.GetComponent<Slider>();
            if (sfxSliderGO != null) sfxSlider = sfxSliderGO.GetComponent<Slider>();

            // Si se encuentran los sliders, actualiza sus valores visuales y añade los listeners
            if (masterSlider != null)
            {
                // Remueve listeners anteriores para evitar que se añadan múltiples veces
                masterSlider.onValueChanged.RemoveAllListeners();
                masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
                masterSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 1f);
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }
        else // Para otras escenas donde los sliders no están presentes
        {
            // Limpiar las referencias a los sliders para evitar errores accidentales
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
        }
    }

    // Métodos para establecer el volumen (sin cambios)
    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MasterVol", value);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVol", value);
    }

    // Métodos para reproducir SFX (sin cambios)
    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && hoverSound != null)
        {
            sfxAudioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
    }

    public void OnBackPressed()
    {
        // Asegúrate de que "Options" es la escena correcta a la que deseas volver
        SceneManager.LoadScene("Options");
    }
}