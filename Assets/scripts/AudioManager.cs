// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mixer; // �Asigna tu Main AudioMixer aqu� en el Inspector!

    // Sliders para la interfaz de usuario (se asignar�n din�micamente)
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    // Audio Clips para SFX (Asigna estos en el Inspector del AudioManager)
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // Fuente de audio para SFX (esta s� la gestiona directamente AudioManager)
    private AudioSource sfxAudioSource;

    // Ya no necesitas 'level1Music' aqu�, la maneja MusicManagerScript.

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

            // �MUY IMPORTANTE!: Conectar este AudioSource de SFX al grupo SFX del mixer
            if (mixer != null)
            {
                AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups("SFX");
                if (sfxGroups.Length > 0)
                {
                    sfxAudioSource.outputAudioMixerGroup = sfxGroups[0];
                    Debug.Log("AudioManager: SFX AudioSource conectado al grupo 'SFX' del mixer.");
                }
                else
                {
                    Debug.LogWarning("AudioManager: No se encontr� el grupo 'SFX' en el AudioMixer. Los SFX no ser�n controlados por el slider.");
                }
            }
            else
            {
                Debug.LogError("AudioManager: No hay AudioMixer asignado en el Inspector del AudioManager. Los controles de audio no funcionar�n.");
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
        // Aplicar los vol�menes guardados al iniciar el juego
        // Esto asegura que los grupos del mixer tengan los valores correctos desde el principio
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 0.5f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.5f));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Escena '{scene.name}' cargada. Verificando configuraci�n de audio.");

        // Solo buscar y configurar los Sliders si la escena cargada es una de opciones/men�
        // Los sliders del men� de pausa se inicializan por llamada directa desde PauseManager
        if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            InitializeSliders(); // Llamar a la funci�n de inicializaci�n para sliders de escena
        }
        else
        {
            // Limpiar referencias a los sliders si no estamos en una escena de opciones
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
            Debug.Log($"AudioManager: Escena '{scene.name}' no es de opciones. Referencias de sliders limpiadas.");
        }

        // --- SOLUCI�N PARA M�SICA FUERTE EN EL NIVEL ---
        // Al cargar la escena "Level1", forzamos la aplicaci�n del volumen de m�sica guardado.
        // Esto es un "doble chequeo" para asegurar que el AudioMixer se actualice.
        if (scene.name == "Level1")
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 1f));
            Debug.Log("AudioManager: Reaplicando volumen de m�sica al cargar Level1.");
        }
    }

    // --- M�TODO CLAVE: Inicializar Sliders ---
    // Este m�todo es p�blico para poder llamarlo desde scripts como PauseManager.
    public void InitializeSliders()
    {
        Debug.Log("AudioManager: Intentando inicializar sliders de audio...");

        // Buscar los GameObjects de los Sliders por su nombre
        // Aseg�rate de que estos nombres sean EXACTOS a los de tus Sliders en la jerarqu�a de Unity
        GameObject masterSliderGO = GameObject.Find("MasterSlider");
        GameObject musicSliderGO = GameObject.Find("MusicSlider");
        GameObject sfxSliderGO = GameObject.Find("SFXSlider");

        // Obtener los componentes Slider
        masterSlider = (masterSliderGO != null) ? masterSliderGO.GetComponent<Slider>() : null;
        musicSlider = (musicSliderGO != null) ? musicSliderGO.GetComponent<Slider>() : null;
        sfxSlider = (sfxSliderGO != null) ? sfxSliderGO.GetComponent<Slider>() : null;

        // Configurar cada slider si se encontr�
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners(); // Evitar duplicados de listeners
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f); // Establecer el valor guardado
            masterSlider.onValueChanged.AddListener(SetMasterVolume); // Asignar el listener
            Debug.Log($"MasterSlider encontrado y configurado. Valor inicial: {masterSlider.value}");
        }
        else { Debug.LogWarning("MasterSlider GameObject o componente Slider no encontrado en la UI activa."); }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            Debug.Log($"MusicSlider encontrado y configurado. Valor inicial: {musicSlider.value}");
        }
        else { Debug.LogWarning("MusicSlider GameObject o componente Slider no encontrado en la UI activa."); }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            Debug.Log($"SFXSlider encontrado y configurado. Valor inicial: {sfxSlider.value}");
        }
        else { Debug.LogWarning("SFXSlider GameObject o componente Slider no encontrado en la UI activa."); }

        if (masterSlider == null && musicSlider == null && sfxSlider == null)
        {
            Debug.Log("AudioManager: No se encontraron sliders de audio en la UI activa tras buscar.");
        }
    }


    // --- M�todos de Control de Volumen (interact�an con el mixer) ---
    // Estos m�todos aplican el volumen a los grupos del AudioMixer.
    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MasterVol", value);
        Debug.Log($"Master Volume set to: {value} (mixer: {Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20} dB)");
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVol", value);
        Debug.Log($"Music Volume set to: {value} (mixer: {Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20} dB)");
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVol", value);
        Debug.Log($"SFX Volume set to: {value} (mixer: {Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20} dB)");
    }

    // --- M�todos de Reproducci�n de SFX ---
    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && hoverSound != null)
        {
            sfxAudioSource.PlayOneShot(hoverSound);
            Debug.Log($"Playing hover sound: {hoverSound.name}");
        }
        else
        {
            Debug.LogWarning("Cannot play hover sound: SFX AudioSource not ready or clip is null.");
        }
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
            Debug.Log($"Playing click sound: {clickSound.name}");
        }
        else
        {
            Debug.LogWarning("Cannot play click sound: SFX AudioSource not ready or clip is null.");
        }
    }

    // --- NUEVO: M�todo gen�rico para reproducir otros SFX (salto, disparo, etc.) ---
    public void PlaySFX(AudioClip clipToPlay)
    {
        if (sfxAudioSource != null && clipToPlay != null)
        {
            sfxAudioSource.PlayOneShot(clipToPlay);
            Debug.Log($"Playing SFX: {clipToPlay.name}");
        }
        else
        {
            Debug.LogWarning($"Cannot play SFX ({clipToPlay?.name ?? "null"}): SFX AudioSource not ready or clip is null.");
        }
    }

    // --- M�todo de Navegaci�n de Escenas ---
    public void OnBackPressed()
    {
        SceneManager.LoadScene("Options");
    }
}