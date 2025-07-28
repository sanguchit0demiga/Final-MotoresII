using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // --- Variables de Movimiento y WallRun ---
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

    // --- FPS / Cámara / Layer ---
    public Transform orientation;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public CamSwitch camSwitcher;
    public CameraMovement playerCameraMovement;
    private Transform fpsCameraTransform;

    // --- Salud ---
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBarUI healthBarUI;
    private Vector3 posicionInicial;

    // --- Arma y Disparo ---
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;
    public ParticleSystem muzzleFlash;
    public float normalBulletDamage = 10f;
    public float boostedBulletDamage = 20f;
    private bool damageBoosted = false;

    // --- Audio ---
    public AudioSource gunShotAudio;
    public AudioSource reloadAudio;
    public AudioSource footstepAudio;
    public AudioSource jumpAudio;
    public AudioSource deathAudioSource;
    public AudioClip deathSoundClip;
    public float delayBeforeDefeatScene = 0.5f;

    // --- UI ---
    public int ammo = 25;
    public int maxAmmo = 25;
    public Text ammoDisplay;
    public string gameOverSceneName = "Defeat";

    // --- Estado ---
    private bool isDead = false;
    public bool isInvincible = false;
    public Renderer playerRenderer;
    private Color originalColor;
    public GameObject playerWeapon;
    private bool wasTopDownMode = false;

    private Vector2 mouseScreenPosition;
    private Vector3 lookTarget;

    public PowerUpUI powerUpUI;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        fpsCameraTransform = Camera.main?.transform;
        originalColor = playerRenderer.material.color;

        if (deathAudioSource == null) deathAudioSource = GetComponent<AudioSource>();
        if (camSwitcher == null) camSwitcher = FindAnyObjectByType<CamSwitch>();
        if (playerCameraMovement == null) playerCameraMovement = Camera.main?.GetComponent<CameraMovement>();
        if (healthBarUI == null) healthBarUI = FindAnyObjectByType<HealthBarUI>();
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

        isWallRunning = !isTopDownMode && (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;
        if (isWallRunning)
        {
            Vector3 wallRunDir = Vector3.Cross(wallNormal, Vector3.up);
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDir, orientation.forward) < 0) wallRunDir = -wallRunDir;
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
        if (ctx.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (ammo <= 0) return;

        muzzleFlash?.Play();
        gunShotAudio?.Play();

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
                TakeDamage(25f);
            }
            Destroy(collision.gameObject);
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

    private IEnumerator LoadDefeatSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (deathAudioSource != null && deathAudioSource.isPlaying) yield return null;
        LoadDefeatScene();
    }

    private void LoadDefeatScene()
    {
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
        playerRenderer.material.color = Color.blue;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        playerRenderer.material.color = originalColor;
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
