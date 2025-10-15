using UnityEngine;

public class CauldronDropZone : MonoBehaviour
{
    [Header("Managers")]
    public ScalePourManager scalePourManager;

    private bool _armed;

    public bool isArmed() => _armed;

    public void Arm(IngredientDraggable owner = null)
    {
        if (_armed) return;
        _armed = true;
        if (scalePourManager != null) scalePourManager.BeginForIngredient(owner);
        Debug.Log("[DropZone] ARMED → shaker ready");
    }

    public void Disarm()
    {
        if (!_armed) return;
        _armed = false;
        Debug.Log("[DropZone] DISARMED");
    }
}
