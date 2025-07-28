using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnRate = 5f;
    public int numberOfEnemiesToSpawn = 14; // <--- ¡Asegúrate que este valor sea 14 en el Inspector!
    private int enemiesSpawned = 0; // Contador de enemigos generados por ESTE spawner

    private float nextSpawnTime;
    private bool spawnerActive = false; // Controla si el spawner está activo

    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[EnemySpawner] No se encontró un GameManager en la escena. El conteo de enemigos no funcionará.");
        }
    }

    void Update()
    {
        // PRIMERA CONDICIÓN: Si el spawner está activo Y NO ha generado todos los enemigos Y es tiempo de spawnear
        if (spawnerActive && enemiesSpawned < numberOfEnemiesToSpawn && Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
        // SEGUNDA CONDICIÓN: Si el spawner está activo Y YA ha generado todos los enemigos
        else if (spawnerActive && enemiesSpawned >= numberOfEnemiesToSpawn)
        {
            StopSpawner(); // Detiene el spawner para que no genere más
        }
    }

    public void StartSpawner()
    {
        // Reinicia el contador de enemigos generados por este spawner al inicio
        enemiesSpawned = 0;
        spawnerActive = true;
        nextSpawnTime = Time.time + spawnRate;
        Debug.Log("[EnemySpawner] Spawner activado y listo para generar " + numberOfEnemiesToSpawn + " enemigos.");

        // ¡IMPORTANTE! Notifica al GameManager el número TOTAL de enemigos que este spawner generará.
        if (gameManager != null)
        {
            gameManager.SetSpawnerEnemyCount(numberOfEnemiesToSpawn); // Le dice al GM que va a generar 14.
        }
    }

    public void StopSpawner()
    {
        spawnerActive = false; // Esto es CRÍTICO: Desactiva la bandera para que Update() no siga spawneando
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
        enemiesSpawned++; // Incrementa el contador de enemigos que ESTE spawner ha generado
        Debug.Log($"[EnemySpawner] Enemigo '{newEnemy.name}' spawneado en {spawnPoint.position}. Enemigos generados por este spawner: {enemiesSpawned}/{numberOfEnemiesToSpawn}.");

        // No necesitas llamar a RegisterSpawnedEnemy() aquí. El GameManager ya sumó los 14 cuando se llamó a SetSpawnerEnemyCount.
    }
}