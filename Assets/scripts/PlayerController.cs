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
    private Vector3 posicionInicial; // Esta ser� la posici�n de reaparici�n

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public ParticleSystem muzzleFlash;

    public CamSwitch camSwitcher; // �Aqu� la referencia!

    public int ammo = 25;
    public int maxAmmo = 25;
    public Text ammoDisplay; // �Importante! Aseg�rate de que este es un componente Text UI

    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio; // �Este se usa para pasos? No veo la l�gica aqu�, pero est� bien.
    public AudioSource jumpAudio;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // BUSCAR EL CAMSWITCH AQU� SI NO LO ASIGNAS MANUALMENTE EN EL INSPECTOR
        if (camSwitcher == null) // Solo si no ha sido asignado ya
        {
            camSwitcher = FindFirstObjectByType<CamSwitch>();
            if (camSwitcher == null)
            {
                Debug.LogError("PlayerController: No se encontr� un CamSwitch en la escena. El reseteo de c�mara en muerte podr�a fallar.");
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxhHealth;
        posicionInicial = transform.position; // Guarda la posici�n inicial del jugador como punto de reaparici�n
        UpdateAmmoDisplay();
    }

    void Update()
    {
        CheckForWalls();

        Vector3 move = orientation.forward * input.y + orientation.right * input.x;
        move *= moveSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Peque�a fuerza hacia abajo para mantener al CharacterController pegado al suelo
            wallJumpHorizontalVelocity = Vector3.zero; // Resetea la velocidad horizontal del walljump
            if (isJumping) // Si se presion� saltar mientras estaba en el suelo
            {
                verticalVelocity = jumpForce;
                isJumping = false; // Ya se inici� el salto
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

        // L�gica de Wall Running
        isWallRunning = (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;

        if (isWallRunning)
        {
            // Calcula la direcci�n del wall run (paralela a la pared y en la direcci�n del movimiento del jugador)
            Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDirection, orientation.forward) < 0)
                wallRunDirection = -wallRunDirection; // Asegura que la direcci�n es hacia adelante

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

        // Recarga de munici�n
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
        }

        // L�gica para footstepAudio si `isMoving` y `isGrounded`
        // bool isMoving = input.magnitude > 0.1f; // Ya calculada
        // bool isGrounded = controller.isGrounded; // Ya calculada
        // Aqu� podr�as agregar l�gica para reproducir footstepAudio si `isMoving` y `isGrounded`
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
                isJumping = true; // Marca que se debe saltar en el pr�ximo Update
                if (jumpAudio != null)
                    jumpAudio.Play();
            }
            else if (isWallRunning) // Si no est� en el suelo pero est� haciendo wallrun
            {
                verticalVelocity = jumpForce; // Salto vertical
                Vector3 wallJumpDir = (wallNormal + Vector3.up).normalized; // Direcci�n lejos de la pared + arriba
                wallJumpHorizontalVelocity = wallJumpDir * wallJumpForce; // Impulso horizontal de walljump

                isWallRunning = false; // Termina el wall run
                if (jumpAudio != null)
                    jumpAudio.Play();

                // �ATENCI�N! Este bloque if (isJumping) dentro del wall run parece un duplicado o error l�gico.
                // Si ya est�s en wall run y saltas, ya manejaste el wall jump arriba.
                // Este bloque de c�digo parece ser un remanente. Lo puedes eliminar o revisar su prop�sito.
                if (isJumping) // <--- ESTO ES PROBLEM�TICO AQU�
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
            Die(); // Llama al m�todo Die()
        }
    }

    public void Die()
    {
        // 1. Deshabilitar el CharacterController para evitar colisiones y movimiento mientras reaparece
        controller.enabled = false;

        // 2. �La clave para tu problema de c�mara!
        if (camSwitcher != null)
        {
            camSwitcher.ResetToFPS(); // Asegura que la c�mara se restablezca a FPS
            Debug.Log("PlayerController: Reseteando c�mara a FPS a trav�s de CamSwitch.");
        }
        else
        {
            Debug.LogWarning("PlayerController: camSwitcher es nulo. No se pudo resetear la c�mara a FPS.");
        }

        // 3. Mover al jugador a la posici�n inicial (reaparici�n)
        transform.position = posicionInicial;
        Debug.Log("PlayerController: Jugador movido a la posici�n inicial: " + posicionInicial);


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

        // Si tienes alg�n UI de "Game Over" o "Has Muerto", aseg�rate de desactivarlo aqu�
        // O si tienes una l�gica de "Respawn Delay", esto se ejecutar�a despu�s de ese delay.
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
            Debug.Log("Sin munici�n");
            return; // No dispara si no hay munici�n
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

        Destroy(bullet, 4f); // Destruye la bala despu�s de 4 segundos
        ammo--; // Reduce la munici�n
        UpdateAmmoDisplay(); // Actualiza el UI de munici�n
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy")) // Aseg�rate de que el Tag "BulletEnemy" est� bien escrito
        {
            TakeDamage(25f);
            Destroy(collision.gameObject); // Destruye la bala del enemigo al impactar
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString(); // Muestra solo el n�mero de munici�n
        }
    }

    private void Reload()
    {
        if (reloadAudio != null)
        {
            reloadAudio.Play();
        }

        ammo = maxAmmo; // Rellena la munici�n
        UpdateAmmoDisplay(); // Actualiza el UI
        Debug.Log("Recargado");
    }
}