// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mixer; // ¡Este es crucial!

    // Sliders para la interfaz de usuario
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    // Audio Clips para SFX
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // Fuente de audio para SFX (esta sí la gestiona directamente AudioManager)
    private AudioSource sfxAudioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // El AudioManager es persistente

            // Inicializar AudioSource de SFX
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            // Opcional: Asignar el sfxAudioSource a un grupo SFX si tienes uno en tu mixer
            // sfxAudioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0]; 

        }
        else
        {
            Destroy(gameObject);
            return;
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
        // Aplicar los volúmenes guardados al iniciar
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 1f));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena '{scene.name}' cargada. Verificando sliders de audio.");

        // Solo busca y configura los Sliders en las escenas de opciones
        if (scene.name == "Audio" || scene.name == "Options" || scene.name == "AudioOptions")
        {
            GameObject masterSliderGO = GameObject.Find("MasterSlider");
            GameObject musicSliderGO = GameObject.Find("MusicSlider");
            GameObject sfxSliderGO = GameObject.Find("SFXSlider"); // ¡Asegúrate de que este nombre sea correcto en tu UI!

            if (masterSliderGO != null) masterSlider = masterSliderGO.GetComponent<Slider>();
            if (musicSliderGO != null) musicSlider = musicSliderGO.GetComponent<Slider>();
            if (sfxSliderGO != null) sfxSlider = sfxSliderGO.GetComponent<Slider>();

            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveAllListeners();
                masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
                masterSlider.onValueChanged.AddListener(SetMasterVolume);
                Debug.Log("MasterSlider encontrado y configurado.");
            }
            else { Debug.LogWarning("MasterSlider no encontrado en la escena de opciones."); }

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
                Debug.Log("MusicSlider encontrado y configurado.");
            }
            else { Debug.LogWarning("MusicSlider no encontrado en la escena de opciones."); }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 1f);
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
                Debug.Log("SFXSlider encontrado y configurado.");
            }
            else { Debug.LogWarning("SFXSlider no encontrado en la escena de opciones."); }
        }
        else
        {
            // Si no es una escena de opciones, limpiar las referencias a los sliders
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
            Debug.Log($"Escena '{scene.name}' cargada, no es una escena de opciones. Referencias de slider limpiadas.");
        }
    }

    // --- Métodos de Control de Volumen (sin cambios, interactúan con el mixer) ---
    // Estos volúmenes se aplican a los grupos del AudioMixer, ¡que es lo que controla la música y los SFX!

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

    // --- Métodos de Reproducción de SFX (sin cambios) ---

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

    // --- Método de Navegación de Escenas ---

    public void OnBackPressed()
    {
        SceneManager.LoadScene("Options");
    }
}