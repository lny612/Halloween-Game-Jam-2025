using UnityEngine;
using UnityEngine.EventSystems;

public class CauldronDropZone : MonoBehaviour, IDropHandler
{
    [Header("Managers")]
    public ScalePourManager scalePourManager;

    [Tooltip("If true, only the first drop starts the step; ignores further drops until complete.")]
    public bool singleUse = true;

    private bool _armed;

    public void OnDrop(PointerEventData eventData)
    {
        if (singleUse && _armed) return;

        var draggable = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<IngredientDraggable>() : null;
        if (draggable == null) return;

        // Arm the pour step with the data from the dragged ingredient
        scalePourManager.BeginForIngredient(
            draggable.ingredientName,
            draggable.targetAmount,
            draggable.unit,
            draggable.tolerance,
            draggable.pourRatePerSecond,
            draggable.timeLimit
        );

        _armed = true;
    }
}
