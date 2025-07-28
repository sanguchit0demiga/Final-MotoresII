using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para Coroutines

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
    public LayerMask groundLayer; // Capa para Raycast en modo Top-Down
    public CamSwitch camSwitcher; // Para alternar cámaras
    public CameraMovement playerCameraMovement; // Para movimiento de cámara FPS
    private Transform fpsCameraTransform; // Referencia a la Transform de la cámara principal (FPS)

    // --- Salud ---
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBarUI healthBarUI; // Referencia al script de la barra de vida
    private Vector3 posicionInicial; // Para guardar la posición de inicio del jugador

    // --- Arma y Disparo ---
    public GameObject bulletPrefab;
    public Transform firePoint; // Punto desde donde se disparan las balas
    public float bulletForce = 20f; // Fuerza de la bala
    public float fireRate = 0.2f; // Tiempo entre disparos
    private float nextFireTime = 0f; // Controla el cooldown de disparo
    public ParticleSystem muzzleFlash; // Efecto de flash al disparar
    public float normalBulletDamage = 10f;
    public float boostedBulletDamage = 20f;
    private bool damageBoosted = false;

    // --- Audio ---
    public AudioSource gunShotAudio; // Fuente de audio para disparos normales
    public AudioSource reloadAudio;
    public AudioSource footstepAudio;
    public AudioSource jumpAudio;
    public AudioSource deathAudioSource; // Fuente de audio para la muerte
    public AudioClip deathSoundClip; // Clip de sonido de muerte
    public float delayBeforeDefeatScene = 0.5f; // Retraso antes de cargar la escena de derrota

    // --- NUEVA VARIABLE DE AUDIO ÚNICA PARA POCA MUNICIÓN ---
    [Header("Low Ammo Sound")]
    [Tooltip("Sonido de disparo cuando quedan 1 o 2 balas.")]
    public AudioClip lowAmmoSound;
    // --- FIN NUEVA VARIABLE DE AUDIO ---

    // --- UI ---
    public int ammo = 25; // Munición actual
    public int maxAmmo = 25; // Munición máxima
    public Text ammoDisplay; // Elemento UI para mostrar la munición
    public string gameOverSceneName = "Defeat"; // Nombre de la escena de derrota

    // --- Estado del Jugador ---
    private bool isDead = false; // Bandera para saber si el jugador está muerto
    public bool isInvincible = false; // Bandera para invencibilidad (Power-Up)
    public Renderer playerRenderer; // Para cambiar el color del jugador con Power-Up
    private Color originalColor; // Guarda el color original del jugador
    public GameObject playerWeapon; // Referencia al arma del jugador
    private bool wasTopDownMode = false; // Para detectar cambios de modo de cámara

    // --- Input de Ratón para Modo Top-Down ---
    private Vector2 mouseScreenPosition;
    private Vector3 lookTarget; // Punto al que el jugador debe mirar en modo Top-Down

    public PowerUpUI powerUpUI; // Referencia al script de UI de Power-Ups

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Intenta obtener la cámara principal y su transform
        if (Camera.main != null)
        {
            fpsCameraTransform = Camera.main.transform;
            playerCameraMovement = Camera.main.GetComponent<CameraMovement>();
        }
        else
        {
            Debug.LogWarning("[PlayerController] No se encontró la cámara principal ('MainCamera' tag). Algunas funcionalidades de cámara y arma pueden no funcionar.");
        }

        // Si playerRenderer está asignado, guarda su color original
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("[PlayerController] Player Renderer no asignado. No se podrá cambiar el color con la invencibilidad.");
        }

        // Asignación de fuentes de audio si no están en el Inspector
        if (deathAudioSource == null) deathAudioSource = GetComponent<AudioSource>();
        if (camSwitcher == null) camSwitcher = FindAnyObjectByType<CamSwitch>();
        if (healthBarUI == null) healthBarUI = FindAnyObjectByType<HealthBarUI>();
        if (powerUpUI == null) powerUpUI = FindAnyObjectByType<PowerUpUI>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla
        Cursor.visible = false; // Oculta el cursor
        currentHealth = maxHealth; // Inicializa la salud al máximo
        posicionInicial = transform.position; // Guarda la posición inicial
        UpdateAmmoDisplay(); // Actualiza el display de munición
        healthBarUI?.SetMaxHealth(maxHealth); // Configura la barra de vida
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida

        // Ajusta el parent del arma según el modo de cámara inicial
        if (playerWeapon != null && fpsCameraTransform != null)
        {
            playerWeapon.transform.SetParent(fpsCameraTransform);
        }

        // Guarda el estado inicial del modo Top-Down
        wasTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();
    }

    void Update()
    {
        CheckForWalls(); // Revisa si hay paredes cerca para WallRun
        if (isDead) return; // Si el jugador está muerto, no ejecuta más lógica

        HandleCameraSwitch(); // Maneja el cambio de padre del arma al cambiar de cámara
        HandleMovement(); // Maneja el movimiento del jugador

        // Recarga al presionar 'R'
        if (Keyboard.current.rKey.wasPressedThisFrame) Reload();
    }

    private void HandleCameraSwitch()
    {
        // Verifica si el modo Top-Down está activo
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (playerWeapon != null && camSwitcher != null)
        {
            // Si acabamos de entrar en modo Top-Down
            if (isTopDownMode && !wasTopDownMode)
            {
                playerWeapon.transform.SetParent(transform); // Arma sigue al jugador
                // Opcional: ajustar posición/rotación del arma para el modo Top-Down
            }
            // Si acabamos de salir del modo Top-Down
            else if (!isTopDownMode && wasTopDownMode)
            {
                playerWeapon.transform.SetParent(fpsCameraTransform); // Arma sigue a la cámara FPS
                // Opcional: ajustar posición/rotación del arma para el modo FPS
            }
        }
        wasTopDownMode = isTopDownMode; // Actualiza el estado del modo Top-Down
    }

    private void HandleMovement()
    {
        Vector3 move;
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (!isTopDownMode)
        {
            // Movimiento basado en la orientación de la cámara (FPS)
            move = orientation.forward * input.y + orientation.right * input.x;
        }
        else
        {
            // Movimiento en el plano XY para vista Top-Down
            move = new Vector3(input.x, 0f, input.y).normalized;
            // Asegura que el target de mirada esté en la misma altura que el jugador
            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget); // Gira al jugador para que mire al ratón
        }
        move *= moveSpeed; // Aplica la velocidad de movimiento

        // Lógica de gravedad y estado del suelo
        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Pequeña fuerza hacia abajo para asegurar que el CharacterController esté en el suelo
            wallJumpHorizontalVelocity = Vector3.zero; // Reinicia la velocidad horizontal de salto de pared
            if (isJumping) // Si se presionó el botón de salto estando en el suelo
            {
                verticalVelocity = jumpForce; // Aplica la fuerza de salto
                isJumping = false; // Resetea la bandera de salto
            }
        }
        else
        {
            // Aplica gravedad normal o gravedad reducida si está en WallRun
            verticalVelocity -= (isWallRunning ? wallRunGravity : gravity) * Time.deltaTime;
        }

        // Lógica de WallRun
        // Se activa si no está en modo Top-Down, hay pared a izquierda o derecha, no está en el suelo y se está moviendo hacia adelante
        isWallRunning = !isTopDownMode && (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;
        if (isWallRunning)
        {
            Vector3 wallRunDir = Vector3.Cross(wallNormal, Vector3.up); // Calcula la dirección a lo largo de la pared
            // Asegura que la dirección sea hacia adelante del jugador
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDir, orientation.forward) < 0) wallRunDir = -wallRunDir;

            Vector3 finalMove = (wallRunDir * wallRunSpeed + move * 0.5f); // Combina WallRun y un poco de input de movimiento
            finalMove.y = verticalVelocity; // Mantiene la velocidad vertical
            controller.Move(finalMove * Time.deltaTime); // Mueve al jugador en WallRun
        }
        else
        {
            move.y = verticalVelocity; // Aplica la velocidad vertical (gravedad/salto)
            // Aplica velocidad horizontal del salto de pared y la reduce gradualmente
            if (wallJumpHorizontalVelocity.magnitude > 0.1f)
            {
                move += wallJumpHorizontalVelocity;
                wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, 10f * Time.deltaTime);
            }
            controller.Move(move * Time.deltaTime); // Mueve al jugador normalmente
        }
    }

    void CheckForWalls()
    {
        // Desactiva la detección de paredes en modo Top-Down
        if (camSwitcher != null && camSwitcher.IsTopDownActive()) { wallLeft = wallRight = false; return; }

        RaycastHit hit;
        // Raycast hacia la derecha para detectar pared
        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight) wallNormal = hit.normal; // Guarda la normal de la pared
        // Raycast hacia la izquierda para detectar pared
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallLeft) wallNormal = hit.normal; // Guarda la normal de la pared
    }

    // --- Métodos de Input System ---
    public void OnMove(InputAction.CallbackContext ctx) => input = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return; // Solo actua al inicio de la pulsación
        // Permite saltar desde el suelo o al hacer WallRun (si no está en modo Top-Down)
        if ((camSwitcher == null || !camSwitcher.IsTopDownActive()) && (controller.isGrounded || isWallRunning))
        {
            verticalVelocity = jumpForce; // Aplica la fuerza de salto
            isJumping = controller.isGrounded; // Setea isJumping solo si salta desde el suelo (para resetear verticalVelocity)
            if (isWallRunning) wallJumpHorizontalVelocity = (wallNormal + Vector3.up).normalized * wallJumpForce; // Salto de pared
            jumpAudio?.Play(); // Reproduce sonido de salto
        }
    }

    public void OnMouseLook(InputAction.CallbackContext ctx)
    {
        if (camSwitcher != null && camSwitcher.IsTopDownActive())
        {
            // En modo Top-Down, el ratón define el punto al que el jugador mira
            mouseScreenPosition = ctx.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
            // Raycast al suelo para encontrar el punto de mirada
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                lookTarget = hit.point;
        }
        else if (playerCameraMovement != null && ctx.phase == InputActionPhase.Performed)
        {
            // En modo FPS, el ratón controla el movimiento de la cámara
            playerCameraMovement.ReceiveMouseInput(ctx.ReadValue<Vector2>());
        }
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        // Dispara si el botón se presiona y el cooldown ha terminado
        if (ctx.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate; // Resetea el cooldown
        }
    }

    private void Shoot()
    {
        if (ammo <= 0)
        {
            Debug.LogWarning("[PlayerController] No hay munición. No se puede disparar.");
            return; // No dispares si no hay munición
        }

        muzzleFlash?.Play(); // Reproduce el flash del cañón

        // --- LÓGICA DE SONIDO DE DISPARO CONDICIONAL ---
        // Se ejecuta ANTES de que la munición se reduzca
        // Si quedan 1 o 2 balas, reproduce el sonido de poca munición
        if ((ammo == 2 || ammo == 1) && lowAmmoSound != null)
        {
            gunShotAudio?.PlayOneShot(lowAmmoSound); // Reproduce el sonido de poca munición
            Debug.Log($"[PlayerController] Reproduciendo sonido de poca munición ({ammo} balas restantes).");
        }
        else // Para el resto de los disparos (más de 2 balas)
        {
            gunShotAudio?.Play(); // Reproduce el sonido de disparo normal
            Debug.Log("[PlayerController] Reproduciendo sonido de disparo normal.");
        }
        // --- FIN LÓGICA DE SONIDO ---

        // Determina la dirección del disparo según el modo de cámara
        Vector3 dir = (camSwitcher != null && camSwitcher.IsTopDownActive()) ? transform.forward : Camera.main.transform.forward;
        // Instancia la bala
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Pasa el daño a la bala (normal o boosteado)
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            float damage = damageBoosted ? boostedBulletDamage : normalBulletDamage;
            bulletScript.SetDamage(damage);
        }

        // Aplica fuerza a la bala
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb?.AddForce(dir * bulletForce, ForceMode.Impulse);

        Destroy(bullet, 4f); // Destruye la bala después de un tiempo
        ammo--; // Reduce la munición
        UpdateAmmoDisplay(); // Actualiza el display de munición en UI
    }

    private void Reload()
    {
        reloadAudio?.Play(); // Reproduce el sonido de recarga
        ammo = maxAmmo; // Rellena la munición
        UpdateAmmoDisplay(); // Actualiza el display de munición
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString(); // Muestra la munición actual
        }
    }

    // --- Gestión de Daño y Muerte ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy")) // Si choca con una bala enemiga
        {
            if (!isInvincible) // Si no es invencible
            {
                TakeDamage(15f); // Recibe daño (ej. 15 de daño por bala enemiga)
            }
            Destroy(collision.gameObject); // Destruye la bala enemiga
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; // Reduce la salud
        currentHealth = Mathf.Max(currentHealth, 0); // Asegura que la salud no baje de 0
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida
        if (currentHealth <= 0) Die(); // Si la salud llega a 0, el jugador muere
    }

    public void Die()
    {
        if (isDead) return; // Evita morir múltiples veces

        isDead = true; // Marca al jugador como muerto
        controller.enabled = false; // Deshabilita el CharacterController para detener el movimiento
        MusicManagerScript.instance?.StopMusic(); // Detiene la música (si tienes un MusicManager)

        // Reproduce sonido de muerte y carga la escena de derrota después de un retraso
        if (deathAudioSource != null && deathSoundClip != null)
        {
            deathAudioSource.PlayOneShot(deathSoundClip);
            StartCoroutine(LoadDefeatSceneAfterDelay(delayBeforeDefeatScene));
        }
        else
        {
            LoadDefeatScene(); // Carga la escena de derrota directamente si no hay audio
        }
    }

    private IEnumerator LoadDefeatSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Espera el retraso
        // Espera a que termine el sonido de muerte si aún está sonando
        while (deathAudioSource != null && deathAudioSource.isPlaying) yield return null;
        LoadDefeatScene(); // Carga la escena de derrota
    }

    private void LoadDefeatScene()
    {
        camSwitcher?.ResetToFPS(); // Asegura que la cámara vuelva a FPS si estaba en Top-Down
        playerCameraMovement?.RecenterCamera(); // Restaura la orientación de la cámara
        SceneManager.LoadScene("Defeat"); // Carga la escena de derrota
    }

    // --- Métodos de Power-Up ---
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Cura al jugador, sin exceder la salud máxima
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida
    }

    public void ActivateInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration)); // Inicia la coroutine de invencibilidad

        // Muestra la UI del Power-Up de invencibilidad
        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.invincibilityOverlay, powerUpUI.invincibilityUI, duration);
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true; // Activa la bandera de invencibilidad
        if (playerRenderer != null) playerRenderer.material.color = Color.blue; // Cambia el color a azul
        yield return new WaitForSeconds(duration); // Espera la duración
        isInvincible = false; // Desactiva la invencibilidad
        if (playerRenderer != null) playerRenderer.material.color = originalColor; // Restaura el color original
    }

    public void ActivateDamageBoost(float duration)
    {
        StartCoroutine(DamageBoostCoroutine(duration)); // Inicia la coroutine de boost de daño

        // Muestra la UI del Power-Up de daño
        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.damageOverlay, powerUpUI.damageUI, duration);
    }

    private IEnumerator DamageBoostCoroutine(float duration)
    {
        damageBoosted = true; // Activa el boost de daño
        yield return new WaitForSeconds(duration); // Espera la duración
        damageBoosted = false; // Desactiva el boost de daño
    }
}