using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/PowerUpData")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public GameObject prefab;
    [Range(0f, 1f)] public float dropChance;
}

