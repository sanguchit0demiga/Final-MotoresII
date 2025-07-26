using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayHoverSound();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClickSound();
    }
}