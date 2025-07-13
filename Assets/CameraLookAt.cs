using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, -5f); 
    public float smoothSpeed = 5f;
    public float cursorOffsetAmount = 2f; 

    void LateUpdate()
    {
        Vector3 cursorOffset = GetCursorOffset();

        Vector3 desiredPosition = player.position + offset + cursorOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    Vector3 GetCursorOffset()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePosition = Input.mousePosition;

        Vector2 offsetFromCenter = (mousePosition - screenCenter) / screenCenter;
        offsetFromCenter = Vector2.ClampMagnitude(offsetFromCenter, 1f);

        Vector3 result = new Vector3(offsetFromCenter.x, offsetFromCenter.y, 0f) * cursorOffsetAmount;
        return result;
    }
}