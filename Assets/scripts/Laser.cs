using UnityEngine;

public class Laser : MonoBehaviour
{
   public int laserDamage = 100;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(laserDamage);
            }
        }
    }
}
