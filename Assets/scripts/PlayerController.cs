using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necesario para Text

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

    public Transform orientation; // Este es para la vista FPS.
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
    public ParticleSystem muzzleFlash;

    public CamSwitch camSwitcher; // Referencia al script CamSwitch
    public CameraMovement playerCameraMovement; // Referencia al script CameraMovement

    public int ammo = 25;
    public int maxAmmo = 25;
    public Text ammoDisplay;

    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio;
    public AudioSource jumpAudio;

    // --- NUEVAS VARIABLES PARA ROTACIÓN TOP-DOWN ---
    private Vector2 mouseScreenPosition; // Posición del mouse en la pantalla
    private Vector3 lookTarget;          // Punto en el suelo al que el jugador debe mirar
    public LayerMask groundLayer;        // ¡Asegúrate de configurar esta capa en el Inspector!
    // ----------------------------------------------


    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (camSwitcher == null)
        {
            camSwitcher = FindFirstObjectByType<CamSwitch>(); // Usa FindFirstObjectByType
            if (camSwitcher == null)
            {
                Debug.LogError("PlayerController: No se encontró un CamSwitch en la escena. El reseteo de cámara en muerte podría fallar.");
            }
        }

        if (playerCameraMovement == null)
        {
            if (Camera.main != null)
            {
                playerCameraMovement = Camera.main.GetComponent<CameraMovement>();
            }
            if (playerCameraMovement == null)
            {
                Debug.LogError("PlayerController: No se encontró el script 'CameraMovement' en la Main Camera. Asegúrate de que esté adjunto a tu Main Camera GameObject.");
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Cursor bloqueado para FPS al inicio
        Cursor.visible = false;

        currentHealth = maxhHealth;
        posicionInicial = transform.position;
        UpdateAmmoDisplay();
    }

    void Update()
    {
        CheckForWalls();

        // Obtener el estado actual de la cámara del CamSwitch
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive(); // Necesitamos un método IsTopDownActive en CamSwitch

        Vector3 move;

        if (!isTopDownMode) // Lógica de movimiento FPS
        {
            move = orientation.forward * input.y + orientation.right * input.x;
            move *= moveSpeed;
        }
        else // Lógica de movimiento y rotación Top-Down
        {
            move = new Vector3(input.x, 0f, input.y).normalized * moveSpeed; // Movimiento relativo al mundo

            // Rotación del personaje para mirar al cursor
            lookTarget.y = transform.position.y; // Mantener la altura del jugador para el LookAt
            transform.LookAt(lookTarget); // El jugador mira hacia el punto del cursor
        }

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

        // La lógica de Wall Running solo se aplica en modo FPS
        isWallRunning = !isTopDownMode && (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;

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

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
        }
    }


    void CheckForWalls()
    {
        // Solo comprueba paredes en modo FPS
        if (camSwitcher == null || !camSwitcher.IsTopDownActive())
        {
            RaycastHit hit;

            wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
            if (wallRight) wallNormal = hit.normal;

            wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
            if (wallLeft) wallNormal = hit.normal;
        }
        else
        {
            wallLeft = false;
            wallRight = false;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // La lógica de salto de pared solo para FPS
        if (camSwitcher == null || !camSwitcher.IsTopDownActive())
        {
            if (ctx.started)
            {
                if (controller.isGrounded)
                {
                    isJumping = true;
                    if (jumpAudio != null)
                        jumpAudio.Play();
                }
                else if (isWallRunning)
                {
                    verticalVelocity = jumpForce;
                    Vector3 wallJumpDir = (wallNormal + Vector3.up).normalized;
                    wallJumpHorizontalVelocity = wallJumpDir * wallJumpForce;

                    isWallRunning = false;
                    if (jumpAudio != null)
                        jumpAudio.Play();
                }
            }
        }
        // En modo Top-Down, el salto podría ser simplemente un salto vertical normal.
        else if (ctx.started && controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            if (jumpAudio != null)
                jumpAudio.Play();
        }
    }

    // --- MÉTODO PARA MANEJAR EL MOUSE LOOK EN TOP-DOWN ---
    public void OnMouseLook(InputAction.CallbackContext context)
    {
        // Solo actualizamos la posición del mouse y el lookTarget si estamos en modo Top-Down
        if (camSwitcher != null && camSwitcher.IsTopDownActive())
        {
            mouseScreenPosition = context.ReadValue<Vector2>();

            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
            RaycastHit hit;

            // Lanzar un rayo y verificar si golpea algo en la 'groundLayer'
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                lookTarget = hit.point;
            }
            else
            {
                // Esto te ayudará a depurar si el rayo no golpea nada.
                // Asegúrate de que tu suelo tenga un collider y la capa 'Ground' asignada.
                Debug.LogWarning("PlayerController: Raycast del mouse no golpeó la groundLayer. Asegúrate de que el suelo tenga un collider y la capa 'Ground'.");
            }
        }
    }
    // -------------------------------------------------------------

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

        if (camSwitcher != null)
        {
            camSwitcher.ResetToFPS();
            Debug.Log("PlayerController: Reseteando cámara a FPS a través de CamSwitch.");
        }
        else
        {
            Debug.LogWarning("PlayerController: camSwitcher es nulo. No se pudo resetear la cámara a FPS.");
        }

        transform.position = posicionInicial;
        Debug.Log("PlayerController: Jugador movido a la posición inicial: " + posicionInicial);

        if (playerCameraMovement != null)
        {
            playerCameraMovement.RecenterCamera();
            Debug.Log("PlayerController: Recentrando la cámara FPS después de reaparecer.");
        }
        else
        {
            Debug.LogWarning("PlayerController: playerCameraMovement es nulo. No se pudo recentrar la cámara FPS.");
        }

        currentHealth = maxhHealth;
        Debug.Log("PlayerController: Salud restablecida a: " + currentHealth);

        controller.enabled = true;
        Debug.Log("PlayerController: CharacterController re-habilitado.");

        verticalVelocity = 0f;
        isJumping = false;
        isWallRunning = false;
        wallJumpHorizontalVelocity = Vector3.zero;
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
        if (ammo <= 0)
        {
            Debug.Log("Sin munición");
            return;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if (gunShotAudio != null)
        {
            gunShotAudio.Play();
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // La bala siempre debería disparar hacia donde el jugador está mirando.
            rb.AddForce(transform.forward * bulletForce, ForceMode.Impulse);
        }

        Destroy(bullet, 4f);
        ammo--;
        UpdateAmmoDisplay();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy"))
        {
            TakeDamage(25f);
            Destroy(collision.gameObject);
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString();
        }
    }

    private void Reload()
    {
        if (reloadAudio != null)
        {
            reloadAudio.Play();
        }

        ammo = maxAmmo;
        UpdateAmmoDisplay();
        Debug.Log("Recargado");
    }
}