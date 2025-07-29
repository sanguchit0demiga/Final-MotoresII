using UnityEngine;
using System; 
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public ParticleSystem deathParticles;
    public float detectionRange = 15f;
    public float moveSpeed = 1f;
    public int health = 50;


    protected UnityEngine.AI.NavMeshAgent agent;

    [Header("Item Drops")]
    public GameObject[] possibleDrops;
    [Range(0f, 1f)]
    public float dropChance = 0.3f;

    protected virtual void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning($"[Enemy] No se encontr� un GameObject con la etiqueta 'Player' para {gameObject.name}.", this);
            }
        }

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} no tiene un NavMeshAgent.", this);
        }
    }

    protected virtual void Update()
    {
        if (player != null && agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
        else if (player == null)
        {
            Debug.LogWarning($"[Enemy] El jugador es nulo. El enemigo {gameObject.name} no puede moverse.", this);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"[Enemy] {gameObject.name} recibi� {damage} de da�o. Vida restante: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"[Enemy] {gameObject.name} ha sido destruido. Llamando a Die().");

        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            Debug.Log($"[Enemy] Part�culas de muerte instanciadas para {gameObject.name}.");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDied(); 
            Debug.Log($"[Enemy] GameManager.Instance notificado sobre la muerte de {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"[Enemy] GameManager.Instance no encontrado. El conteo de enemigos NO se actualizar� para {gameObject.name}.");
        }

        Debug.Log($"[Enemy] Intentando dropear �tem para {gameObject.name}...");
        DropItem();

        Destroy(gameObject);
        Debug.Log($"[Enemy] {gameObject.name} GameObject destruido.");
    }

    void DropItem()
    {
        Debug.Log($"[DropItem] Iniciando DropItem() para {gameObject.name}.");

        if (possibleDrops == null)
        {
            Debug.LogError($"[DropItem] possibleDrops es NULL para {gameObject.name}. Aseg�rate de asignarlo en el Inspector.");
            return;
        }

        if (possibleDrops.Length == 0)
        {
            Debug.LogWarning($"[DropItem] No hay �tems asignados en possibleDrops (array vac�o) para {gameObject.name}. No se dropea nada.");
            return;
        }

        Debug.Log($"[DropItem] Hay {possibleDrops.Length} posibles �tems para dropear para {gameObject.name}.");

        float randomValue = UnityEngine.Random.value;
        Debug.Log($"[DropItem] Valor aleatorio generado: {randomValue:F4}. Probabilidad de drop: {dropChance:F4}.");

        if (randomValue <= dropChance)
        {
            Debug.Log($"[DropItem] �Probabilidad de drop exitosa! ({randomValue:F4} <= {dropChance:F4}).");

            int randomIndex = UnityEngine.Random.Range(0, possibleDrops.Length);
            GameObject itemToDrop = possibleDrops[randomIndex];

            if (itemToDrop != null)
            {
                Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
                Debug.Log($"[DropItem] Seleccionado �tem '{itemToDrop.name}' en �ndice {randomIndex}. Posici�n de drop: {dropPosition}.");

                GameObject instantiatedItem = Instantiate(itemToDrop, dropPosition, Quaternion.identity);
                Debug.Log($"[DropItem] �tem '{instantiatedItem.name}' instanciado para {gameObject.name}.");
            }
            else
            {
                Debug.LogError($"[DropItem] ERROR: El prefab de �tem en el �ndice {randomIndex} es NULL en possibleDrops para {gameObject.name}. �Verifica tu Inspector!");
            }
        }
        else
        {
            Debug.Log($"[DropItem] Probabilidad de drop fallida ({randomValue:F4} > {dropChance:F4}). No se drope� ning�n �tem para {gameObject.name}.");
        }
    }
}