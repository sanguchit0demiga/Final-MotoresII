using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDrop", menuName = "PowerUps/Drop")]
public class PowerUpDrop : ScriptableObject
{
    public PowerUpData[] possibleDrops;
}
