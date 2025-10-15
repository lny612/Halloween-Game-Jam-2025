using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ingredient Data")]
    public string ingredientName = "Iris Sugar";
    public float targetAmount = 100f;
    public string unit = "g";
    public float tolerance = 5f;
    public float pourRatePerSecond = 150f;
    public float timeLimit = 5f;

    [Header("Drag Visuals")]
    public Canvas canvas;              // assign your UI Canvas
    public Image ghostImagePrefab;     // optional: a drag ghost (duplicate sprite)

    private RectTransform _rt;
    private CanvasGroup _group;
    private Image _ghostInstance;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _group.blocksRaycasts = false;        // allow drop zones to receive
        if (ghostImagePrefab != null && canvas != null)
        {
            _ghostInstance = Instantiate(ghostImagePrefab, canvas.transform);
            _ghostInstance.sprite = GetComponent<Image>()?.sprite;
            _ghostInstance.raycastTarget = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghostInstance != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out var localPos);
            _ghostInstance.rectTransform.anchoredPosition = localPos;
        }
        else
        {
            // fallback: move self (if you prefer dragging the actual object)
            _rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _group.blocksRaycasts = true;
        if (_ghostInstance != null) Destroy(_ghostInstance.gameObject);
    }
}
