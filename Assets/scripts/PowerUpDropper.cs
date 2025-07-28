using UnityEngine;

public class PowerUpDropper : MonoBehaviour
{
    public PowerUpDrop dropTable;

    public void TryDropPowerUp()
    {
        foreach (var powerUp in dropTable.possibleDrops)
        {
            float roll = Random.value;
            if (roll <= powerUp.dropChance)
            {
                Instantiate(powerUp.prefab, transform.position, Quaternion.identity);
                break; 
            }
        }
    }
}
