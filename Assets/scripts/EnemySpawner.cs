using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnRate = 5f;
    public int numberOfEnemiesToSpawn = 14; // ¡Asegúrate que este valor sea 14 en el Inspector!

    private int enemiesSpawned = 0; // Contador de enemigos generados por ESTE spawner
    private float nextSpawnTime;
    private bool spawnerActive = false; // Controla si el spawner está activo

    // Ya no necesitas una referencia directa a GameManager aquí si usas el Singleton
    // private GameManager gameManager; 

    void Awake()
    {
        // Ya no necesitas FindAnyObjectByType aquí si usas el Singleton
        // gameManager = FindAnyObjectByType<GameManager>();
        if (GameManager.Instance == null) // Verifica si el Singleton está disponible
        {
            Debug.LogError("[EnemySpawner] No se encontró un GameManager. Asegúrate de que haya uno en la escena y se inicialice primero.");
        }
    }

    void Update()
    {
        if (spawnerActive && enemiesSpawned < numberOfEnemiesToSpawn && Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
        else if (spawnerActive && enemiesSpawned >= numberOfEnemiesToSpawn)
        {
            StopSpawner(); // Detiene el spawner una vez que ha generado todos los enemigos
        }
    }

    public void StartSpawner()
    {
        enemiesSpawned = 0; // Reinicia el contador de este spawner
        spawnerActive = true;
        nextSpawnTime = Time.time + spawnRate;
        Debug.Log("[EnemySpawner] Spawner activado y listo para generar " + numberOfEnemiesToSpawn + " enemigos.");

        // ¡IMPORTANTE! Notifica al GameManager el número de enemigos que va a generar.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemiesFromSpawner(numberOfEnemiesToSpawn);
        }
    }

    public void StopSpawner()
    {
        spawnerActive = false;
        Debug.Log("[EnemySpawner] Spawner detenido. Se generaron " + enemiesSpawned + " de " + numberOfEnemiesToSpawn + " enemigos.");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Configuración de spawner incompleta. Asegura que 'enemyPrefab' y 'spawnPoints' estén asignados.");
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        // Asegúrate de que el enemigo spawneado tenga la etiqueta "Enemy"
        newEnemy.tag = "Enemy";

        enemiesSpawned++;
        Debug.Log($"[EnemySpawner] Enemigo '{newEnemy.name}' spawneado en {spawnPoint.position}. Enemigos generados por este spawner: {enemiesSpawned}/{numberOfEnemiesToSpawn}.");
    }
}