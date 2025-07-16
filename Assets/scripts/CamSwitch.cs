using Unity.Cinemachine;
using UnityEngine;

public class CamSwitch : MonoBehaviour
{
    public GameObject fpsCamera;
    public GameObject topDownCamObject;

    public Navmesh[] enemies;

    private bool isTopDown = false;

    private void Start()
    {
        fpsCamera.SetActive(true);
        topDownCamObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTopDown = !isTopDown;

            fpsCamera.SetActive(!false);     
            topDownCamObject.SetActive(true);

            foreach (var enemy in enemies)
            {
                enemy.StartFollowing();
            }
        }
    }
}