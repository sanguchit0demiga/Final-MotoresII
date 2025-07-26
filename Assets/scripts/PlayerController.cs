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

    public Transform orientation;
    public LayerMask wallLayer;

    private Vector3 wallNormal;
    private Vector3 wallJumpHorizontalVelocity = Vector3.zero;

    private float maxhHealth = 100f;
    public float currentHealth;
    private Vector3 posicionInicial; // Esta será la posición de reaparición

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public ParticleSystem muzzleFlash;

    public CamSwitch camSwitcher; // ¡Aquí la referencia!

    public int ammo = 25;
    public int maxAmmo = 25;
    public Text ammoDisplay; // ¡Importante! Asegúrate de que este es un componente Text UI

    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio; // ¿Este se usa para pasos? No veo la lógica aquí, pero está bien.
    public AudioSource jumpAudio;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // BUSCAR EL CAMSWITCH AQUÍ SI NO LO ASIGNAS MANUALMENTE EN EL INSPECTOR
        if (camSwitcher == null) // Solo si no ha sido asignado ya
        {
            camSwitcher = FindFirstObjectByType<CamSwitch>();
            if (camSwitcher == null)
            {
                Debug.LogError("PlayerController: No se encontró un CamSwitch en la escena. El reseteo de cámara en muerte podría fallar.");
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxhHealth;
        posicionInicial = transform.position; // Guarda la posición inicial del jugador como punto de reaparición
        UpdateAmmoDisplay();
    }

    void Update()
    {
        CheckForWalls();

        Vector3 move = orientation.forward * input.y + orientation.right * input.x;
        move *= moveSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Pequeña fuerza hacia abajo para mantener al CharacterController pegado al suelo
            wallJumpHorizontalVelocity = Vector3.zero; // Resetea la velocidad horizontal del walljump
            if (isJumping) // Si se presionó saltar mientras estaba en el suelo
            {
                verticalVelocity = jumpForce;
                isJumping = false; // Ya se inició el salto
            }
        }
        else // En el aire (saltando o cayendo)
        {
            verticalVelocity -= (isWallRunning ? wallRunGravity : gravity) * Time.deltaTime;
        }

        // Aplicar la velocidad horizontal del walljump si existe
        if (wallJumpHorizontalVelocity.magnitude > 0.1f)
        {
            move += wallJumpHorizontalVelocity;
            // Disminuir la velocidad del walljump con el tiempo
            wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, 10f * Time.deltaTime);
        }

        // Lógica de Wall Running
        isWallRunning = (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;

        if (isWallRunning)
        {
            // Calcula la dirección del wall run (paralela a la pared y en la dirección del movimiento del jugador)
            Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDirection, orientation.forward) < 0)
                wallRunDirection = -wallRunDirection; // Asegura que la dirección es hacia adelante

            Vector3 wallRunMove = wallRunDirection * wallRunSpeed;
            wallRunMove.y = verticalVelocity; // Mantener la gravedad controlada en wall run

            Vector3 finalMove = wallRunMove + move * 0.5f; // Mezcla el movimiento del input con el wall run
            controller.Move(finalMove * Time.deltaTime);
        }
        else
        {
            move.y = verticalVelocity; // Aplica la gravedad normal
            controller.Move(move * Time.deltaTime);
        }

        // Recarga de munición
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
        }

        // Lógica para footstepAudio si `isMoving` y `isGrounded`
        // bool isMoving = input.magnitude > 0.1f; // Ya calculada
        // bool isGrounded = controller.isGrounded; // Ya calculada
        // Aquí podrías agregar lógica para reproducir footstepAudio si `isMoving` y `isGrounded`
    }


    void CheckForWalls()
    {
        RaycastHit hit;

        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight) wallNormal = hit.normal; // Guarda la normal de la pared derecha

        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallLeft) wallNormal = hit.normal; // Guarda la normal de la pared izquierda
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
                isJumping = true; // Marca que se debe saltar en el próximo Update
                if (jumpAudio != null)
                    jumpAudio.Play();
            }
            else if (isWallRunning) // Si no está en el suelo pero está haciendo wallrun
            {
                verticalVelocity = jumpForce; // Salto vertical
                Vector3 wallJumpDir = (wallNormal + Vector3.up).normalized; // Dirección lejos de la pared + arriba
                wallJumpHorizontalVelocity = wallJumpDir * wallJumpForce; // Impulso horizontal de walljump

                isWallRunning = false; // Termina el wall run
                if (jumpAudio != null)
                    jumpAudio.Play();

                // ¡ATENCIÓN! Este bloque if (isJumping) dentro del wall run parece un duplicado o error lógico.
                // Si ya estás en wall run y saltas, ya manejaste el wall jump arriba.
                // Este bloque de código parece ser un remanente. Lo puedes eliminar o revisar su propósito.
                if (isJumping) // <--- ESTO ES PROBLEMÁTICO AQUÍ
                {
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
            Die(); // Llama al método Die()
        }
    }

    public void Die()
    {
        // 1. Deshabilitar el CharacterController para evitar colisiones y movimiento mientras reaparece
        controller.enabled = false;

        // 2. ¡La clave para tu problema de cámara!
        if (camSwitcher != null)
        {
            camSwitcher.ResetToFPS(); // Asegura que la cámara se restablezca a FPS
            Debug.Log("PlayerController: Reseteando cámara a FPS a través de CamSwitch.");
        }
        else
        {
            Debug.LogWarning("PlayerController: camSwitcher es nulo. No se pudo resetear la cámara a FPS.");
        }

        // 3. Mover al jugador a la posición inicial (reaparición)
        transform.position = posicionInicial;
        Debug.Log("PlayerController: Jugador movido a la posición inicial: " + posicionInicial);


        // 4. Restablecer la salud
        currentHealth = maxhHealth;
        Debug.Log("PlayerController: Salud restablecida a: " + currentHealth);


        // 5. Re-habilitar el CharacterController
        controller.enabled = true;
        Debug.Log("PlayerController: CharacterController re-habilitado.");

        // Opcional: Si necesitas resetear otras cosas (velocidad, animaciones, etc.)
        verticalVelocity = 0f; // Asegurarse de que no caiga al instante
        isJumping = false;
        isWallRunning = false;
        wallJumpHorizontalVelocity = Vector3.zero;

        // Si tienes algún UI de "Game Over" o "Has Muerto", asegúrate de desactivarlo aquí
        // O si tienes una lógica de "Respawn Delay", esto se ejecutaría después de ese delay.
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
            return; // No dispara si no hay munición
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
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }

        Destroy(bullet, 4f); // Destruye la bala después de 4 segundos
        ammo--; // Reduce la munición
        UpdateAmmoDisplay(); // Actualiza el UI de munición
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy")) // Asegúrate de que el Tag "BulletEnemy" esté bien escrito
        {
            TakeDamage(25f);
            Destroy(collision.gameObject); // Destruye la bala del enemigo al impactar
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString(); // Muestra solo el número de munición
        }
    }

    private void Reload()
    {
        if (reloadAudio != null)
        {
            reloadAudio.Play();
        }

        ammo = maxAmmo; // Rellena la munición
        UpdateAmmoDisplay(); // Actualiza el UI
        Debug.Log("Recargado");
    }
}