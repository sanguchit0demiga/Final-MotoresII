using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para InputAction.CallbackContext

public class CamSwitch : MonoBehaviour
{
    // Asigna AQUI TU 'Main Camera' (el GameObject que tiene Camera y CinemachineBrain)
    // Este GameObject DEBE PERMANECER SIEMPRE ACTIVO en la jerarquía.
    public GameObject mainCameraGameObject;

    // Asigna AQUI TU Cinemachine Virtual Camera para la vista Top-Down
    public CinemachineCamera topDownVirtualCamera;

    public Navmesh[] enemies; // Asegúrate de que Navmesh sea accesible o reemplázalo con el tipo correcto de script de enemigo.

    private bool isTopDownActive = false; // Para rastrear qué cámara está activa

    // Variables para el movimiento de la cámara Top-Down con el cursor
    public float topDownCameraMoveSpeed = 5f; // Velocidad de movimiento de la cámara Top-Down
    private Vector2 mouseScreenPosition; // Posición del mouse en la pantalla
    private Vector3 topDownLookTarget; // Punto en el suelo al que la cámara Top-Down debe mirar/seguir

    // Referencia al Player (para posibles usos, aunque la cámara Top-Down no lo siga directamente con un offset)
    public Transform playerTransform;
    public LayerMask groundLayer; // Capa del suelo para el raycast (¡Configúrala en el Inspector!)

    // --- NUEVA REFERENCIA AL MUSICMANAGER ---
    private MusicManagerScript musicManager;

    public EnemySpawner enemySpawner;
    // ----------------------------------------

