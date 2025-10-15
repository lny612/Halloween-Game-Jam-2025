using UnityEngine;

public class CauldronDropZone : MonoBehaviour
{
    [Header("Managers")]
    public ScalePourManager scalePourManager;

    [Tooltip("If true, only the first contact starts the step; ignores further contacts until complete.")]
    public bool singleUse = true;

    private bool _armed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (singleUse && _armed) return;

        var draggable = other.GetComponent<IngredientDraggable>();
        if (draggable == null) return;

        scalePourManager.BeginForIngredient(draggable);

        _armed = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _armed = false;
    }

    public bool isArmed()
    {
        return _armed;
    }   
}
