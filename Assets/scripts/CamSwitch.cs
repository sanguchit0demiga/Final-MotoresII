using Unity.Cinemachine;
using UnityEngine;
using System.Collections; // Aunque no lo uses, es buena práctica si te lo sugiere el IDE

public class CamSwitch : MonoBehaviour
{
    // Asigna aquí tu GameObject de la cámara FPS estándar
    // Debe tener un componente 'Camera' de Unity.
    public GameObject fpsCamera;

    // Asigna aquí tu GameObject de la Cinemachine Virtual Camera (la Top-Down)
    // Debe tener un componente 'CinemachineVirtualCamera'.
    public GameObject topDownCamObject;

    public Navmesh[] enemies;

    // Usamos 'isTopDownActive' para indicar si la cámara Top-Down está en uso
    private bool isTopDownActive = false;
    public AudioClip newMusic;

    // Referencia al MusicManagerScript (asegúrate de que existe o bórralo si no lo usas)
    // private MusicManagerScript musicManager; 

    private void Awake()
    {
        // if (musicManager == null)
        // {
        //     musicManager = FindObjectOfType<MusicManagerScript>();
        // }

        // Añadir una verificación de referencias cruciales
        if (fpsCamera == null) Debug.LogError("CamSwitch: fpsCamera NO ASIGNADA en el Inspector.", this);
        if (topDownCamObject == null) Debug.LogError("CamSwitch: topDownCamObject NO ASIGNADA en el Inspector.", this);
    }

    private void Start()
    {
        // Asegúrate de un estado inicial claro
        // Al inicio, la cámara FPS debe estar activa y la Top-Down inactiva
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
                Debug.LogError("CamSwitch: La Main Camera NO TIENE un componente CinemachineBrain. Las cámaras Cinemachine no funcionarán correctamente.", mainCam.gameObject);
            }
        }
        else
        {
            Debug.LogError("CamSwitch: No se encontró la Main Camera en la escena. ¡Necesitas una para que las cámaras funcionen!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Solo cambia de cámara si no estamos ya en Top-Down
            if (!isTopDownActive)
            {
                // Desactiva la cámara FPS estándar
                fpsCamera.SetActive(false);
                // Activa la Cinemachine Virtual Camera (Top-Down)
                topDownCamObject.SetActive(true);
                isTopDownActive = true;

                Debug.Log($"CamSwitch: Player entró al trigger. Cambiando a Top-Down (Cinemachine). " +
                          $"FPS Cam active: {fpsCamera.activeSelf}, Top-Down Cam active: {topDownCamObject.activeSelf}.");

                // Lógica para cambiar música
                if (AudioManager.instance != null && newMusic != null)
                {
                    if (MusicManagerScript.instance != null) // Asumiendo que MusicManagerScript existe
                    {
                        MusicManagerScript.instance.SetMusicClipAndPlay(newMusic);
                    }
                    else
                    {
                        Debug.LogWarning("CamSwitch: MusicManagerScript.instance es nulo, no se pudo cambiar la música.");
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
            // Si el trigger es para toggle (cambiar de ida y vuelta), esta es la lógica alternativa:
            // else {
            //     ResetToFPS(); // Si ya está en Top-Down, vuelve a FPS
            // }
        }
    }

    public void ResetToFPS()
    {
        // Siempre asegura que la cámara FPS estándar está activa
        fpsCamera.SetActive(true);
        // Siempre asegura que la Cinemachine Virtual Camera está inactiva
        topDownCamObject.SetActive(false);
        isTopDownActive = false; // Actualiza el estado a FPS

        Debug.Log($"CamSwitch: ResetToFPS llamado. Cámara FPS activa: {fpsCamera.activeSelf}, Top-Down Cam activa: {topDownCamObject.activeSelf}.");

        // Opcional: Volver a la música normal del nivel
        // if (MusicManagerScript.instance != null)
        // {
        //     MusicManagerScript.instance.PlayLevelMusic(); 
        // }
    }
}