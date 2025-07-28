using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;

    private float timer;
    private bool canSpawn = false;

    public int enemiesLeft;
    public TMPro.TextMeshProUGUI enemiesLeftText;

    void Update()
    {
        if (!canSpawn) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemies();
            timer = 0f;
        }
    }

    void SpawnEnemies()
    {
        Debug.Log("SpawnEnemies fue llamado!");
        if (enemyPrefabs.Count == 0 || spawnPoints.Length < 2) return;

        int index1 = Random.Range(0, spawnPoints.Length);
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        Transform spawnPoint1 = availablePoints[index1];

        availablePoints.RemoveAt(index1);
        int index2 = Random.Range(0, availablePoints.Count);
        Transform spawnPoint2 = availablePoints[index2];

        int enemy1 = Random.Range(0, enemyPrefabs.Count);
        int enemy2 = Random.Range(0, enemyPrefabs.Count);

        // --- Instanciación ---
        GameObject e1 = Instantiate(enemyPrefabs[enemy1], spawnPoint1.position, Quaternion.identity);
        GameObject e2 = Instantiate(enemyPrefabs[enemy2], spawnPoint2.position, Quaternion.identity);

        // --- Registrar enemigos en el contador ---
        RegisterEnemy();
        RegisterEnemy();
    }


    public void StartSpawner()
    {
        canSpawn = true;
    }

    public void StopSpawner()
    {
        canSpawn = false;
    }
    public void RegisterEnemy()
    {
        enemiesLeft++;
        UpdateUI();
    }

    public void UnregisterEnemy()
    {
        enemiesLeft--;
        UpdateUI();
        if (enemiesLeft <= 0)
        {
            StopSpawner();
            Debug.Log("EnemySpawner: Todos los enemigos eliminados.");
        }
    }

    private void UpdateUI()
    {
        if (enemiesLeftText != null)
            enemiesLeftText.text = "Enemies Left: " + enemiesLeft;
    }
}
