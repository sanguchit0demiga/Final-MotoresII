using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    public PlayerController playerController; 
    public Text ammoDisplay;

    private void Update()
    {
        if (playerController != null && ammoDisplay != null)
        {
            ammoDisplay.text =  playerController.ammo.ToString();
        }
    }
}
