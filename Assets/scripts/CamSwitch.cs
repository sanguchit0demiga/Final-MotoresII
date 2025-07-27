using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para InputAction.CallbackContext

public class CamSwitch : MonoBehaviour
{
    // Asigna AQUI TU 'Main Camera' (el GameObject que tiene Camera y CinemachineBrain)
    // Este GameObject DEBE PERMANECER SIEMPRE ACTIVO en la jerarqu�a.
    public GameObject mainCameraGameObject;

    // Asigna AQUI TU Cinemachine Virtual Camera para la vista Top-Down
    // Este campo ahora es del tipo CinemachineVirtualCamera para evitar advertencias de obsoleto.
    public CinemachineCamera topDownVirtualCamera;

    public Navmesh[] enemies;

    private bool isTopDownActive = false; // Para rastrear qu� c�mara est� activa

    // Variables para el movimiento de la c�mara Top-Down con el cursor
    public float topDownCameraMoveSpeed = 5f; // Velocidad de movimiento de la c�mara Top-Down
    private Vector2 mouseScreenPosition; // Posici�n del mouse en la pantalla
    private Vector3 topDownLookTarget; // Punto en el suelo al que la c�mara Top-Down debe mirar/seguir

    // Referencia al Player (para posibles usos, aunque la c�mara Top-Down no lo siga directamente con un offset)
    public Transform playerTransform;
    public LayerMask groundLayer; // Capa del suelo para el raycast (�Config�rala en el Inspector!)

    private void Awake()
    {
        if (mainCameraGameObject == null)
        {
            if (Camera.main != null)
            {
                mainCameraGameObject = Camera.main.gameObject;
            }
            if (mainCameraGameObject == null)
            {
                Debug.LogError("CamSwitch: La 'Main Camera GameObject' no est� asignada en el Inspector y no se encontr� una Main Camera en la escena.", this);
            }
        }

        // Usa FindFirstObjectByType para evitar la advertencia de obsoleto
        if (topDownVirtualCamera == null)
        {
            topDownVirtualCamera = FindFirstObjectByType<CinemachineCamera>();
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no est� asignada en el Inspector y no se encontr� una CinemachineVirtualCamera en la escena.", this);
            }
        }

        if (playerTransform == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("CamSwitch: No se encontr� el Transform del Player. El movimiento de c�mara Top-Down por cursor puede no funcionar correctamente.");
            }
        }
    }

    private void Start()
    {
        // Aseg�rate de que la Main Camera est� activa al inicio (siempre debe estarlo)
        if (mainCameraGameObject != null)
        {
            mainCameraGameObject.SetActive(true);
            // Verifica que tenga CinemachineBrain, que es crucial
            if (mainCameraGameObject.GetComponent<CinemachineBrain>() == null)
            {
                Debug.LogError("CamSwitch: �WARNING CR�TICO! La 'Main Camera GameObject' NO TIENE un componente CinemachineBrain. Las c�maras Cinemachine NO funcionarer�n.", mainCameraGameObject);
            }
        }

        // Al inicio, la c�mara FPS (vista por defecto de Main Camera) est� activa.
        // Por lo tanto, la Cinemachine Virtual Camera (Top-Down) debe estar inactiva.
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false); // Accede al GameObject para desactivarlo
        }
        isTopDownActive = false;
        Debug.Log($"CamSwitch: Start. FPS (Main Camera) activa. Top-Down Virtual Camera inactiva. isTopDownActive: {isTopDownActive}.");
    }

    void Update()
    {
        // Solo aplica la l�gica de movimiento de c�mara Top-Down si est� activa
        if (isTopDownActive)
        {
            HandleTopDownCameraMovement();
        }
    }

    private void HandleTopDownCameraMovement()
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        // Usar la groundLayer para el raycast
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            topDownLookTarget = hit.point;

            Vector3 currentCamPos = topDownVirtualCamera.transform.position;
            // Solo queremos mover la c�mara horizontalmente, mantener su altura actual.
            Vector3 targetCamPos = new Vector3(topDownLookTarget.x, currentCamPos.y, topDownLookTarget.z);

            topDownVirtualCamera.transform.position = Vector3.Lerp(currentCamPos, targetCamPos, topDownCameraMoveSpeed * Time.deltaTime);
        }
        // else
        // {
        //     Debug.LogWarning("CamSwitch: Raycast del mouse no golpe� la groundLayer en modo Top-Down.");
        // }
    }

    public void OnMouseLookTopDown(InputAction.CallbackContext context)
    {
        // Este m�todo ser� llamado por el sistema de Input cuando el mouse se mueva.
        // Solo actualizamos la posici�n del mouse si estamos en modo Top-Down.
        if (isTopDownActive)
        {
            mouseScreenPosition = context.ReadValue<Vector2>();
            // Debug.Log("Mouse Position (Top-Down): " + mouseScreenPosition); // Para depuraci�n
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Cambiar a Top-Down solo si no estamos ya en Top-Down
            if (!isTopDownActive)
            {
                // NO desactives mainCameraGameObject. Solo activa la Virtual Camera de Top-Down.
                if (topDownVirtualCamera != null)
                {
                    topDownVirtualCamera.gameObject.SetActive(true); // Activa la Cinemachine Virtual Camera
                    isTopDownActive = true;
                    Debug.Log($"CamSwitch: Player entr� al trigger. Cambiando a Top-Down. Top-Down Virtual Camera active: {topDownVirtualCamera.gameObject.activeSelf}.");

                    // �Importante! Ocultar el cursor para que el mouse pueda moverse libremente sin interferir con la UI (si tienes UI)
                    // Y para que el jugador vea el "lookTarget" sin ver el puntero normal del mouse
                    Cursor.lockState = CursorLockMode.None; // Liberar el cursor para moverlo
                    Cursor.visible = true; // Hacerlo visible si es necesario para un puntero custom, o dejarlo oculto para una "mira".
                }
                else
                {
                    Debug.LogWarning("CamSwitch: No se pudo activar la c�mara Top-Down porque 'Top Down Virtual Camera' es nulo.");
                }

                foreach (var enemy in enemies)
                {
                    if (enemy != null) { enemy.StartFollowing(); }
                }
            }
        }
    }

    public void ResetToFPS()
    {
        // La Main Camera (con CinemachineBrain) debe permanecer activa.
        // Para volver a FPS, simplemente desactiva la Cinemachine Virtual Camera.
        // CinemachineBrain detectar� que no hay Virtual Cameras prioritarias y volver� a la vista de la Main Camera.
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false); // Desactiva la Cinemachine Virtual Camera
            isTopDownActive = false; // Actualiza el estado a FPS
            Debug.Log($"CamSwitch: ResetToFPS llamado. Top-Down Virtual Camera desactivada. isTopDownActive: {isTopDownActive}.");

            // �Importante! Volver a bloquear y ocultar el cursor para la vista FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Debug.LogWarning("CamSwitch: No se pudo resetear a FPS porque 'Top Down Virtual Camera' es nulo.");
        }
    }

    // Este m�todo permite a otros scripts (como PlayerController) saber el estado de la c�mara.
    public bool IsTopDownActive()
    {
        return isTopDownActive;
    }
}