// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mixer; 

    
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

   
    public AudioClip hoverSound;
    public AudioClip clickSound;

    
    private AudioSource sfxAudioSource;

    

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 

            
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
            sfxAudioSource.playOnAwake = false; 
            sfxAudioSource.loop = false;        

            
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

    void Start()
    {
        
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 0.5f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.5f));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

       
        if (scene.name == "Menu" || scene.name == "Options" || scene.name == "Audio" || scene.name == "SelectLevel" || scene.name == "Controls")
        {
            InitializeSliders(); 
        }
        else
        {
            
            masterSlider = null;
            musicSlider = null;
            sfxSlider = null;
        }

       
        if (scene.name == "Level1")
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 1f));
        }
    }

    
    public void InitializeSliders()
    {

     
        GameObject masterSliderGO = GameObject.Find("MasterSlider");
        GameObject musicSliderGO = GameObject.Find("MusicSlider");
        GameObject sfxSliderGO = GameObject.Find("SFXSlider");

        
        masterSlider = (masterSliderGO != null) ? masterSliderGO.GetComponent<Slider>() : null;
        musicSlider = (musicSliderGO != null) ? musicSliderGO.GetComponent<Slider>() : null;
        sfxSlider = (sfxSliderGO != null) ? sfxSliderGO.GetComponent<Slider>() : null;

        
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners(); 
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f); 
            masterSlider.onValueChanged.AddListener(SetMasterVolume); 
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

    
    public void OnBackPressed()
    {
        SceneManager.LoadScene("Options");
    }
}