using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Sfx hoverSfx = Sfx.ButtonClick; // or add a new enum value like Sfx.ButtonHover

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySfx(hoverSfx);
    }
}