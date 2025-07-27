using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float cameraHeightOffset = 1.4f; // <-- �NUEVA VARIABLE! Ajusta este valor en el Inspector.

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f)
            xRotation -= 360f;

        // Llama a RecenterCamera al inicio para asegurar la posici�n inicial correcta
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

        // ��APLICAR EL OFFSET DE ALTURA A LA POSICI�N DE LA C�MARA!!
        if (playerBody != null)
        {
            transform.position = playerBody.position + Vector3.up * cameraHeightOffset;
        }
    }

    public void RecenterCamera()
    {
        if (playerBody != null)
        {
            // Posiciona la c�mara donde est� el cuerpo del jugador m�s el offset de altura
            transform.position = playerBody.position + Vector3.up * cameraHeightOffset;

            // Opcional: Si quieres que la rotaci�n se reinicie a 'mirar hacia adelante'
            // Esto es �til si al reaparecer el jugador mira a una direcci�n inesperada.
            // Si tu playerBody ya maneja su rotaci�n horizontal y es suficiente, puedes omitir esto.
            // transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
            // xRotation = 0f; 

            // Si necesitas que la c�mara tambi�n herede la rotaci�n horizontal del jugador al reaparecer:
            // Aseg�rate de que playerBody.rotation.eulerAngles.y es la rotaci�n que quieres.
            // Esto puede ser m�s complejo si el playerBody no est� rotando con el mouse en este script.
            // Si playerBody.Rotate(Vector3.up * mouseX) es lo que mueve la vista horizontal,
            // entonces la c�mara ya estar� alineada horizontalmente.
        }
    }
}