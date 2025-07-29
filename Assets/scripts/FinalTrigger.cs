using UnityEngine;
using UnityEngine.Events;

public class FinalTrigger : MonoBehaviour
{
    
    public Animator doorAnimator; // Arrastra el GameObject de tu puerta aqu� (el que tiene el Animator)
    // �IMPORTANTE! Este nombre debe coincidir EXACTAMENTE con el PARAMETRO "Trigger" que crees en tu Animator.
    public string animationTriggerName = "endingdoor"; // <<-- �CAMBIA ESTE VALOR!

    private bool doorOpened = false; // Para asegurar que la animaci�n solo se active una vez

    // Referencia al Renderer del propio trigger (si tiene uno)
    private Renderer triggerRenderer;

    void Awake()
    {
        triggerRenderer = GetComponent<Renderer>();

        // Deshabilitar el GameObject del trigger al inicio.
        // El GameManager lo habilitar� cuando todos los enemigos sean derrotados.
        gameObject.SetActive(false);

        if (doorAnimator == null)
        {
            Debug.LogError("[DoorTrigger] No se ha asignado un Animator a la puerta en el Inspector del DoorTrigger.", this);
        }
    }

    // Este m�todo es p�blico para que el GameManager pueda llamarlo desde un UnityEvent.
    public void EnableTrigger()
    {
        // Habilita el GameObject entero del trigger.
        // Esto permite que el collider detecte entradas.
        gameObject.SetActive(true);

        // Si este GameObject tiene un Renderer (ej. si el trigger es un cubo visible),
        // lo deshabilitamos para que el trigger sea invisible en el juego.
        if (triggerRenderer != null)
        {
            triggerRenderer.enabled = false;
        }

        Debug.Log("[DoorTrigger] �Trigger de puerta HABILITADO! Todos los enemigos han sido derrotados.");
    }

    void OnTriggerEnter(Collider other)
    {
        // Aseg�rate de que solo el jugador (con la etiqueta "Player") active la puerta
        // y que la puerta no se haya abierto ya.
        if (other.CompareTag("Player") && !doorOpened)
        {
            if (doorAnimator != null)
            {
                // �Aqu� es donde se activa el par�metro Trigger en el Animator!
                doorAnimator.SetTrigger(animationTriggerName);
                doorOpened = true; // Marca la puerta como abierta para evitar re-activaciones
                Debug.Log($"[DoorTrigger] Jugador entr� en el trigger. Activando el Trigger Animator '{animationTriggerName}'.");
            }
            else
            {
                Debug.LogWarning("[DoorTrigger] El jugador entr� en el trigger, pero no hay un Animator asignado o la puerta ya est� abierta.", this);
            }
        }
    }
}