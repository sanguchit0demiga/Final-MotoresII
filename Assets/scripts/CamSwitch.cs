using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para InputAction.CallbackContext

public class CamSwitch : MonoBehaviour
{
    // Asigna AQUI TU 'Main Camera' (el GameObject que tiene Camera y CinemachineBrain)
    // Este GameObject DEBE PERMANECER SIEMPRE ACTIVO en la jerarqu�a.
    public GameObject mainCameraGameObject;

    // Asigna AQUI TU Cinemachine Virtual Camera para la vista Top-Down
    public CinemachineCamera topDownVirtualCamera;

    public Navmesh[] enemies; // Aseg�rate de que Navmesh sea accesible o reempl�zalo con el tipo correcto de script de enemigo.

    private bool isTopDownActive = false; // Para rastrear qu� c�mara est� activa

    // Variables para el movimiento de la c�mara Top-Down con el cursor
    public float topDownCameraMoveSpeed = 5f; // Velocidad de movimiento de la c�mara Top-Down
    private Vector2 mouseScreenPosition; // Posici�n del mouse en la pantalla
    private Vector3 topDownLookTarget; // Punto en el suelo al que la c�mara Top-Down debe mirar/seguir

    // Referencia al Player (para posibles usos, aunque la c�mara Top-Down no lo siga directamente con un offset)
    public Transform playerTransform;
    public LayerMask groundLayer; // Capa del suelo para el raycast (�Config�rala en el Inspector!)

    // --- NUEVA REFERENCIA AL MUSICMANAGER ---
    private MusicManagerScript musicManager;

    public EnemySpawner enemySpawner;
    // ----------------------------------------

    // --- NUEVA REFERENCIA AL GAMEOBJECT DEL TEXTO DE ENEMIGOS ---
    [Header("UI y Contador de Enemigos")] // Encabezado para organizar en el Inspector
    [Tooltip("El GameObject que contiene el TextMeshProUGUI del contador de enemigos. �Desactivar por defecto en el Inspector!")]
    public GameObject enemyCountTextGameObject;
    // --- FIN NUEVA REFERENCIA ---

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

