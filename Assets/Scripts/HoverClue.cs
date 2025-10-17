// HoverClue.cs
using UnityEngine;
using UnityEngine.EventSystems;

public enum ClueType { Desire, Insecurity }

public class HoverClue : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ClueType clueType;
    [SerializeField] private ChildUI childUI; // drag the same ChildUIUpdate here

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!childUI) return;
        if (clueType == ClueType.Desire) childUI.OnHoverDesireEnter();
        else childUI.OnHoverInsecurityEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!childUI) return;
        if (clueType == ClueType.Desire) childUI.OnHoverDesireExit();
        else childUI.OnHoverInsecurityExit();
    }
}
