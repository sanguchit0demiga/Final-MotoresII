
using UnityEngine;
using UnityEngine.UI; 

public class HealthBarUI : MonoBehaviour
{
    public Image healthFillImage; 

    private float maxHealthValue;

    void Awake()
    {
        if (healthFillImage == null)
        {
            healthFillImage = GetComponent<Image>();
            if (healthFillImage == null)
            {
                Debug.LogError("HealthBarUI: No se encontró un componente Image en este GameObject. Asigna uno en el Inspector.");
            }
        }
    }
    public void SetMaxHealth(float maxHealth)
    {
        maxHealthValue = maxHealth; 
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f; 
        }
    }


    public void SetHealth(float currentHealth)
    {
        if (healthFillImage != null && maxHealthValue > 0) 
        {

            healthFillImage.fillAmount = currentHealth / maxHealthValue;
        }
        else if (maxHealthValue <= 0)
        {
            Debug.LogWarning("HealthBarUI: maxHealthValue es cero o negativo. No se puede calcular el fillAmount de la barra de vida.");
        }
    }
}
