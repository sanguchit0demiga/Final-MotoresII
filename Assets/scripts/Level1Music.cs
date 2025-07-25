using UnityEngine;

public class Level1Music : MonoBehaviour
{
    public AudioClip level1Music;
    void Start()
    {
        if (MusicController.instance != null)
        {
            MusicController.instance.PlayMusic(level1Music);
        }
    }

  
}
