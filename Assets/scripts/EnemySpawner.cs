using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnRate = 5f;
    public int numberOfEnemiesToSpawn = 14; 

    private int enemiesSpawned = 0; 
    private float nextSpawnTime;
    private bool spawnerActive = false;  

    void Awake()
    {
        if (GameManager.Instance == null) 
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
            StopSpawner();
        }
    }

    public void StartSpawner()
    {
        enemiesSpawned = 0; 
        spawnerActive = true;
        nextSpawnTime = Time.time + spawnRate;
        Debug.Log("[EnemySpawner] Spawner activado y listo para generar " + numberOfEnemiesToSpawn + " enemigos.");

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
        newEnemy.tag = "Enemy";

        enemiesSpawned++;
        Debug.Log($"[EnemySpawner] Enemigo '{newEnemy.name}' spawneado en {spawnPoint.position}. Enemigos generados por este spawner: {enemiesSpawned}/{numberOfEnemiesToSpawn}.");
    }
}