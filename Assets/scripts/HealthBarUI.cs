using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;

    private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;

            UpdateHealthBar(player.currentHealth);
        }
    }

    private void UpdateHealthBar(float health)
    {
        slider.value = health;
    }

    private void OnDestroy()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
