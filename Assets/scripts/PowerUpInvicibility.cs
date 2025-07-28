using UnityEngine;

public class PowerUpInvincibility : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Invincibility power-up collected!");
                player.ActivateInvincibility(duration);
                Destroy(gameObject); 
            }
        }
    }
}

