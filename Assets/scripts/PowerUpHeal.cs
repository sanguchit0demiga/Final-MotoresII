using UnityEngine;

public class PowerUpHeal : MonoBehaviour
{
    public float healAmount = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}