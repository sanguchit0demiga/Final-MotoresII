using Unity.Cinemachine;
using UnityEngine;
using System.Collections; // Aunque no lo uses, es buena pr�ctica si te lo sugiere el IDE

public class CamSwitch : MonoBehaviour
{
    // Asigna aqu� tu GameObject de la c�mara FPS est�ndar
    // Debe tener un componente 'Camera' de Unity.
    public GameObject fpsCamera;

    // Asigna aqu� tu GameObject de la Cinemachine Virtual Camera (la Top-Down)
    // Debe tener un componente 'CinemachineVirtualCamera'.
    public GameObject topDownCamObject;

    public Navmesh[] enemies;

    // Usamos 'isTopDownActive' para indicar si la c�mara Top-Down est� en uso
    private bool isTopDownActive = false;
    public AudioClip newMusic;

    // Referencia al MusicManagerScript (aseg�rate de que existe o b�rralo si no lo usas)
    // private MusicManagerScript musicManager; 

    private void Awake()
    {
        // if (musicManager == null)
        // {
        //     musicManager = FindObjectOfType<MusicManagerScript>();
        // }

        // A�adir una verificaci�n de referencias cruciales
        if (fpsCamera == null) Debug.LogError("CamSwitch: fpsCamera NO ASIGNADA en el Inspector.", this);
        if (topDownCamObject == null) Debug.LogError("CamSwitch: topDownCamObject NO ASIGNADA en el Inspector.", this);
    }

    private void Start()
    {
        // Aseg�rate de un estado inicial claro
        // Al inicio, la c�mara FPS debe estar activa y la Top-Down inactiva
        fpsCamera.SetActive(true);
        topDownCamObject.SetActive(false);
        isTopDownActive = false;
        Debug.Log($"CamSwitch: Start. FPS Camera Active: {fpsCamera.activeSelf}, Top-Down Cam Active: {topDownCamObject.activeSelf}.");

        // Verifica si la Main Camera tiene CinemachineBrain al inicio
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.GetComponent<CinemachineBrain>() == null)
            {
                Debug.LogError("CamSwitch: La Main Camera NO TIENE un componente CinemachineBrain. Las c�maras Cinemachine no funcionar�n correctamente.", mainCam.gameObject);
            }
        }
        else
        {
            Debug.LogError("CamSwitch: No se encontr� la Main Camera en la escena. �Necesitas una para que las c�maras funcionen!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Solo cambia de c�mara si no estamos ya en Top-Down
            if (!isTopDownActive)
            {
                // Desactiva la c�mara FPS est�ndar
                fpsCamera.SetActive(false);
                // Activa la Cinemachine Virtual Camera (Top-Down)
                topDownCamObject.SetActive(true);
                isTopDownActive = true;

                Debug.Log($"CamSwitch: Player entr� al trigger. Cambiando a Top-Down (Cinemachine). " +
                          $"FPS Cam active: {fpsCamera.activeSelf}, Top-Down Cam active: {topDownCamObject.activeSelf}.");

                // L�gica para cambiar m�sica
                if (AudioManager.instance != null && newMusic != null)
                {
                    if (MusicManagerScript.instance != null) // Asumiendo que MusicManagerScript existe
                    {
                        MusicManagerScript.instance.SetMusicClipAndPlay(newMusic);
                    }
                    else
                    {
                        Debug.LogWarning("CamSwitch: MusicManagerScript.instance es nulo, no se pudo cambiar la m�sica.");
                    }
                }

                // Iniciar seguimiento de enemigos
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        enemy.StartFollowing();
                    }
                }
            }
            // Si el trigger es para toggle (cambiar de ida y vuelta), esta es la l�gica alternativa:
            // else {
            //     ResetToFPS(); // Si ya est� en Top-Down, vuelve a FPS
            // }
        }
    }

    public void ResetToFPS()
    {
        // Siempre asegura que la c�mara FPS est�ndar est� activa
        fpsCamera.SetActive(true);
        // Siempre asegura que la Cinemachine Virtual Camera est� inactiva
        topDownCamObject.SetActive(false);
        isTopDownActive = false; // Actualiza el estado a FPS

        Debug.Log($"CamSwitch: ResetToFPS llamado. C�mara FPS activa: {fpsCamera.activeSelf}, Top-Down Cam activa: {topDownCamObject.activeSelf}.");

        // Opcional: Volver a la m�sica normal del nivel
        // if (MusicManagerScript.instance != null)
        // {
        //     MusicManagerScript.instance.PlayLevelMusic(); 
        // }
    }
}