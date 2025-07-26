using UnityEngine;

using UnityEngine.SceneManagement;



public class PauseManager : MonoBehaviour

{



    public GameObject pausePanel;



    private bool isPaused = false;

    public MonoBehaviour playerController;



    void Update()

    {

        if (Input.GetKeyDown(KeyCode.Escape))

        {

            TogglePause();

        }

    }



    public void TogglePause()

    {

        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;



        if (isPaused)

        {

            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = true;

        }

        else

        {

            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;

        }



        AudioListener.pause = isPaused;

    }



    public void Resume()

    {

        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

    }



    public void GoToMenu()

    {

        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");

    }

    public void QuitGame()

    {

        Application.Quit();

    }

}