using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections; 

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
    private Vector3 wallNormal;
    private Vector3 wallJumpHorizontalVelocity = Vector3.zero;

    public Transform orientation; 
    public LayerMask wallLayer; 
    public LayerMask groundLayer;
    public CamSwitch camSwitcher; 
    public CameraMovement playerCameraMovement;
    private Transform fpsCameraTransform; 

    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBarUI healthBarUI; 
    private Vector3 posicionInicial;

    public GameObject bulletPrefab; 
    public Transform firePoint; 
    public float bulletForce = 20f; 
    public float fireRate = 0.2f; 
    private float nextFireTime = 0f; 
    public ParticleSystem muzzleFlash; 
    public float normalBulletDamage = 10f; 
    public float boostedBulletDamage = 20f; 
    private bool damageBoosted = false; 

    public AudioSource gunShotAudio; 
    public AudioSource reloadAudio; 
    public AudioSource footstepAudio; 
    public AudioSource jumpAudio;
    public AudioSource deathAudioSource; 
    public AudioClip deathSoundClip; 
    public float delayBeforeDefeatScene = 0.5f; 

 
    [Header("Low Ammo Sound")]
    [Tooltip("Sonido de disparo cuando quedan 1 o 2 balas.")]
    public AudioClip lowAmmoSound;
  

    
    public int ammo = 25; 
    public int maxAmmo = 25; 
    public Text ammoDisplay; 
    public string gameOverSceneName = "Defeat"; 


    private bool isDead = false; 
    public bool isInvincible = false; 
    public Renderer playerRenderer; 
    private Color originalColor; 
    public GameObject playerWeapon; 
    private bool wasTopDownMode = false; 
    private Vector2 mouseScreenPosition;
    private Vector3 lookTarget;

    public PowerUpUI powerUpUI; 
    private Animator animator;
    public bool canShoot = false;
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (Camera.main != null)
        {
            fpsCameraTransform = Camera.main.transform;
            playerCameraMovement = Camera.main.GetComponent<CameraMovement>();
        }
        else
        {
            Debug.LogWarning("[PlayerController] No se encontró la cámara principal ('MainCamera' tag). Algunas funcionalidades de cámara y arma pueden no funcionar.");
        }

        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("[PlayerController] Player Renderer no asignado. No se podrá cambiar el color con la invencibilidad.");
        }

        if (deathAudioSource == null) deathAudioSource = GetComponent<AudioSource>();
        if (camSwitcher == null) camSwitcher = FindAnyObjectByType<CamSwitch>();
        if (healthBarUI == null) healthBarUI = FindAnyObjectByType<HealthBarUI>();
        if (powerUpUI == null) powerUpUI = FindAnyObjectByType<PowerUpUI>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
        currentHealth = maxHealth; 
        posicionInicial = transform.position; 
        UpdateAmmoDisplay(); 
        healthBarUI?.SetMaxHealth(maxHealth); 
        healthBarUI?.SetHealth(currentHealth); 


        if (playerWeapon != null && fpsCameraTransform != null)
        {
            playerWeapon.transform.SetParent(fpsCameraTransform);
        }

        wasTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();
    }

    void Update()
    {
        CheckForWalls(); 
        if (isDead) return; 

        HandleCameraSwitch(); 
        HandleMovement(); 

        if (Keyboard.current.rKey.wasPressedThisFrame) Reload();
    }

    private void HandleCameraSwitch()
    {
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (playerWeapon != null && camSwitcher != null)
        {

            if (isTopDownMode && !wasTopDownMode)
            {
                playerWeapon.transform.SetParent(transform); 
            }
            else if (!isTopDownMode && wasTopDownMode)
            {
                playerWeapon.transform.SetParent(fpsCameraTransform); 

            }
        }
        wasTopDownMode = isTopDownMode; 
    }

    private void HandleMovement()
    {
        Vector3 move;
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (!isTopDownMode)
        {
            move = orientation.forward * input.y + orientation.right * input.x;
        }
        else
        {
            move = new Vector3(input.x, 0f, input.y).normalized;
            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget); 
        }

        Vector3 horizontalMove = new Vector3(move.x, 0f, move.z);
        float speed = horizontalMove.magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsRunning", speed > 0.1f);
        }

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

        // Lógica de WallRun
        isWallRunning = !isTopDownMode && (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;
        if (isWallRunning)
        {
            Vector3 wallRunDir = Vector3.Cross(wallNormal, Vector3.up);
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDir, orientation.forward) < 0)
                wallRunDir = -wallRunDir;

            Vector3 finalMove = (wallRunDir * wallRunSpeed + move * 0.5f);
            finalMove.y = verticalVelocity;
            controller.Move(finalMove * Time.deltaTime);
        }
        else
        {
            move.y = verticalVelocity;

            if (wallJumpHorizontalVelocity.magnitude > 0.1f)
            {
                move += wallJumpHorizontalVelocity;
                wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, 10f * Time.deltaTime);
            }

            controller.Move(move * Time.deltaTime);
        }
    }



    void CheckForWalls()
    {
        if (camSwitcher != null && camSwitcher.IsTopDownActive()) { wallLeft = wallRight = false; return; }

        RaycastHit hit;

        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight) wallNormal = hit.normal;
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallLeft) wallNormal = hit.normal; 
    }
    public void OnMove(InputAction.CallbackContext ctx) => input = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return; 

        if ((camSwitcher == null || !camSwitcher.IsTopDownActive()) && (controller.isGrounded || isWallRunning))
        {
            verticalVelocity = jumpForce; 
            isJumping = controller.isGrounded; 
            if (isWallRunning) wallJumpHorizontalVelocity = (wallNormal + Vector3.up).normalized * wallJumpForce; 
            jumpAudio?.Play(); 
        }
    }

    public void OnMouseLook(InputAction.CallbackContext ctx)
    {
        if (camSwitcher != null && camSwitcher.IsTopDownActive())
        {
            mouseScreenPosition = ctx.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition); 
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                lookTarget = hit.point;
        }
        else if (playerCameraMovement != null && ctx.phase == InputActionPhase.Performed)
        {
            playerCameraMovement.ReceiveMouseInput(ctx.ReadValue<Vector2>());
        }
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (canShoot && ctx.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (ammo <= 0)
        {
            Debug.LogWarning("[PlayerController] No hay munición. No se puede disparar.");
            return;
        }

        muzzleFlash?.Play(); 

        if ((ammo == 2 || ammo == 1) && lowAmmoSound != null)
        {
            gunShotAudio?.PlayOneShot(lowAmmoSound);
            Debug.Log($"[PlayerController] Reproduciendo sonido de poca munición ({ammo} balas restantes).");
        }
        else 
        {
            gunShotAudio?.Play(); 
            Debug.Log("[PlayerController] Reproduciendo sonido de disparo normal.");
        }

        Vector3 dir = (camSwitcher != null && camSwitcher.IsTopDownActive()) ? transform.forward : Camera.main.transform.forward;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            float damage = damageBoosted ? boostedBulletDamage : normalBulletDamage;
            bulletScript.SetDamage(damage);
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb?.AddForce(dir * bulletForce, ForceMode.Impulse);

        Destroy(bullet, 4f);
        ammo--;
        UpdateAmmoDisplay(); 
    }

    private void Reload()
    {
        reloadAudio?.Play(); 
        ammo = maxAmmo;
        UpdateAmmoDisplay(); 
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString(); 
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy"))
        {
            if (!isInvincible) 
            {
                TakeDamage(15f); 
            }
            Destroy(collision.gameObject); 
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(15f);
        }
    }



    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; 
        currentHealth = Mathf.Max(currentHealth, 0);
        healthBarUI?.SetHealth(currentHealth); 
        if (currentHealth <= 0) Die(); 
    }

    public void Die()
    {
        if (isDead) return; 

        isDead = true; 
        controller.enabled = false; 
        MusicManagerScript.instance?.StopMusic();

        Time.timeScale = 0f;
        Debug.Log("[PlayerController] Juego pausado al morir.");

        if (deathAudioSource != null && deathSoundClip != null)
        {
            deathAudioSource.PlayOneShot(deathSoundClip);

            StartCoroutine(UnpauseAndLoadDefeatSceneAfterDelay(delayBeforeDefeatScene));
        }
        else
        {
            LoadDefeatScene();
        }
    }

    private IEnumerator UnpauseAndLoadDefeatSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        while (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            yield return null; 
        }

       
        Time.timeScale = 1f;
        Debug.Log("[PlayerController] Juego despausado para cargar escena de derrota.");

        LoadDefeatScene(); 
    }

    private void LoadDefeatScene()
    {
        Time.timeScale = 1f;
        camSwitcher?.ResetToFPS(); 
        playerCameraMovement?.RecenterCamera(); 
        SceneManager.LoadScene("Defeat"); 
    }


    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); 
        healthBarUI?.SetHealth(currentHealth); 
    }

    public void ActivateInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration)); 

        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.invincibilityOverlay, powerUpUI.invincibilityUI, duration);
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        if (playerRenderer != null) playerRenderer.material.color = Color.blue; 
        yield return new WaitForSeconds(duration); 
        isInvincible = false; 
        if (playerRenderer != null) playerRenderer.material.color = originalColor; 
    }

    public void ActivateDamageBoost(float duration)
    {
        StartCoroutine(DamageBoostCoroutine(duration)); 

        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.damageOverlay, powerUpUI.damageUI, duration);
    }

    private IEnumerator DamageBoostCoroutine(float duration)
    {
        damageBoosted = true; 
        yield return new WaitForSeconds(duration); 
        damageBoosted = false; 
    }
}