// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mixer; // ¡Asigna tu Main AudioMixer aquí en el Inspector!

    // Sliders para la interfaz de usuario (se asignarán dinámicamente)
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    // Audio Clips para SFX (Asigna estos en el Inspector del AudioManager)
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // Fuente de audio para SFX (esta sí la gestiona directamente AudioManager)
    private AudioSource sfxAudioSource;

    // Ya no necesitas 'level1Music' aquí, la maneja MusicManagerScript.

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // El AudioManager es persistente

            // Inicializar AudioSource de SFX en este mismo GameObject
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
            sfxAudioSource.playOnAwake = false; // No reproducir al inicio
            sfxAudioSource.loop = false;        // No loop por defecto para SFX

            // ¡MUY IMPORTANTE!: Conectar este AudioSource de SFX al grupo SFX del mixer
            if (mixer != null)
            {
                AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups("SFX");
                if (sfxGroups.Length > 0)
                {
                    sfxAudioSource.outputAudioMixerGroup = sfxGroups[0];
                }
                else
                {
                }
            }
            else
            {
            }
        }
        else
        {
            Destroy(gameObject); // Si ya hay una instancia, destruye este nuevo
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

    void Start()
    {
        // Aplicar los volúmenes guardados al iniciar el juego
        // Esto asegura que los grupos del mixer tengan los valores correctos desde el principio
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 0.5f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.5f));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        // Solo buscar y configurar los Sliders si la escena cargada es una de opciones/menú
        // Los sliders del menú de pausa se inicializan por llamada directa desde PauseManager
        if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            InitializeSliders(); // Llamar a la función de inicialización para sliders de escena
        }
        else
        {
            // Limpiar referencias a los sliders si no estamos en una escena de opciones
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
        }

        // --- SOLUCIÓN PARA MÚSICA FUERTE EN EL NIVEL ---
        // Al cargar la escena "Level1", forzamos la aplicación del volumen de música guardado.
        // Esto es un "doble chequeo" para asegurar que el AudioMixer se actualice.
        if (scene.name == "Level1")
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 1f));
        }
    }

    // --- MÉTODO CLAVE: Inicializar Sliders ---
    // Este método es público para poder llamarlo desde scripts como PauseManager.
    public void InitializeSliders()
    {

        // Buscar los GameObjects de los Sliders por su nombre
        // Asegúrate de que estos nombres sean EXACTOS a los de tus Sliders en la jerarquía de Unity
        GameObject masterSliderGO = GameObject.Find("MasterSlider");
        GameObject musicSliderGO = GameObject.Find("MusicSlider");
        GameObject sfxSliderGO = GameObject.Find("SFXSlider");

        // Obtener los componentes Slider
        masterSlider = (masterSliderGO != null) ? masterSliderGO.GetComponent<Slider>() : null;
        musicSlider = (musicSliderGO != null) ? musicSliderGO.GetComponent<Slider>() : null;
        sfxSlider = (sfxSliderGO != null) ? sfxSliderGO.GetComponent<Slider>() : null;

        // Configurar cada slider si se encontró
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners(); // Evitar duplicados de listeners
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f); // Establecer el valor guardado
            masterSlider.onValueChanged.AddListener(SetMasterVolume); // Asignar el listener
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            Debug.Log($"MusicSlider encontrado y configurado. Valor inicial: {musicSlider.value}");
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            Debug.Log($"SFXSlider encontrado y configurado. Valor inicial: {sfxSlider.value}");
        }

        if (masterSlider == null && musicSlider == null && sfxSlider == null)
        {
        }
    }


    // --- Métodos de Control de Volumen (interactúan con el mixer) ---
    // Estos métodos aplican el volumen a los grupos del AudioMixer.
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

    // --- Métodos de Reproducción de SFX ---
    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && hoverSound != null)
        {
            sfxAudioSource.PlayOneShot(hoverSound);
        }
        else
        {
        }
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
       
    }

    // --- NUEVO: Método genérico para reproducir otros SFX (salto, disparo, etc.) ---
    public void PlaySFX(AudioClip clipToPlay)
    {
        if (sfxAudioSource != null && clipToPlay != null)
        {
            sfxAudioSource.PlayOneShot(clipToPlay);
        }
        else
        {
        }
    }

    // --- Método de Navegación de Escenas ---
    public void OnBackPressed()
    {
        SceneManager.LoadScene("Options");
    }
}