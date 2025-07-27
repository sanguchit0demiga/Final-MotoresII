using UnityEngine;
using UnityEngine.SceneManagement;

public class DefeatController : MonoBehaviour
{
    void Start()
    {
       
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void OnRetryPressed()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OnMenuPressed()
    {
        SceneManager.LoadScene("Menu");
    }
    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
