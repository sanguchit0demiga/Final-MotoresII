using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necesario para Text
using UnityEngine.SceneManagement; // Necesario para cambiar de escena
using System.Collections; // ¡MUY IMPORTANTE: Necesario para Coroutines!

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

    public float maxHealth = 100f;
    public float currentHealth;
    private Vector3 posicionInicial;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public ParticleSystem muzzleFlash;

    public CamSwitch camSwitcher;
    public CameraMovement playerCameraMovement;

    public int ammo = 25;
    public int maxAmmo = 25; // Asumo que este es el que quieres para la munición máxima
    public Text ammoDisplay;

    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio;
    public AudioSource jumpAudio;

    private Vector2 mouseScreenPosition;
    private Vector3 lookTarget;
    public LayerMask groundLayer;

    public HealthBarUI healthBarUI;
    public string gameOverSceneName = "GameOverScene";

    // --- Variables de Audio de Muerte ---
    public AudioSource deathAudioSource; // AudioSource para el sonido de muerte
    public AudioClip deathSoundClip;     // El clip de audio del sonido de muerte
    public float delayBeforeDefeatScene = 0.5f; // <<<<<< ¡IMPORTANTE: Agregamos esta variable de nuevo!
    // -------------------------------------

    // --- Variables para el manejo del arma ---
    public GameObject playerWeapon;
    private Transform fpsCameraTransform;
    private bool wasTopDownMode = false;
    private bool isDead = false;

    // ------------------------------------------

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (camSwitcher == null)
        {
            // Preferir FindObjectOfType para obtener la primera instancia si solo hay una
            camSwitcher = FindAnyObjectByType<CamSwitch>();
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

        if (healthBarUI == null)
        {
            // Preferir FindObjectOfType para obtener la primera instancia si solo hay una
            healthBarUI = FindAnyObjectByType<HealthBarUI>();
            if (healthBarUI == null)
            {
                Debug.LogWarning("PlayerController: No se encontró un HealthBarUI en la escena. La barra de vida de la UI no se actualizará.");
            }
        }

        if (Camera.main != null)
        {
            fpsCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("PlayerController: No se encontró la Main Camera. El arma no podrá seguir la cámara FPS.");
        }

        // Opcional: Si deathAudioSource no se asigna, intenta obtenerlo del mismo GameObject
        // Esto es útil si el AudioSource está en el mismo objeto que el PlayerController.
        if (deathAudioSource == null)
        {
            deathAudioSource = GetComponent<AudioSource>();
            if (deathAudioSource == null)
            {
                Debug.LogWarning("PlayerController: No se encontró un AudioSource para el sonido de muerte en este GameObject. Asigna uno en el Inspector o asegúrate de que esté adjunto al mismo GameObject.");
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxHealth;
        posicionInicial = transform.position;
        UpdateAmmoDisplay();

        if (healthBarUI != null)
        {
            healthBarUI.SetMaxHealth(maxHealth);
            healthBarUI.SetHealth(currentHealth);
        }

        if (playerWeapon != null && fpsCameraTransform != null)
        {
            playerWeapon.transform.SetParent(fpsCameraTransform);
            // playerWeapon.transform.localPosition = new Vector3(0.5f, -0.4f, 0.8f);
            // playerWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
        }

        wasTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();
    }

    void Update()
    {
        CheckForWalls();
         if (isDead) return;

        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (playerWeapon != null && camSwitcher != null)
        {
            if (isTopDownMode && !wasTopDownMode)
            {
                playerWeapon.transform.SetParent(transform);
                // playerWeapon.transform.localPosition = new Vector3(0.2f, 0.5f, 0f); 
                // playerWeapon.transform.localRotation = Quaternion.identity; 
            }
            else if (!isTopDownMode && wasTopDownMode)
            {
                if (fpsCameraTransform != null)
                {
                    playerWeapon.transform.SetParent(fpsCameraTransform);
                    // playerWeapon.transform.localPosition = new Vector3(0.5f, -0.4f, 0.8f);
                    // playerWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }
        wasTopDownMode = isTopDownMode;

        Vector3 move;

        if (!isTopDownMode)
        {
            move = orientation.forward * input.y + orientation.right * input.x;
            move *= moveSpeed;
        }
        else
        {
            move = new Vector3(input.x, 0f, input.y).normalized * moveSpeed;

            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget);
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
        else if (ctx.started && controller.isGrounded)
        {
            verticalVelocity = jumpForce;
            if (jumpAudio != null)
                jumpAudio.Play();
        }
    }

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        if (camSwitcher != null && camSwitcher.IsTopDownActive())
        {
            mouseScreenPosition = context.ReadValue<Vector2>();

            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                lookTarget = hit.point;
            }
            else
            {
                Debug.LogWarning("PlayerController: Raycast del mouse no golpeó la groundLayer. Asegúrate de que el suelo tenga un collider y la capa 'Ground'.");
            }
        }
        else
        {
            if (playerCameraMovement != null && context.phase == InputActionPhase.Performed)
            {
                playerCameraMovement.ReceiveMouseInput(context.ReadValue<Vector2>());
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("vida actual: " + currentHealth);

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        controller.enabled = false;

       
        if (MusicManagerScript.instance != null)
        {
            MusicManagerScript.instance.StopMusic();
        }

        if (deathAudioSource != null && deathSoundClip != null)
        {
            deathAudioSource.PlayOneShot(deathSoundClip);
            StartCoroutine(LoadDefeatSceneAfterDelay(delayBeforeDefeatScene));
        }
        else
        {
            LoadDefeatScene();
        }
    }

    // --- Corrutina para esperar antes de cargar la escena ---
    IEnumerator LoadDefeatSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Espera adicional si el audio sigue sonando
        while (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            yield return null; // Espera al siguiente frame
        }

        SceneManager.LoadScene("Defeat");
    }

    // --- Método para cargar la escena de derrota (centralizado) ---
    private void LoadDefeatScene()
    {
        if (camSwitcher != null)
        {
            camSwitcher.ResetToFPS();
            Debug.Log("PlayerController: Reseteando cámara a FPS a través de CamSwitch.");
        }
        else
        {
            Debug.LogWarning("PlayerController: camSwitcher es nulo. No se pudo resetear la cámara a FPS.");
        }

        if (playerCameraMovement != null)
        {
            playerCameraMovement.RecenterCamera();
            Debug.Log("PlayerController: Recentrando la cámara FPS después de reaparecer.");
        }
        else
        {
            Debug.LogWarning("PlayerController: playerCameraMovement es nulo. No se pudo recentrar la cámara FPS.");
        }

        Debug.Log("PlayerController: ¡El jugador ha muerto! Cargando escena de derrota...");

        // Cargar directamente la escena "Defeat"
        SceneManager.LoadScene("Defeat");
    }
    // ---------------------------------------------------------------

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

        Vector3 shootDirection;
        Quaternion bulletRotation;

        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (isTopDownMode)
        {
            shootDirection = transform.forward;
            shootDirection.y = 0;
            shootDirection.Normalize();
            bulletRotation = Quaternion.LookRotation(shootDirection);
        }
        else
        {
            shootDirection = Camera.main.transform.forward;
            bulletRotation = Camera.main.transform.rotation;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * bulletForce, ForceMode.Impulse);
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

        ammo = maxAmmo; // Usar maxAmmo
        UpdateAmmoDisplay();
        Debug.Log("Recargado");
    }
}