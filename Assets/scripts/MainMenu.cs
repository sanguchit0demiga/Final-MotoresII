using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnplayPressed()
    {
        SceneManager.LoadScene("SelectLevel");
    }
    public void OnOptionsPressed()
    {
        SceneManager.LoadScene("Options");
    }
    public void OnExitPressed()
    {
        Application.Quit();

    }
}
