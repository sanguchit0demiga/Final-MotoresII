using Unity.Cinemachine;
using UnityEngine;

public class CamSwitch : MonoBehaviour
{
    // Asigna AQUI TU 'Main Camera' (el GameObject que tiene Camera y CinemachineBrain)
    // Este GameObject DEBE PERMANECER SIEMPRE ACTIVO en la jerarqu�a.
    public GameObject mainCameraGameObject;

    // Asigna AQUI TU Cinemachine Virtual Camera para la vista Top-Down
    // Este GameObject se activar�/desactivar� para cambiar la vista.
    public GameObject topDownVirtualCamera;

    public Navmesh[] enemies;

    private bool isTopDownActive = false; // Para rastrear qu� c�mara est� activa

    // --- Otros campos y m�todos no relacionados con la c�mara se omiten para enfocar la soluci�n ---
    // public AudioClip newMusic;
    // ...

    private void Awake()
    {
        // Esto es �til si no asignas las referencias en el Inspector
        // Aunque siempre es mejor asignarlas manualmente si son fijas en la escena.
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

        if (topDownVirtualCamera == null)
        {
            // Usa FindAnyObjectByType para evitar la advertencia de obsoleto
            topDownVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>()?.gameObject;
            if (topDownVirtualCamera == null)
            {
                Debug.LogError("CamSwitch: La 'Top Down Virtual Camera' no est� asignada en el Inspector y no se encontr� una CinemachineVirtualCamera en la escena.", this);
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
                Debug.LogError("CamSwitch: �WARNING CR�TICO! La 'Main Camera GameObject' NO TIENE un componente CinemachineBrain. Las c�maras Cinemachine NO funcionar�n.", mainCameraGameObject);
            }
        }

        // Al inicio, la c�mara FPS (vista por defecto de Main Camera) est� activa.
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
                    Debug.Log($"CamSwitch: Player entr� al trigger. Cambiando a Top-Down. Top-Down Virtual Camera active: {topDownVirtualCamera.activeSelf}.");
                }
                else
                {
                    Debug.LogWarning("CamSwitch: No se pudo activar la c�mara Top-Down porque 'Top Down Virtual Camera' es nulo.");
                }

                // L�gica de enemigos, m�sica, etc., si pertenece aqu�.
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