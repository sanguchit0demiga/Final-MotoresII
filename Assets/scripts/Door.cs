using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator door;

    private void OnTriggerEnter(Collider other)
    {
        door.Play("open");
    }

    private void OnTriggerExit(Collider other)
    {
        door.Play("close");
    }
}
