using UnityEngine;

public class PowerUpHeal : MonoBehaviour
{
    public float healAmount = 30f;

    void Awake()
    {
       
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider == null)
        {
            Debug.LogError($"[PowerUpHeal] ERROR: {gameObject.name} does not have a Collider component! It cannot detect triggers.", this);
        }
        else if (!ownCollider.isTrigger)
        {
            Debug.LogWarning($"[PowerUpHeal] WARNING: Collider on {gameObject.name} is NOT marked as 'Is Trigger'. This might prevent pickup.", this);
        }

     
        Rigidbody ownRigidbody = GetComponent<Rigidbody>();
        if (ownRigidbody == null)
        {
            Debug.LogWarning($"[PowerUpHeal] WARNING: {gameObject.name} does not have a Rigidbody component. Trigger detection might be inconsistent without one (if the other collider also lacks a Rigidbody).", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PowerUpHeal] OnTriggerEnter fired! Object: {gameObject.name}, Other object: {other.name}.", this);

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[PowerUpHeal] 'Other' object is tagged as 'Player'. Name: {other.name}.", other.gameObject);

        
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                Debug.Log($"[PowerUpHeal] Found PlayerController on {other.name}. Healing player by {healAmount}.", player.gameObject);
                player.Heal(healAmount);
                Destroy(gameObject);
                Debug.Log($"[PowerUpHeal] {gameObject.name} picked up and destroyed.");
            }
            else
            {
                Debug.LogWarning($"[PowerUpHeal] Player object ({other.name}) does NOT have a PlayerController script attached. Cannot heal.", other.gameObject);
            }
        }
        else
        {
            Debug.Log($"[PowerUpHeal] 'Other' object is NOT tagged as 'Player'. Tag: '{other.tag}'. Name: {other.name}.", other.gameObject);
        }
    }

   
    void Start()
    {
        Destroy(gameObject, 15f); 
    }
}