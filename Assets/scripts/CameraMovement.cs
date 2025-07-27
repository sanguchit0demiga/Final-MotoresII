using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float cameraHeightOffset = 1.4f; // <-- ¡NUEVA VARIABLE! Ajusta este valor en el Inspector.

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f)
            xRotation -= 360f;

        // Llama a RecenterCamera al inicio para asegurar la posición inicial correcta
        RecenterCamera();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        // ¡¡APLICAR EL OFFSET DE ALTURA A LA POSICIÓN DE LA CÁMARA!!
        if (playerBody != null)
        {
            transform.position = playerBody.position + Vector3.up * cameraHeightOffset;
        }
    }

    public void RecenterCamera()
    {
        if (playerBody != null)
        {
            // Posiciona la cámara donde está el cuerpo del jugador más el offset de altura
            transform.position = playerBody.position + Vector3.up * cameraHeightOffset;

            // Opcional: Si quieres que la rotación se reinicie a 'mirar hacia adelante'
            // Esto es útil si al reaparecer el jugador mira a una dirección inesperada.
            // Si tu playerBody ya maneja su rotación horizontal y es suficiente, puedes omitir esto.
            // transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
            // xRotation = 0f; 

            // Si necesitas que la cámara también herede la rotación horizontal del jugador al reaparecer:
            // Asegúrate de que playerBody.rotation.eulerAngles.y es la rotación que quieres.
            // Esto puede ser más complejo si el playerBody no está rotando con el mouse en este script.
            // Si playerBody.Rotate(Vector3.up * mouseX) es lo que mueve la vista horizontal,
            // entonces la cámara ya estará alineada horizontalmente.
        }
    }
}