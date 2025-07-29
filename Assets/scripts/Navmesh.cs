
using UnityEngine;
using UnityEngine.AI;

public class Navmesh : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    private bool canFollow = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning($"Navmesh ({gameObject.name}): No se encontró el GameObject con la etiqueta 'Player'. Asegúrate de que tu jugador tenga esa etiqueta.", this);
            }
        }


        CamSwitch camSwitch = FindAnyObjectByType<CamSwitch>();
        if (camSwitch != null && camSwitch.IsTopDownActive())
        {
            StartFollowing();
        }
    }

    private void Update()
    {
        if (canFollow && player != null)
        {
            agent.destination = player.position;
        }
    }

    public void StartFollowing()
    {
        canFollow = true;
    }
}