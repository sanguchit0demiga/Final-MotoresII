using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
   public void OnLevel1Pressed()
    {
        SceneManager.LoadScene("Level1");
    }
    public void OnBackPressed()
    {
        SceneManager.LoadScene("Menu");
    }
}

