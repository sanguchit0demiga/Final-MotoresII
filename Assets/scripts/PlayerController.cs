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

    public CamSwitch camSwitcher; // �Aqu� la referencia al script CamSwitch!
    public CameraMovement playerCameraMovement; // <-- �NUEVA REFERENCIA para tu script CameraMovement!

    public int ammo = 25;
    public int maxAmmo = 25;
    public Text ammoDisplay; // �Importante! Aseg�rate de que este es un componente Text UI

    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio;
    public AudioSource jumpAudio;


    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Intenta encontrar CamSwitch si no ha sido asignado en el Inspector
        if (camSwitcher == null)
        {
            camSwitcher = FindFirstObjectByType<CamSwitch>();
            if (camSwitcher == null)
            {
                Debug.LogError("PlayerController: No se encontr� un CamSwitch en la escena. El reseteo de c�mara en muerte podr�a fallar.");
            }
        }

        // Intenta encontrar CameraMovement si no ha sido asignado en el Inspector
        // Asumiendo que CameraMovement est� en el mismo GameObject que la Main Camera (Camera.main)
        if (playerCameraMovement == null)
        {
            // Camera.main busca la c�mara con la etiqueta "MainCamera". Aseg�rate de que tu c�mara FPS la tenga.
            if (Camera.main != null)
            {
                playerCameraMovement = Camera.main.GetComponent<CameraMovement>();
            }
            if (playerCameraMovement == null)
            {
                Debug.LogError("PlayerController: No se encontr� el script 'CameraMovement' en la Main Camera. Aseg�rate de que est� adjunto a tu Main Camera GameObject.");
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

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reload();
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

                // Este bloque duplicado deber�a ser revisado o eliminado.
                // Ya la l�gica de salto desde la pared se maneja arriba.
                // if (isJumping) 
                // {
                //     verticalVelocity = jumpForce;
                //     isJumping = false;
                // }
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

        // 2. Restablecer la c�mara a FPS usando CamSwitch
        if (camSwitcher != null)
        {
            camSwitcher.ResetToFPS();
            Debug.Log("PlayerController: Reseteando c�mara a FPS a trav�s de CamSwitch.");
        }
        else
        {
            Debug.LogWarning("PlayerController: camSwitcher es nulo. No se pudo resetear la c�mara a FPS.");
        }

        // 3. Mover al jugador a la posici�n inicial (reaparici�n)
        transform.position = posicionInicial;
        Debug.Log("PlayerController: Jugador movido a la posici�n inicial: " + posicionInicial);

        // 4. �Llamar al m�todo para recentrar la c�mara FPS en la nueva posici�n del jugador!
        if (playerCameraMovement != null)
        {
            playerCameraMovement.RecenterCamera(); // Llama al m�todo del script CameraMovement
            Debug.Log("PlayerController: Recentrando la c�mara FPS despu�s de reaparecer.");
        }
        else
        {
            Debug.LogWarning("PlayerController: playerCameraMovement es nulo. No se pudo recentrar la c�mara FPS.");
        }

        // 5. Restablecer la salud
        currentHealth = maxhHealth;
        Debug.Log("PlayerController: Salud restablecida a: " + currentHealth);

        // 6. Re-habilitar el CharacterController
        controller.enabled = true;
        Debug.Log("PlayerController: CharacterController re-habilitado.");

        // Opcional: Si necesitas resetear otras cosas (velocidad, animaciones, etc.)
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
            Debug.Log("Sin munici�n");
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
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
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