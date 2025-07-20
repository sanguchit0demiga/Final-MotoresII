using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    public void OnAudioPressed()
    {
        SceneManager.LoadScene("Audio");
    }
    public void OnControlsPressed()
    {
        SceneManager.LoadScene("Controls");
    }
    public void OnBackPressed()
    {
        SceneManager.LoadScene("Menu");
    }
}
