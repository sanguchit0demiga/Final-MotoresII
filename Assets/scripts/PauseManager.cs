
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); 
        Time.timeScale = 1f;         
        isPaused = false;

        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.UnpauseMusic();
            Debug.Log("PauseManager: Música reanudada.");
        }

        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;                 
        Debug.Log("PauseManager: Cursor bloqueado y oculto (juego reanudado).");
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;  
        isPaused = true;

        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.PauseMusic();
            Debug.Log("PauseManager: Música pausada.");
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.InitializeSliders();
            Debug.Log("PauseManager: Sliders de audio inicializados.");
        }
        else
        {
            Debug.LogWarning("PauseManager: AudioManager.instance es nulo al intentar inicializar sliders de pausa.");
        }

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;                
        Debug.Log("PauseManager: Cursor liberado y visible (juego pausado).");
    }

    public void LoadMenu()
    {
        Resume(); 

        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.StopMusic();
        }
        SceneManager.LoadScene("Menu"); 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}