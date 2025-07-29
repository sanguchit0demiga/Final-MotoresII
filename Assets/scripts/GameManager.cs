using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // �IMPORTANTE! Necesitas esto para cambiar de escena

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TextMeshProUGUI enemyCountText;
    public string winSceneName = "Win"; // Nombre de la escena de victoria (�aseg�rate de que coincida!)

    private int _enemiesRemaining;
    public int EnemiesRemaining
    {
        get { return _enemiesRemaining; }
        private set
        {
            _enemiesRemaining = value;
            Debug.Log($"[GameManager] Enemigos restantes: {_enemiesRemaining}");
            UpdateEnemyCountUI();

            if (_enemiesRemaining <= 0)
            {
                Debug.Log("[GameManager] �Todos los enemigos derrotados! Cambiando a la escena de victoria.");
                LoadWinScene(); // Llama al m�todo para cargar la escena de victoria
                // Si a�n quieres disparar el UnityEvent para otras cosas, puedes mantenerlo,
                // pero si solo era para la puerta, ya no es necesario para este prop�sito.
                // OnAllEnemiesDefeated?.Invoke();
            }
        }
    }

    // Ya no es estrictamente necesario si solo era para la puerta, pero puedes mantenerlo si lo usas para otras cosas.
    public UnityEvent OnAllEnemiesDefeated;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (OnAllEnemiesDefeated == null)
        {
            OnAllEnemiesDefeated = new UnityEvent();
        }

        int initialEnemiesInScene = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _enemiesRemaining = initialEnemiesInScene;
        UpdateEnemyCountUI();
        Debug.Log($"[GameManager] Enemigos iniciales (pre-colocados con etiqueta 'Enemy') encontrados: {initialEnemiesInScene}");
    }

    public void EnemyDied()
    {
        EnemiesRemaining--;
        Debug.Log("[GameManager] Un enemigo ha muerto, decrementando contador.");
    }

    public void AddEnemiesFromSpawner(int count)
    {
        EnemiesRemaining += count;
        Debug.Log($"[GameManager] Spawner ha notificado {count} enemigos. Total acumulado de enemigos a eliminar: {EnemiesRemaining}");
    }

    void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
        {
            int displayCount = Mathf.Max(0, _enemiesRemaining);
            enemyCountText.text = $"Enemigos restantes: {displayCount}";
        }
        else
        {
            Debug.LogWarning("[GameManager] TextMeshProUGUI 'enemyCountText' no est� asignado en el Inspector.");
        }
    }

    // Nuevo m�todo para cargar la escena de victoria
    private void LoadWinScene()
    {
        if (!string.IsNullOrEmpty("Win"))
        {
            // Antes de cargar la escena, puedes a�adir un peque�o retraso o una pantalla de carga si lo deseas.
            // Por ahora, cargamos directamente.
            SceneManager.LoadScene("Win");
        }
        else
        {
            Debug.LogError("[GameManager] El nombre de la escena de victoria (winSceneName) no est� asignado. No se puede cargar la escena.");
        }
    }

    public void ResetEnemyCount()
    {
        _enemiesRemaining = 0;
        UpdateEnemyCountUI();
    }
}