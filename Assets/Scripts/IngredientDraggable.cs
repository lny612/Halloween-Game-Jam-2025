using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Scripts")]
    public ScalePourManager scalePourManager;
    public CauldronDropZone cauldronDropZone;

    [Header("UI")]
    public Canvas canvas;
    public RectTransform bottleRT;
    public RectTransform cauldronZone;
    public ParticleSystem pourParticles;

    [Header("Shaking")]
    
    public float pixelsPerSecondForUnitSpeed = 500f;
    public float minShakeSpeed = 0.15f;
    public float maxShakeSpeed = 3f;
    [Range(0f, 1f)] public float speedLerp = 0.25f;

    [Header("Ingredient Information")]
    public IngredientSubtype ingredientSubtype;

    // runtime
    private RectTransform _rt;
    private CanvasGroup _group;
    private Camera _uiCam;
    private Vector2 _lastLocalPos;
    private float _lastTime;
    private float _filteredVSpeed;
    private Vector2 _startAnchoredPos;
    private Vector2 localNow;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!bottleRT) bottleRT = _rt;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        _startAnchoredPos = bottleRT.anchoredPosition;
        if (pourParticles) SetParticleEmission(0f);
    }

    public void SetShakerMode(bool enabled, RectTransform cauldron = null, object _ = null)
    {
        if (cauldron) cauldronZone = cauldron;
        _filteredVSpeed = 0f;
        if (pourParticles) SetParticleEmission(0f);
        var img = GetComponent<Image>();
        if (img) img.raycastTarget = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _uiCam = eventData.pressEventCamera;
        _group.blocksRaycasts = false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                bottleRT.parent as RectTransform, eventData.position, _uiCam, out _lastLocalPos))
            return;

        _lastTime = Time.unscaledTime;
        _filteredVSpeed = 0f;

        if (pourParticles) pourParticles.Play(true);
        Debug.Log("[Draggable] BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // pointer → local
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                bottleRT.parent as RectTransform, eventData.position, _uiCam, out localNow))
            return;

        // move UI
        bottleRT.anchoredPosition = localNow;

        // arm/disarm by UI overlap
        bool overlap = RectOverlaps(bottleRT, cauldronZone, _uiCam);
        if (overlap && !cauldronDropZone.isArmed()) cauldronDropZone.Arm(this);
        else if (!overlap && cauldronDropZone.isArmed()) cauldronDropZone.Disarm();

        if (!cauldronDropZone.isArmed()) return;

        StartShakerMode();
    }

    private void StartShakerMode()
    {
        // vertical speed in px/s
        float dt = Mathf.Max(0.0001f, Time.unscaledTime - _lastTime);
        float vSpeedPxPerSec = Mathf.Abs(localNow.y - _lastLocalPos.y) / dt;

        // normalize + smooth
        float normSpeed = vSpeedPxPerSec / Mathf.Max(1f, pixelsPerSecondForUnitSpeed);
        _filteredVSpeed = Mathf.Lerp(_filteredVSpeed, normSpeed, speedLerp);

        // particles rate + tilt
        float emitStrength = (_filteredVSpeed >= minShakeSpeed) ? Mathf.Clamp(_filteredVSpeed, 0f, maxShakeSpeed) : 0f;
        SetParticleEmission(emitStrength);

        // gauge update + log
        if (scalePourManager)
        {
            scalePourManager.OnShakeUpdate(_filteredVSpeed, true, ingredientSubtype);
            Debug.Log($"[Shaker] speed={_filteredVSpeed:0.00} → gauge tick");
        }

        _lastLocalPos = localNow;
        _lastTime = Time.unscaledTime;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _group.blocksRaycasts = true;

        _filteredVSpeed = 0f;
        SetParticleEmission(0f);
        if (scalePourManager) scalePourManager.OnShakeUpdate(0f, false, ingredientSubtype);

        if (cauldronDropZone.isArmed()) cauldronDropZone.Disarm();
        bottleRT.anchoredPosition = _startAnchoredPos;

        Debug.Log("[Draggable] EndDrag");
    }

    // --- helpers ---
    private static bool RectOverlaps(RectTransform a, RectTransform b, Camera cam)
    {
        if (!a || !b) return false;
        Vector3[] ac = new Vector3[4]; Vector3[] bc = new Vector3[4];
        a.GetWorldCorners(ac); b.GetWorldCorners(bc);

        var aMin = RectTransformUtility.WorldToScreenPoint(cam, ac[0]);
        var aMax = RectTransformUtility.WorldToScreenPoint(cam, ac[2]);
        var bMin = RectTransformUtility.WorldToScreenPoint(cam, bc[0]);
        var bMax = RectTransformUtility.WorldToScreenPoint(cam, bc[2]);

        Rect ra = Rect.MinMaxRect(Mathf.Min(aMin.x, aMax.x), Mathf.Min(aMin.y, aMax.y),
                                  Mathf.Max(aMin.x, aMax.x), Mathf.Max(aMin.y, aMax.y));
        Rect rb = Rect.MinMaxRect(Mathf.Min(bMin.x, bMax.x), Mathf.Min(bMin.y, bMax.y),
                                  Mathf.Max(bMin.x, bMax.x), Mathf.Max(bMin.y, bMax.y));
        return ra.Overlaps(rb);
    }

    private void SetParticleEmission(float strength)
    {
        if (!pourParticles) return;
        var em = pourParticles.emission;
        float rate = Mathf.Lerp(0f, 200f, Mathf.Clamp01(strength / Mathf.Max(0.001f, maxShakeSpeed)));
        em.rateOverTime = rate;

        var rot = bottleRT.localEulerAngles;
        rot.z = Mathf.Lerp(0f, -12f, Mathf.Clamp01(strength));
        bottleRT.localEulerAngles = rot;
    }
}