    // --- NUEVA REFERENCIA AL GAMEOBJECT DEL TEXTO DE ENEMIGOS ---
    [Header("UI y Contador de Enemigos")] // Encabezado para organizar en el Inspector
    [Tooltip("El GameObject que contiene el TextMeshProUGUI del contador de enemigos. ¡Desactivar por defecto en el Inspector!")]
    public GameObject enemyCountTextGameObject;
    // --- FIN NUEVA REFERENCIA ---
    public Animator playerAnimator;       // Animator del jugador
    public GameObject arma;               // Arma del jugador
    public GameObject extraText1;         // Primer texto adicional
    public GameObject extraText2;         // Segundo texto adicional
    public GameObject extraImage;
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
                Debug.LogError("CamSwitch: La 'Main Camera GameObject' no está asignada en el Inspector y no se encontró una Main Camera en la escena.", this);
            }
        }

        // Usa FindObjectOfType para evitar la advertencia de obsoleto
        if (topDownVirtualCamera == null)
        {
            topDownVirtualCamera = FindAnyObjectByType<CinemachineCamera>(); // Cambiado a FindObjectOfType
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no está asignada en el Inspector y no se encontró una CinemachineCamera en la escena.", this);
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
                Debug.LogWarning("CamSwitch: No se encontró el Transform del Player. El movimiento de cámara Top-Down por cursor puede no funcionar correctamente.");
            }
        }

        // Obtener la instancia del MusicManagerScript
        musicManager = MusicManagerScript.instance;
        if (musicManager == null)
        {
            Debug.LogError("CamSwitch: No se encontró la instancia de MusicManagerScript. Asegúrate de que haya uno en la escena y que tenga su script y DontDestroyOnLoad.");
        }

        // ¡Verificación importante para el GameObject del texto!
        if (enemyCountTextGameObject == null)
        {
            Debug.LogError("CamSwitch: El GameObject del texto del contador de enemigos (enemyCountTextGameObject) NO está asignado en el Inspector. El texto no se mostrará.", this);
        }
    }

    private void Start()
    {
        // Asegúrate de que la Main Camera esté activa al inicio (siempre debe estarlo)
        if (mainCameraGameObject != null)
        {
            mainCameraGameObject.SetActive(true);
            // Verifica que tenga CinemachineBrain, que es crucial
            if (mainCameraGameObject.GetComponent<CinemachineBrain>() == null)
            {
                Debug.LogError("CamSwitch: ¡WARNING CRÍTICO! La 'Main Camera GameObject' NO TIENE un componente CinemachineBrain. Las cámaras Cinemachine NO funcionarán.", mainCameraGameObject);
            }
        }

        // Al inicio, la cámara FPS (vista por defecto de Main Camera) está activa.
        // Por lo tanto, la Cinemachine Virtual Camera (Top-Down) debe estar inactiva.
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false); // Accede al GameObject para desactivarlo
        }
        isTopDownActive = false;
        Debug.Log($"CamSwitch: Start. FPS (Main Camera) activa. Top-Down Virtual Camera inactiva. isTopDownActive: {isTopDownActive}.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- Asegúrate de que el GameObject del texto esté DESACTIVADO al inicio ---
        if (enemyCountTextGameObject != null)
        {
            enemyCountTextGameObject.SetActive(false);
            Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado al inicio.");
        }
        // --- FIN ---
    }

    void Update()
    {
        // Solo aplica la lógica de movimiento de cámara Top-Down si está activa
        if (isTopDownActive)
        {
            HandleTopDownCameraMovement();
        }
    }

    private void HandleTopDownCameraMovement()
    {
        // Asegúrate de que Camera.main no sea nulo antes de usarlo. Esto se resuelve al etiquetar tu cámara como "MainCamera".
        if (Camera.main == null)
        {
            Debug.LogError("CamSwitch: Camera.main es nula. Asegúrate de que tu cámara principal tenga la etiqueta 'MainCamera'.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        // Usar la groundLayer para el raycast
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            topDownLookTarget = hit.point;

            Vector3 currentCamPos = topDownVirtualCamera.transform.position;
            // Solo queremos mover la cámara horizontalmente, mantener su altura actual.
            Vector3 targetCamPos = new Vector3(topDownLookTarget.x, currentCamPos.y, topDownLookTarget.z);

            topDownVirtualCamera.transform.position = Vector3.Lerp(currentCamPos, targetCamPos, topDownCameraMoveSpeed * Time.deltaTime);
        }
    }

    public void OnMouseLookTopDown(InputAction.CallbackContext context)
    {
        // Este método será llamado por el sistema de Input cuando el mouse se mueva.
        // Solo actualizamos la posición del mouse si estamos en modo Top-Down.
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
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.canShoot = true; // ✅ Disparo habilitado
                Debug.Log("CamSwitch: Disparo habilitado para el jugador en top-down.");
            }

            if (topDownVirtualCamera != null)
            {
                topDownVirtualCamera.gameObject.SetActive(true);
                isTopDownActive = true;
                    if (playerAnimator != null)
                        playerAnimator.enabled = true;

                    if (arma != null)
                        arma.SetActive(true);

                    if (extraText1 != null)
                        extraText1.SetActive(true);

                    if (extraText2 != null)
                        extraText2.SetActive(true);

                    if (extraImage != null)
                        extraImage.SetActive(true);
                    Debug.Log($"CamSwitch: Player entró al trigger. Cambiando a Top-Down. Top-Down Virtual Camera active: {topDownVirtualCamera.gameObject.activeSelf}.");

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (enemyCountTextGameObject != null)
                {
                    enemyCountTextGameObject.SetActive(true);
                    Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha activado.");
                }

                if (musicManager != null && musicManager.topDownMusicClip != null)
                {
                    musicManager.SetMusicClipAndPlay(musicManager.topDownMusicClip);
                    Debug.Log("CamSwitch: Solicitando cambio de música a Top-Down Music.");
                }

                if (enemySpawner != null)
                {
                    enemySpawner.StartSpawner();
                    Debug.Log("CamSwitch: EnemySpawner activado.");
                }

                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        enemy.StartFollowing();
                    }
                }
            }
            else
            {
                Debug.LogWarning("CamSwitch: No se pudo activar la cámara Top-Down porque 'Top Down Virtual Camera' es nulo.");
            }
        }
    }
}


    // ELIMINADO: OnTriggerExit para que la cámara y música Top-Down persistan.
    // El cambio de vuelta a FPS y la música original ahora solo ocurrirá al llamar a ResetToFPS().

    public void ResetToFPS()
    {
        // La Main Camera (con CinemachineBrain) debe permanecer activa.
        // Para volver a FPS, simplemente desactiva la Cinemachine Virtual Camera.
        // CinemachineBrain detectará que no hay Virtual Cameras prioritarias y volverá a la vista de la Main Camera.
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false); // Desactiva la Cinemachine Virtual Camera
            isTopDownActive = false; // Actualiza el estado a FPS
            Debug.Log($"CamSwitch: ResetToFPS llamado. Top-Down Virtual Camera desactivada. isTopDownActive: {isTopDownActive}.");

            // ¡Importante! Volver a bloquear y ocultar el cursor para la vista FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // --- DESACTIVAR EL GAMEOBJECT DEL TEXTO CUANDO SE VUELVE A FPS ---
            if (enemyCountTextGameObject != null)
            {
                enemyCountTextGameObject.SetActive(false);
                Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado.");
            }
            // --- FIN ---

            // --- VOLVER A LA MÚSICA ORIGINAL DEL NIVEL ---
            if (musicManager != null && musicManager.GetCurrentLevelMusicClip() != null)
            {
                musicManager.SetMusicClipAndPlay(musicManager.GetCurrentLevelMusicClip());
                Debug.Log("CamSwitch: Volviendo a la música del Nivel 1.");
            }
            else if (musicManager == null)
            {
                Debug.LogWarning("CamSwitch: MusicManager es nulo, no se pudo volver a la música del nivel.");
            }
            else if (musicManager.GetCurrentLevelMusicClip() == null)
            {
                Debug.LogWarning("CamSwitch: CurrentLevelMusicClip no está asignado en MusicManager, no se pudo volver a la música del nivel.");
            }
            // --------------------------------------------
        }
        else
        {
            Debug.LogWarning("CamSwitch: No se pudo resetear a FPS porque 'Top Down Virtual Camera' es nulo.");
        }
    }

    // Este método permite a otros scripts (como PlayerController) saber el estado de la cámara.
    public bool IsTopDownActive()
    {
        return isTopDownActive;
    }
}