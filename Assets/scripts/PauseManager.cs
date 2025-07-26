// PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Asigna tu panel/canvas del men� de pausa aqu� en el Inspector

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
        pauseMenuUI.SetActive(false); // Desactiva el men� de pausa
        Time.timeScale = 1f;          // Reanuda el tiempo del juego
        isPaused = false;

        // --- M�sica: Reanudar ---
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.UnpauseMusic();
            Debug.Log("PauseManager: M�sica reanudada.");
        }

        // --- Control del Cursor: Bloquear y ocultar ---
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla
        Cursor.visible = false;                   // Oculta el cursor
        Debug.Log("PauseManager: Cursor bloqueado y oculto (juego reanudado).");
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // Activa el men� de pausa
        Time.timeScale = 0f;         // Pausa el tiempo del juego
        isPaused = true;

        // --- M�sica: Pausar ---
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.PauseMusic();
            Debug.Log("PauseManager: M�sica pausada.");
        }

        // --- Sliders: Inicializar al abrir el men� de pausa ---
        if (AudioManager.instance != null)
        {
            AudioManager.instance.InitializeSliders();
            Debug.Log("PauseManager: Sliders de audio inicializados.");
        }
        else
        {
            Debug.LogWarning("PauseManager: AudioManager.instance es nulo al intentar inicializar sliders de pausa.");
        }

        // --- Control del Cursor: Liberar y mostrar ---
        Cursor.lockState = CursorLockMode.None; // Libera el cursor
        Cursor.visible = true;                  // Muestra el cursor
        Debug.Log("PauseManager: Cursor liberado y visible (juego pausado).");
    }

    // Ejemplo de otros botones en el men� de pausa
    public void LoadMenu()
    {
        Resume(); // Aseg�rate de reanudar antes de cargar una nueva escena
        // Opcional: Detener completamente la m�sica si no quieres que el men� tenga la misma m�sica que el nivel
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.StopMusic();
        }
        SceneManager.LoadScene("Menu"); // Carga tu escena de men� principal

        // Cuando cargas una escena de men�, el cursor ya deber�a estar visible
        // y sin lock, pero es buena pr�ctica asegurarlo si cambias de escena.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}