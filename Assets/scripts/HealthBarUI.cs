// HealthBarUI.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para Image

public class HealthBarUI : MonoBehaviour
{
    public Image healthFillImage; // Arrastra el componente Image que representa el "relleno" de la barra aqu�.
                                  // Aseg�rate de que el tipo de Image en el Inspector sea "Filled" (Relleno).

    private float maxHealthValue; // Almacenar� el valor m�ximo de salud para calcular el porcentaje.

    void Awake()
    {
        // Opcional: Intenta encontrar el Image si no se asigna en el Inspector
        if (healthFillImage == null)
        {
            healthFillImage = GetComponent<Image>();
            if (healthFillImage == null)
            {
                Debug.LogError("HealthBarUI: No se encontr� un componente Image en este GameObject. Asigna uno en el Inspector.");
            }
        }
    }

    // Inicializa la barra de vida con el valor m�ximo
    public void SetMaxHealth(float maxHealth)
    {
        maxHealthValue = maxHealth; // Almacena el valor m�ximo
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f; // Rellena la imagen al m�ximo al inicio (100%)
        }
    }

    // Actualiza la barra de vida al valor actual
    public void SetHealth(float currentHealth)
    {
        if (healthFillImage != null && maxHealthValue > 0) // Aseg�rate de que maxHealthValue no sea cero para evitar divisi�n por cero
        {
            // Calcula el porcentaje de salud (0 a 1)
            healthFillImage.fillAmount = currentHealth / maxHealthValue;
        }
        else if (maxHealthValue <= 0)
        {
            Debug.LogWarning("HealthBarUI: maxHealthValue es cero o negativo. No se puede calcular el fillAmount de la barra de vida.");
        }
    }
}
