using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsManager : MonoBehaviour
{
   public void OnBackPressed()
    {
        SceneManager.LoadScene("Options");
    }
}
