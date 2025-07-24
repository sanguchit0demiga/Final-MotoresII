using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SFXManager.instance != null)
            SFXManager.instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SFXManager.instance != null)
            SFXManager.instance.PlayClick();
    }
}