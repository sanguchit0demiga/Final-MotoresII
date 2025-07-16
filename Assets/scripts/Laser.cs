using UnityEngine;

public class Laser : MonoBehaviour
{
   public int laserDamage = 100;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by laser!");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(laserDamage);
            }
        }
    }
}
