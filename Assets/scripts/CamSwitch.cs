using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; 

public class CamSwitch : MonoBehaviour
{
 
    public GameObject mainCameraGameObject;


    public CinemachineCamera topDownVirtualCamera;

    public Navmesh[] enemies; 

    private bool isTopDownActive = false; 

    
    public float topDownCameraMoveSpeed = 5f; 
    private Vector2 mouseScreenPosition;
    private Vector3 topDownLookTarget; 

    public Transform playerTransform;
    public LayerMask groundLayer; 

    private MusicManagerScript musicManager;

    public EnemySpawner enemySpawner;


    public GameObject enemyCountTextGameObject;

    public Animator playerAnimator;     
    public GameObject arma;             
    public GameObject extraText1;        
    public GameObject extraText2;         
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

        if (topDownVirtualCamera == null)
        {
            topDownVirtualCamera = FindAnyObjectByType<CinemachineCamera>();
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no está asignada en el Inspector y no se encontró una CinemachineCamera en la escena.", this);
            }
        }

        if (playerTransform == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>(); 
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("CamSwitch: No se encontró el Transform del Player. El movimiento de cámara Top-Down por cursor puede no funcionar correctamente.");
            }
        }

        musicManager = MusicManagerScript.instance;
        if (musicManager == null)
        {
            Debug.LogError("CamSwitch: No se encontró la instancia de MusicManagerScript. Asegúrate de que haya uno en la escena y que tenga su script y DontDestroyOnLoad.");
        }

        if (enemyCountTextGameObject == null)
        {
            Debug.LogError("CamSwitch: El GameObject del texto del contador de enemigos (enemyCountTextGameObject) NO está asignado en el Inspector. El texto no se mostrará.", this);
        }
    }

    private void Start()
    {
        if (mainCameraGameObject != null)
        {
            mainCameraGameObject.SetActive(true);
            if (mainCameraGameObject.GetComponent<CinemachineBrain>() == null)
            {
                Debug.LogError("CamSwitch: ¡WARNING CRÍTICO! La 'Main Camera GameObject' NO TIENE un componente CinemachineBrain. Las cámaras Cinemachine NO funcionarán.", mainCameraGameObject);
            }
        }

        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false);
        }
        isTopDownActive = false;
        Debug.Log($"CamSwitch: Start. FPS (Main Camera) activa. Top-Down Virtual Camera inactiva. isTopDownActive: {isTopDownActive}.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (enemyCountTextGameObject != null)
        {
            enemyCountTextGameObject.SetActive(false);
            Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado al inicio.");
        }
    }

    void Update()
    {
        if (isTopDownActive)
        {
            HandleTopDownCameraMovement();
        }
    }

    private void HandleTopDownCameraMovement()
    {
        if (Camera.main == null)
        {
            Debug.LogError("CamSwitch: Camera.main es nula. Asegúrate de que tu cámara principal tenga la etiqueta 'MainCamera'.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            topDownLookTarget = hit.point;

            Vector3 currentCamPos = topDownVirtualCamera.transform.position;
            Vector3 targetCamPos = new Vector3(topDownLookTarget.x, currentCamPos.y, topDownLookTarget.z);

            topDownVirtualCamera.transform.position = Vector3.Lerp(currentCamPos, targetCamPos, topDownCameraMoveSpeed * Time.deltaTime);
        }
    }

    public void OnMouseLookTopDown(InputAction.CallbackContext context)
    {
     
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
                playerController.canShoot = true; 
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

    public void ResetToFPS()
    {
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.gameObject.SetActive(false); 
            isTopDownActive = false; 
            Debug.Log($"CamSwitch: ResetToFPS llamado. Top-Down Virtual Camera desactivada. isTopDownActive: {isTopDownActive}.");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (enemyCountTextGameObject != null)
            {
                enemyCountTextGameObject.SetActive(false);
                Debug.Log("CamSwitch: El GameObject del contador de enemigos se ha desactivado.");
            }
            
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
        }
        else
        {
            Debug.LogWarning("CamSwitch: No se pudo resetear a FPS porque 'Top Down Virtual Camera' es nulo.");
        }
    }
    public bool IsTopDownActive()
    {
        return isTopDownActive;
    }
}