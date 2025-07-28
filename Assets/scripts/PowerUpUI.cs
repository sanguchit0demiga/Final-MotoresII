using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerUpUI : MonoBehaviour
{
    [Header("Damage UI")]
    public GameObject damageUI;
    public Image damageOverlay;

    [Header("Invincibility UI")]
    public GameObject invincibilityUI;
    public Image invincibilityOverlay;

    public void ShowPowerUp(Image overlayImage, GameObject uiObject, float duration)
    {
        uiObject.SetActive(true);
        overlayImage.fillAmount = 0f; // Comienza vacío
        StartCoroutine(FillRadial(overlayImage, duration, uiObject));
    }

    private IEnumerator FillRadial(Image overlay, float duration, GameObject uiObject)
    {
        float time = 0f;
        while (time < duration)
        {
            overlay.fillAmount = time / duration; // Va de 0 a 1
            time += Time.deltaTime;
            yield return null;
        }

        overlay.fillAmount = 1f;
        uiObject.SetActive(false); // O dejarlo visible, según prefieras
    }
}