        // Usa FindObjectOfType para evitar la advertencia de obsoleto
        if (topDownVirtualCamera == null)
        {
            topDownVirtualCamera = FindAnyObjectByType<CinemachineCamera>(); // Cambiado a FindObjectOfType
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no est� asignada en el Inspector y no se encontr� una CinemachineCamera en la escena.", this);
            }
        }

        if (playerTransform == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>(); // Cambiado a FindObjectOfType
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("CamSwitch: No se encontr� el Transform del Player. El movimiento de c�mara Top-Down por cursor puede no funcionar correctamente.");
            }
        }

        // Obtener la instancia del MusicManagerScript
        musicManager = MusicManagerScript.instance;
        if (musicManager == null)
        {
            Debug.LogError("CamSwitch: No se encontr� la instancia de MusicManagerScript. Aseg�rate de que haya uno en la escena y que tenga su script y DontDestroyOnLoad.");
        }

        // �Verificaci�n importante para el GameObject del texto!
        if (enemyCountTextGameObject == null)
        {
            Debug.LogError("CamSwitch: El GameObject del texto del contador de enemigos (enemyCountTextGameObject) NO est� asignado en el Inspector. El texto no se mostrar�.", this);
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
                Debug.LogError("CamSwitch: �WARNING CR�TICO! La 'Main Camera GameObject' NO TIENE un componente CinemachineBrain. Las c�maras Cinemachine NO funcionar�n.", mainCameraGameObject);
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- Aseg�rate de que el GameObject del texto est� DESACTIVADO al inicio ---
        if (enemyCountTextGameObject != null)
        {
            enemyCountTextGameObject.SetActive(false);
            Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado al inicio.");
        }
        // --- FIN ---
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
        // Aseg�rate de que Camera.main no sea nulo antes de usarlo. Esto se resuelve al etiquetar tu c�mara como "MainCamera".
        if (Camera.main == null)
        {
            Debug.LogError("CamSwitch: Camera.main es nula. Aseg�rate de que tu c�mara principal tenga la etiqueta 'MainCamera'.");
            return;
        }

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
    }

    public void OnMouseLookTopDown(InputAction.CallbackContext context)
    {
        // Este m�todo ser� llamado por el sistema de Input cuando el mouse se mueva.
        // Solo actualizamos la posici�n del mouse si estamos en modo Top-Down.
        if (isTopDownActive)
        {
            mouseScreenPosition = context.ReadValue<Vector2>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isTopDownActive)
            {
                if (topDownVirtualCamera != null)
                {
                    topDownVirtualCamera.gameObject.SetActive(true);
                    isTopDownActive = true;
                    Debug.Log($"CamSwitch: Player entr� al trigger. Cambiando a Top-Down. Top-Down Virtual Camera active: {topDownVirtualCamera.gameObject.activeSelf}.");

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    // --- ACTIVAR EL GAMEOBJECT DEL TEXTO CUANDO EL JUGADOR PISA EL TRIGGER ---
                    if (enemyCountTextGameObject != null)
                    {
                        enemyCountTextGameObject.SetActive(true);
                        Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha activado.");
                    }
                    // --- FIN ---

                    // Cambio de m�sica
                    if (musicManager != null && musicManager.topDownMusicClip != null)
                    {
                        musicManager.SetMusicClipAndPlay(musicManager.topDownMusicClip);
                        Debug.Log("CamSwitch: Solicitando cambio de m�sica a Top-Down Music.");
                    }
                    else if (musicManager == null)
                    {
                        Debug.LogWarning("CamSwitch: MusicManager es nulo, no se pudo cambiar la m�sica a Top-Down.");
                    }
                    else if (musicManager.topDownMusicClip == null)
                    {
                        Debug.LogWarning("CamSwitch: topDownMusicClip no est� asignado en MusicManager.");
                    }

                    // Activar el spawner de enemigos
                    if (enemySpawner != null)
                    {
                        enemySpawner.StartSpawner();
                        Debug.Log("CamSwitch: EnemySpawner activado.");
                    }
                    else
                    {
                        Debug.LogWarning("CamSwitch: enemySpawner no asignado en el Inspector.");
                    }

                    // Activar movimiento de enemigos si est�n asignados
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null)
                        {
                            // Asegurate de que exista este m�todo en los enemigos (Navmesh en este caso)
                            enemy.StartFollowing();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("CamSwitch: No se pudo activar la c�mara Top-Down porque 'Top Down Virtual Camera' es nulo.");
                }
            }
        }
    }


    // ELIMINADO: OnTriggerExit para que la c�mara y m�sica Top-Down persistan.
    // El cambio de vuelta a FPS y la m�sica original ahora solo ocurrir� al llamar a ResetToFPS().

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

            // --- DESACTIVAR EL GAMEOBJECT DEL TEXTO CUANDO SE VUELVE A FPS ---
            if (enemyCountTextGameObject != null)
            {
                enemyCountTextGameObject.SetActive(false);
                Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado.");
            }
            // --- FIN ---

            // --- VOLVER A LA M�SICA ORIGINAL DEL NIVEL ---
            if (musicManager != null && musicManager.GetCurrentLevelMusicClip() != null)
            {
                musicManager.SetMusicClipAndPlay(musicManager.GetCurrentLevelMusicClip());
                Debug.Log("CamSwitch: Volviendo a la m�sica del Nivel 1.");
            }
            else if (musicManager == null)
            {
                Debug.LogWarning("CamSwitch: MusicManager es nulo, no se pudo volver a la m�sica del nivel.");
            }
            else if (musicManager.GetCurrentLevelMusicClip() == null)
            {
                Debug.LogWarning("CamSwitch: CurrentLevelMusicClip no est� asignado en MusicManager, no se pudo volver a la m�sica del nivel.");
            }
            // --------------------------------------------
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