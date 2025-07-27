using Unity.Cinemachine;
using UnityEngine;

public class CamSwitch : MonoBehaviour
{
    // Asigna AQUI TU 'Main Camera' (el GameObject que tiene Camera y CinemachineBrain)
    // Este GameObject DEBE PERMANECER SIEMPRE ACTIVO en la jerarquía.
    public GameObject mainCameraGameObject;

    // Asigna AQUI TU Cinemachine Virtual Camera para la vista Top-Down
    // Este GameObject se activará/desactivará para cambiar la vista.
    public GameObject topDownVirtualCamera;

    public Navmesh[] enemies;

    private bool isTopDownActive = false; // Para rastrear qué cámara está activa

    // --- Otros campos y métodos no relacionados con la cámara se omiten para enfocar la solución ---
    // public AudioClip newMusic;
    // ...

    private void Awake()
    {
        // Esto es útil si no asignas las referencias en el Inspector
        // Aunque siempre es mejor asignarlas manualmente si son fijas en la escena.
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
            // Usa FindAnyObjectByType para evitar la advertencia de obsoleto
            topDownVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>()?.gameObject;
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no está asignada en el Inspector y no se encontró una CinemachineVirtualCamera en la escena.", this);
            }
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
            topDownVirtualCamera.SetActive(false);
        }
        isTopDownActive = false;
        Debug.Log($"CamSwitch: Start. FPS (Main Camera) activa. Top-Down Virtual Camera inactiva. isTopDownActive: {isTopDownActive}.");
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
                    topDownVirtualCamera.SetActive(true); // Activa la Cinemachine Virtual Camera
                    isTopDownActive = true;
                    Debug.Log($"CamSwitch: Player entró al trigger. Cambiando a Top-Down. Top-Down Virtual Camera active: {topDownVirtualCamera.activeSelf}.");
                }
                else
                {
                    Debug.LogWarning("CamSwitch: No se pudo activar la cámara Top-Down porque 'Top Down Virtual Camera' es nulo.");
                }

                // Lógica de enemigos, música, etc., si pertenece aquí.
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
        // CinemachineBrain detectará que no hay Virtual Cameras prioritarias y volverá a la vista de la Main Camera.
        if (topDownVirtualCamera != null)
        {
            topDownVirtualCamera.SetActive(false); // Desactiva la Cinemachine Virtual Camera
            isTopDownActive = false; // Actualiza el estado a FPS
            Debug.Log($"CamSwitch: ResetToFPS llamado. Top-Down Virtual Camera desactivada. isTopDownActive: {isTopDownActive}.");
        }
        else
        {
            Debug.LogWarning("CamSwitch: No se pudo resetear a FPS porque 'Top Down Virtual Camera' es nulo.");
        }
    }
}