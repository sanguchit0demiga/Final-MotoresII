// PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Asigna tu panel/canvas del menú de pausa aquí en el Inspector

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
        pauseMenuUI.SetActive(false); // Desactiva el menú de pausa
        Time.timeScale = 1f;          // Reanuda el tiempo del juego
        isPaused = false;

        // --- Música: Reanudar ---
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.UnpauseMusic();
            Debug.Log("PauseManager: Música reanudada.");
        }

        // --- Control del Cursor: Bloquear y ocultar ---
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla
        Cursor.visible = false;                   // Oculta el cursor
        Debug.Log("PauseManager: Cursor bloqueado y oculto (juego reanudado).");
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // Activa el menú de pausa
        Time.timeScale = 0f;         // Pausa el tiempo del juego
        isPaused = true;

        // --- Música: Pausar ---
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.PauseMusic();
            Debug.Log("PauseManager: Música pausada.");
        }

        // --- Sliders: Inicializar al abrir el menú de pausa ---
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

    // Ejemplo de otros botones en el menú de pausa
    public void LoadMenu()
    {
        Resume(); // Asegúrate de reanudar antes de cargar una nueva escena
        // Opcional: Detener completamente la música si no quieres que el menú tenga la misma música que el nivel
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.StopMusic();
        }
        SceneManager.LoadScene("Menu"); // Carga tu escena de menú principal

        // Cuando cargas una escena de menú, el cursor ya debería estar visible
        // y sin lock, pero es buena práctica asegurarlo si cambias de escena.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}