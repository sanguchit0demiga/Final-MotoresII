using UnityEngine;
using TMPro; // Necesitas esto si usas TextMeshPro
using System; // Necesario para Action en el evento Enemy.OnEnemyDied

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI enemyCountText; // �Arrastra tu objeto de texto aqu� en el Inspector!

    private int totalEnemies;
    private int enemiesKilled = 0;

    void Awake()
    {
        // 1. Contar los enemigos que YA est�n en la escena al inicio del juego.
        // Aseg�rate de que estos 6 enemigos tengan la etiqueta "Enemy".
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemiesKilled = 0; // Siempre inicia el conteo de muertos en 0.

        Debug.Log($"[GameManager] Enemigos iniciales (pre-colocados con etiqueta 'Enemy') encontrados: {totalEnemies}");
        // La UI se actualizar� con el valor correcto (6) inicialmente.
        UpdateEnemyCountUI();
    }

    void OnEnable()
    {
        // Suscr�bete al evento OnEnemyDied cuando este script se activa.
        Enemy.OnEnemyDied += HandleEnemyDied;
        Debug.Log("[GameManager] Suscrito al evento OnEnemyDied.");
    }

    void OnDisable()
    {
        // �Desuscr�bete del evento cuando este script se desactiva!
        Enemy.OnEnemyDied -= HandleEnemyDied;
        Debug.Log("[GameManager] Desuscrito del evento OnEnemyDied.");
    }

    private void HandleEnemyDied()
    {
        enemiesKilled++;
        Debug.Log($"[GameManager] Un enemigo fue eliminado. Total eliminados: {enemiesKilled}");
        UpdateEnemyCountUI();
    }

    void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
        {
            int remainingEnemies = totalEnemies - enemiesKilled;
            remainingEnemies = Mathf.Max(0, remainingEnemies); // Asegura que no sea negativo

            enemyCountText.text = $"Enemigos restantes: {remainingEnemies}";

            if (remainingEnemies <= 0 && totalEnemies > 0)
            {
                enemyCountText.text = "�Todos los enemigos eliminados!";
                Debug.Log("[GameManager] �Todos los enemigos han sido eliminados! Nivel completado.");
                // Aqu� podr�as a�adir l�gica para terminar el nivel, ir a la siguiente escena, etc.
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] TextMeshProUGUI 'enemyCountText' no est� asignado en el Inspector.");
        }
    }

    // M�todo llamado por el EnemySpawner para notificar cu�ntos enemigos va a generar.
    public void SetSpawnerEnemyCount(int count)
    {
        // Suma los enemigos que el spawner generar� a los que ya estaban en la escena.
        // Esto crea el "total final" que el jugador debe eliminar (6 fijos + 14 del spawner = 20).
        totalEnemies += count;
        // No reinicies enemiesKilled aqu� a menos que quieras que el conteo de muertos se resetee
        // cada vez que el spawner se activa (lo cual no es lo que quieres para un total de 20).
        Debug.Log($"[GameManager] Spawner ha notificado {count} enemigos. Total acumulado de enemigos a eliminar: {totalEnemies}");
        UpdateEnemyCountUI(); // Actualiza la UI con el nuevo total (ej. de 6 a 20)
    }

    // ... (otros m�todos como RegisterSpawnedEnemy o ResetEnemyCount si los usas) ...
    // Nota: El m�todo RegisterSpawnedEnemy() ya no es necesario si SetSpawnerEnemyCount()
    // est� siendo usado para establecer el total.
}