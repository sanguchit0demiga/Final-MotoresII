using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = 20f;
    public float jumpForce = 8f;
    public float wallRunGravity = 2f;
    public float wallRunSpeed = 8f;
    public float wallJumpForce = 10f;
    public float wallCheckDistance = 1f;

    private CharacterController controller;
    private Vector2 input;
    private float verticalVelocity;
    private bool isJumping = false;

    private bool isWallRunning = false;
    private bool wallLeft, wallRight;

    public Transform orientation;
    public LayerMask wallLayer;

    private Vector3 wallNormal;
    private Vector3 wallJumpHorizontalVelocity = Vector3.zero;

    private float maxhHealth = 100f;
    public float currentHealth;
    private Vector3 posicionInicial;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxhHealth;
        posicionInicial = transform.position;
    }
    void Update()
    {
        CheckForWalls();

        Vector3 move = orientation.forward * input.y + orientation.right * input.x;
        move *= moveSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            wallJumpHorizontalVelocity = Vector3.zero;
            if (isJumping)
            {
                verticalVelocity = jumpForce;
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity -= (isWallRunning ? wallRunGravity : gravity) * Time.deltaTime;
        }

        if (wallJumpHorizontalVelocity.magnitude > 0.1f)
        {
            move += wallJumpHorizontalVelocity;
            wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, 10f * Time.deltaTime);
        }

        isWallRunning = (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;

        if (isWallRunning)
        {
            Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDirection, orientation.forward) < 0)
                wallRunDirection = -wallRunDirection;

            Vector3 wallRunMove = wallRunDirection * wallRunSpeed;
            wallRunMove.y = verticalVelocity;

            Vector3 finalMove = wallRunMove + move * 0.5f;

            controller.Move(finalMove * Time.deltaTime);
        }
        else
        {
            move.y = verticalVelocity;
            controller.Move(move * Time.deltaTime);
        }
    }


    void CheckForWalls()
    {
        RaycastHit hit;

        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight) wallNormal = hit.normal;

        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallLeft) wallNormal = hit.normal;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (controller.isGrounded)
            {
                isJumping = true;
            }
            else if (isWallRunning)
            {
                verticalVelocity = jumpForce;

                Vector3 wallJumpDir = (wallNormal + Vector3.up).normalized;
                wallJumpHorizontalVelocity = wallJumpDir * wallJumpForce;

                isWallRunning = false;
                if (isJumping)
                {
                    Debug.Log("SALTANDO");
                    verticalVelocity = jumpForce;
                    isJumping = false;
                }
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        Debug.Log("vida actual: " + currentHealth);
        if (currentHealth <= 0)
        {

            Die();

        }
    }
    public void Die()
    {

        controller.enabled = false;

        transform.position = posicionInicial;
        currentHealth = maxhHealth;

        controller.enabled = true;

    }
    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }

        
        Destroy(bullet, 6f);
    }
}
