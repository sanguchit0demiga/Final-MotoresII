using UnityEngine;

public class PowerUpDamageBoost : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateDamageBoost(duration);
                Destroy(gameObject);
            }
        }
    }
}