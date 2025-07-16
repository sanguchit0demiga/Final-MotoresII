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

