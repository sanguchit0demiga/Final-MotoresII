using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System
using UnityEngine.UI; // Necesario para componentes UI como Text
using UnityEngine.SceneManagement; // Necesario para cargar escenas
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
    public Transform orientation; // Objeto que define la dirección hacia adelante del jugador (usualmente la cámara)
    public LayerMask wallLayer; // Capa para detectar paredes en WallRun
    public LayerMask groundLayer; // Capa para el raycast en modo Top-Down para detectar el suelo
    public CamSwitch camSwitcher; // Referencia al script que controla el cambio de cámaras
    public CameraMovement playerCameraMovement; // Referencia al script de movimiento de cámara FPS
    private Transform fpsCameraTransform; // Referencia a la Transform de la cámara principal (FPS)

    // --- Salud ---
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBarUI healthBarUI; // Referencia al script de la barra de vida UI
    private Vector3 posicionInicial; // Para guardar la posición de inicio del jugador (aunque no se usa actualmente para respawn)

    // --- Arma y Disparo ---
    public GameObject bulletPrefab; // Prefab de la bala
    public Transform firePoint; // Punto desde donde se disparan las balas
    public float bulletForce = 20f; // Fuerza con la que se dispara la bala
    public float fireRate = 0.2f; // Tiempo de espera entre disparos
    private float nextFireTime = 0f; // Variable para controlar el cooldown de disparo
    public ParticleSystem muzzleFlash; // Efecto de partículas del fogonazo del arma
    public float normalBulletDamage = 10f; // Daño normal de la bala
    public float boostedBulletDamage = 20f; // Daño de la bala con Power-Up
    private bool damageBoosted = false; // Bandera para el Power-Up de daño

    // --- Audio ---
    public AudioSource gunShotAudio; // Fuente de audio para disparos normales (y baja munición)
    public AudioSource reloadAudio; // Fuente de audio para la recarga
    public AudioSource footstepAudio; // Fuente de audio para pasos (no implementado en este script, pero declarado)
    public AudioSource jumpAudio; // Fuente de audio para el salto
    public AudioSource deathAudioSource; // Fuente de audio para el sonido de muerte
    public AudioClip deathSoundClip; // Clip de sonido específico para la muerte
    public float delayBeforeDefeatScene = 0.5f; // Retraso antes de cargar la escena de derrota después de morir

    // --- SONIDO DE POCA MUNICIÓN ---
    [Header("Low Ammo Sound")]
    [Tooltip("Sonido de disparo cuando quedan 1 o 2 balas.")]
    public AudioClip lowAmmoSound;
    // --- FIN SONIDO DE POCA MUNICIÓN ---

    // --- UI ---
    public int ammo = 25; // Munición actual
    public int maxAmmo = 25; // Munición máxima
    public Text ammoDisplay; // Componente de texto UI para mostrar la munición
    public string gameOverSceneName = "Defeat"; // Nombre de la escena a cargar al morir

    // --- Estado del Jugador ---
    private bool isDead = false; // Bandera para controlar si el jugador ha muerto
    public bool isInvincible = false; // Bandera para el Power-Up de invencibilidad
    public Renderer playerRenderer; // Componente Renderer del jugador para cambiar su color (invencibilidad)
    private Color originalColor; // Guarda el color original del renderer
    public GameObject playerWeapon; // Referencia al objeto del arma del jugador (para cambiar su padre)
    private bool wasTopDownMode = false; // Bandera para detectar cambios de modo de cámara

    // --- Input de Ratón para Modo Top-Down (para apuntar) ---
    private Vector2 mouseScreenPosition;
    private Vector3 lookTarget; // El punto en el mundo al que el jugador debe mirar en modo Top-Down

    public PowerUpUI powerUpUI; // Referencia al script de la UI de Power-Ups

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Intenta obtener la cámara principal y sus componentes
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

        // Asignación de fuentes de audio si no están asignadas en el Inspector (fallback)
        if (deathAudioSource == null) deathAudioSource = GetComponent<AudioSource>();
        if (camSwitcher == null) camSwitcher = FindAnyObjectByType<CamSwitch>();
        if (healthBarUI == null) healthBarUI = FindAnyObjectByType<HealthBarUI>();
        if (powerUpUI == null) powerUpUI = FindAnyObjectByType<PowerUpUI>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla
        Cursor.visible = false; // Oculta el cursor para una experiencia FPS inmersiva
        currentHealth = maxHealth; // Inicializa la salud al máximo
        posicionInicial = transform.position; // Guarda la posición inicial del jugador
        UpdateAmmoDisplay(); // Actualiza el display de munición al inicio del juego
        healthBarUI?.SetMaxHealth(maxHealth); // Configura la barra de vida UI
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida UI a la salud actual

        // Ajusta el parent del arma al transform de la cámara FPS si existe
        if (playerWeapon != null && fpsCameraTransform != null)
        {
            playerWeapon.transform.SetParent(fpsCameraTransform);
        }

        // Guarda el estado inicial del modo Top-Down para detectar cambios
        wasTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();
    }

    void Update()
    {
        CheckForWalls(); // Realiza Raycasts para detectar paredes para WallRun
        if (isDead) return; // Si el jugador está muerto, no se ejecuta ninguna lógica de juego

        HandleCameraSwitch(); // Maneja la lógica de cambio de padre del arma al cambiar de cámara
        HandleMovement(); // Maneja el movimiento del jugador basado en el input y la física

        // Detecta la pulsación de la tecla 'R' para recargar
        if (Keyboard.current.rKey.wasPressedThisFrame) Reload();
    }

    private void HandleCameraSwitch()
    {
        // Verifica el estado actual del modo Top-Down
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (playerWeapon != null && camSwitcher != null)
        {
            // Si acabamos de entrar en modo Top-Down (y antes no lo estábamos)
            if (isTopDownMode && !wasTopDownMode)
            {
                playerWeapon.transform.SetParent(transform); // El arma se convierte en hija del jugador
                // Opcional: Aquí podrías ajustar la posición local y rotación del arma para el modo Top-Down
            }
            // Si acabamos de salir del modo Top-Down (y antes sí lo estábamos)
            else if (!isTopDownMode && wasTopDownMode)
            {
                playerWeapon.transform.SetParent(fpsCameraTransform); // El arma se convierte en hija de la cámara FPS
                // Opcional: Aquí podrías ajustar la posición local y rotación del arma para el modo FPS
            }
        }
        wasTopDownMode = isTopDownMode; // Actualiza el estado previo del modo Top-Down
    }

    private void HandleMovement()
    {
        Vector3 move;
        bool isTopDownMode = camSwitcher != null && camSwitcher.IsTopDownActive();

        if (!isTopDownMode)
        {
            // Movimiento en 3D basado en la orientación de la cámara (para FPS)
            move = orientation.forward * input.y + orientation.right * input.x;
        }
        else
        {
            // Movimiento en el plano horizontal (XY) para vista Top-Down
            move = new Vector3(input.x, 0f, input.y).normalized;
            // Asegura que el target de mirada esté a la misma altura que el jugador
            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget); // Gira al jugador para que mire hacia el ratón
        }
        move *= moveSpeed; // Aplica la velocidad de movimiento base

        // Lógica de gravedad y detección de suelo
        if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Pequeña fuerza hacia abajo para mantener al CharacterController pegado al suelo
            wallJumpHorizontalVelocity = Vector3.zero; // Reinicia la velocidad horizontal de salto de pared
            if (isJumping) // Si el botón de salto fue presionado y el jugador está en el suelo
            {
                verticalVelocity = jumpForce; // Aplica la fuerza de salto
                isJumping = false; // Reinicia la bandera de salto
            }
        }
        else
        {
            // Aplica gravedad, reducida si está en WallRun
            verticalVelocity -= (isWallRunning ? wallRunGravity : gravity) * Time.deltaTime;
        }

        // Lógica de WallRun
        // Se activa si no está en modo Top-Down, hay pared detectada, no está en el suelo y hay input de movimiento hacia adelante
        isWallRunning = !isTopDownMode && (wallLeft || wallRight) && !controller.isGrounded && Mathf.Abs(input.y) > 0;
        if (isWallRunning)
        {
            // Calcula la dirección a lo largo de la pared usando el producto cruz
            Vector3 wallRunDir = Vector3.Cross(wallNormal, Vector3.up);
            // Asegura que la dirección del WallRun sea coherente con el input del jugador
            if (Mathf.Sign(input.y) * Vector3.Dot(wallRunDir, orientation.forward) < 0) wallRunDir = -wallRunDir;

            // Combina el movimiento de WallRun con una pequeña influencia del input normal
            Vector3 finalMove = (wallRunDir * wallRunSpeed + move * 0.5f);
            finalMove.y = verticalVelocity; // Mantiene la velocidad vertical
            controller.Move(finalMove * Time.deltaTime); // Mueve al jugador en modo WallRun
        }
        else
        {
            move.y = verticalVelocity; // Aplica la velocidad vertical (gravedad/salto)
            // Aplica la velocidad horizontal del salto de pared y la interpola a cero
            if (wallJumpHorizontalVelocity.magnitude > 0.1f)
            {
                move += wallJumpHorizontalVelocity;
                wallJumpHorizontalVelocity = Vector3.Lerp(wallJumpHorizontalVelocity, Vector3.zero, 10f * Time.deltaTime);
            }
            controller.Move(move * Time.deltaTime); // Mueve al jugador en modo normal
        }
    }

    void CheckForWalls()
    {
        // Desactiva la detección de paredes en modo Top-Down
        if (camSwitcher != null && camSwitcher.IsTopDownActive()) { wallLeft = wallRight = false; return; }

        RaycastHit hit;
        // Raycast hacia la derecha del jugador para detectar pared
        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight) wallNormal = hit.normal; // Guarda la normal de la pared si se detecta
        // Raycast hacia la izquierda del jugador para detectar pared
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallLeft) wallNormal = hit.normal; // Guarda la normal de la pared si se detecta
    }

    // --- Métodos de Input System (llamados por los eventos de Input Actions) ---
    public void OnMove(InputAction.CallbackContext ctx) => input = ctx.ReadValue<Vector2>(); // Lee el input de movimiento

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return; // Solo actúa al inicio de la pulsación del botón de salto
        // Permite saltar si el jugador está en el suelo o en WallRun (y no está en modo Top-Down)
        if ((camSwitcher == null || !camSwitcher.IsTopDownActive()) && (controller.isGrounded || isWallRunning))
        {
            verticalVelocity = jumpForce; // Establece la velocidad vertical para el salto
            isJumping = controller.isGrounded; // Marca isJumping solo si salta desde el suelo (para resetear verticalVelocity)
            if (isWallRunning) wallJumpHorizontalVelocity = (wallNormal + Vector3.up).normalized * wallJumpForce; // Calcula el impulso del salto de pared
            jumpAudio?.Play(); // Reproduce el sonido de salto
        }
    }

    public void OnMouseLook(InputAction.CallbackContext ctx)
    {
        if (camSwitcher != null && camSwitcher.IsTopDownActive())
        {
            // En modo Top-Down, el input del ratón se usa para definir el punto de mira del jugador
            mouseScreenPosition = ctx.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition); // Crea un rayo desde la posición del ratón en la pantalla
            // Si el rayo impacta con el suelo, establece ese punto como el objetivo de mirada
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                lookTarget = hit.point;
        }
        else if (playerCameraMovement != null && ctx.phase == InputActionPhase.Performed)
        {
            // En modo FPS, el input del ratón se envía al script de movimiento de cámara
            playerCameraMovement.ReceiveMouseInput(ctx.ReadValue<Vector2>());
        }
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        // Dispara si el botón se presiona completamente y el cooldown de disparo ha terminado
        if (ctx.performed && Time.time >= nextFireTime)
        {
            Shoot(); // Llama al método de disparo
            nextFireTime = Time.time + fireRate; // Reinicia el temporizador de cooldown
        }
    }

    private void Shoot()
    {
        if (ammo <= 0)
        {
            Debug.LogWarning("[PlayerController] No hay munición. No se puede disparar.");
            return; // Sale del método si no hay munición
        }

        muzzleFlash?.Play(); // Reproduce el efecto de partículas del fogonazo

        // --- LÓGICA DE SONIDO DE DISPARO CONDICIONAL ---
        // Se ejecuta ANTES de que la munición se reduzca para que la condición sea precisa.
        // Si quedan 1 o 2 balas, reproduce el sonido de poca munición
        if ((ammo == 2 || ammo == 1) && lowAmmoSound != null)
        {
            gunShotAudio?.PlayOneShot(lowAmmoSound); // Usa PlayOneShot para que no se superpongan si se llama rápido
            Debug.Log($"[PlayerController] Reproduciendo sonido de poca munición ({ammo} balas restantes).");
        }
        else // Para el resto de los disparos (más de 2 balas)
        {
            gunShotAudio?.Play(); // Reproduce el sonido de disparo normal configurado en la fuente de audio
            Debug.Log("[PlayerController] Reproduciendo sonido de disparo normal.");
        }
        // --- FIN LÓGICA DE SONIDO ---

        // Determina la dirección del disparo: hacia adelante del jugador en Top-Down, o hacia adelante de la cámara en FPS
        Vector3 dir = (camSwitcher != null && camSwitcher.IsTopDownActive()) ? transform.forward : Camera.main.transform.forward;
        // Instancia el prefab de la bala en la posición del firePoint y con la rotación adecuada
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Obtiene el script Bullet de la bala y establece su daño
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            float damage = damageBoosted ? boostedBulletDamage : normalBulletDamage;
            bulletScript.SetDamage(damage);
        }

        // Obtiene el Rigidbody de la bala y aplica una fuerza para lanzarla
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb?.AddForce(dir * bulletForce, ForceMode.Impulse);

        Destroy(bullet, 4f); // Destruye la bala después de 4 segundos para limpiar la escena
        ammo--; // Reduce la munición en 1
        UpdateAmmoDisplay(); // Actualiza el texto de la munición en la UI
    }

    private void Reload()
    {
        reloadAudio?.Play(); // Reproduce el sonido de recarga
        ammo = maxAmmo; // Restaura la munición al máximo
        UpdateAmmoDisplay(); // Actualiza el texto de la munición en la UI
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = ammo.ToString(); // Asigna el valor de munición al texto de la UI
        }
    }

    // --- Gestión de Daño y Muerte del Jugador ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BulletEnemy")) // Si el jugador colisiona con un objeto con la etiqueta "BulletEnemy"
        {
            if (!isInvincible) // Si el jugador no está invencible
            {
                TakeDamage(15f); // Llama al método TakeDamage para reducir la salud (ej. 15 de daño)
            }
            Destroy(collision.gameObject); // Destruye la bala enemiga después de colisionar
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; // Reduce la salud actual
        currentHealth = Mathf.Max(currentHealth, 0); // Asegura que la salud no baje de cero
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida UI
        if (currentHealth <= 0) Die(); // Si la salud llega a cero o menos, el jugador muere
    }

    public void Die()
    {
        if (isDead) return; // Evita que la lógica de muerte se ejecute varias veces

        isDead = true; // Marca al jugador como muerto
        controller.enabled = false; // Deshabilita el CharacterController para detener todo el movimiento del jugador
        MusicManagerScript.instance?.StopMusic(); // Detiene la música del juego (asumiendo que tienes una instancia de MusicManagerScript)

        // Pausa el juego inmediatamente
        Time.timeScale = 0f;
        Debug.Log("[PlayerController] Juego pausado al morir.");

        if (deathAudioSource != null && deathSoundClip != null)
        {
            // Reproduce el sonido de muerte
            deathAudioSource.PlayOneShot(deathSoundClip);
            // Inicia la corrutina para esperar el sonido y luego despausar y cargar la escena
            StartCoroutine(UnpauseAndLoadDefeatSceneAfterDelay(delayBeforeDefeatScene));
        }
        else
        {
            // Si no hay sonido de muerte, despausa y carga la escena directamente
            LoadDefeatScene();
        }
    }

    // --- Corrutina para manejar la pausa y la carga de la escena de derrota ---
    private IEnumerator UnpauseAndLoadDefeatSceneAfterDelay(float delay)
    {
        // Espera el retraso inicial usando tiempo real (ignora Time.timeScale = 0f)
        yield return new WaitForSecondsRealtime(delay);

        // Espera a que el sonido de muerte termine de reproducirse (si aún está sonando)
        while (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            yield return null; // Espera un frame real
        }

        // Despausa el juego antes de cargar la nueva escena
        Time.timeScale = 1f;
        Debug.Log("[PlayerController] Juego despausado para cargar escena de derrota.");

        LoadDefeatScene(); // Llama al método para cargar la escena de derrota
    }

    private void LoadDefeatScene()
    {
        // Es una buena práctica asegurarse de que Time.timeScale esté en 1f antes de cargar una nueva escena
        // en caso de que la corrutina no se haya completado por alguna razón inesperada.
        Time.timeScale = 1f;
        camSwitcher?.ResetToFPS(); // Asegura que la cámara vuelva al modo FPS por defecto al morir
        playerCameraMovement?.RecenterCamera(); // Resetea la orientación de la cámara
        SceneManager.LoadScene("Defeat"); // Carga la escena de derrota
    }

    // --- Métodos de Power-Up ---
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Aumenta la salud sin exceder el máximo
        healthBarUI?.SetHealth(currentHealth); // Actualiza la barra de vida UI
    }

    public void ActivateInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration)); // Inicia la corrutina de invencibilidad

        // Muestra la UI del Power-Up de invencibilidad si powerUpUI está asignado
        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.invincibilityOverlay, powerUpUI.invincibilityUI, duration);
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true; // Activa la bandera de invencibilidad
        if (playerRenderer != null) playerRenderer.material.color = Color.blue; // Cambia el color del jugador a azul
        yield return new WaitForSeconds(duration); // Espera la duración del Power-Up (usando tiempo normal)
        isInvincible = false; // Desactiva la invencibilidad
        if (playerRenderer != null) playerRenderer.material.color = originalColor; // Restaura el color original del jugador
    }

    public void ActivateDamageBoost(float duration)
    {
        StartCoroutine(DamageBoostCoroutine(duration)); // Inicia la corrutina de boost de daño

        // Muestra la UI del Power-Up de daño si powerUpUI está asignado
        if (powerUpUI != null)
            powerUpUI.ShowPowerUp(powerUpUI.damageOverlay, powerUpUI.damageUI, duration);
    }

    private IEnumerator DamageBoostCoroutine(float duration)
    {
        damageBoosted = true; // Activa la bandera de daño boosteado
        yield return new WaitForSeconds(duration); // Espera la duración del Power-Up (usando tiempo normal)
        damageBoosted = false; // Desactiva el boost de daño
    }
}