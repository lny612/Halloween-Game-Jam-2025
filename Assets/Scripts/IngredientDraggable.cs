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
    public RectTransform dragLayer; // ← assign a top-level overlay under the same Canvas

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
    private bool _startPosCaptured;
    private Vector2 localNow;

    // layout-safe restore
    private Transform _origParent;
    private int _origSiblingIndex;
    private LayoutElement _layoutElement;

    // drag offset to keep cursor and object aligned
    private Vector2 _dragOffset;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!bottleRT) bottleRT = _rt;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!dragLayer) dragLayer = canvas.transform as RectTransform; // fallback

        InstantiatePourParticles();
    }

    void Start()
    {
        // Capture start position AFTER first layout pass
        StartCoroutine(CaptureStartPosNextFrame());
    }

    System.Collections.IEnumerator CaptureStartPosNextFrame()
    {
        yield return null; // wait one frame so HorizontalLayoutGroup has positioned things
        _startAnchoredPos = bottleRT.anchoredPosition;
        _startPosCaptured = true;
    }

    private void InstantiatePourParticles()
    {
        if (pourParticles != null)
        {
            Vector3 topOfBottleLocalPos = new Vector3(0f, bottleRT.rect.height * 0.5f, 0f);
            pourParticles = Instantiate(pourParticles, bottleRT);
            pourParticles.transform.localPosition = topOfBottleLocalPos;
            pourParticles.transform.localRotation = Quaternion.identity;
            pourParticles.transform.localScale = Vector3.one;
            SetParticleEmission(0f);
        }
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

        // Ensure we have the true starting position (layout settled)
        if (!_startPosCaptured)
        {
            _startAnchoredPos = bottleRT.anchoredPosition;
            _startPosCaptured = true;
        }

        _lastTime = Time.unscaledTime;
        _filteredVSpeed = 0f;

        // Layout opt-out + store original hierarchy
        _layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        _layoutElement.ignoreLayout = true;

        _origParent = bottleRT.parent;
        _origSiblingIndex = bottleRT.GetSiblingIndex();

        // Reparent to DragLayer first so all future pointer->local conversions use the new parent
        bottleRT.SetParent(dragLayer, true);      // keep world pos while dragging freely
        bottleRT.SetAsLastSibling();              // render on top within DragLayer

        // Ensure on-canvas plane (for Screen Space - Camera / World Space)
        var lp = bottleRT.localPosition; lp.z = 0f; bottleRT.localPosition = lp;

        // Compute pointer in DragLayer's local space
        RectTransform dragParent = bottleRT.parent as RectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragParent, eventData.position, _uiCam, out _lastLocalPos))
            return;

        // Capture initial click-to-pivot offset so the bottle doesn't jump under the cursor
        _dragOffset = bottleRT.anchoredPosition - _lastLocalPos;

        if (pourParticles) pourParticles.Play(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Always convert using the CURRENT parent (dragLayer) after reparenting
        RectTransform dragParent = bottleRT.parent as RectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragParent, eventData.position, _uiCam, out localNow))
            return;

        // Apply the preserved offset so the cursor stays exactly where you grabbed
        bottleRT.anchoredPosition = localNow + _dragOffset;

        // Cross-canvas safe overlap (fixes gauge not increasing)
        bool overlap = RectOverlapsCrossCanvas(bottleRT, cauldronZone);
        if (cauldronDropZone != null)
        {
            if (overlap && !cauldronDropZone.isArmed()) cauldronDropZone.Arm(this);
            else if (!overlap && cauldronDropZone.isArmed()) cauldronDropZone.Disarm();
        }

        if (!overlap) return;

        StartShakerMode();
    }

    private void StartShakerMode()
    {
        float dt = Mathf.Max(0.0001f, Time.unscaledTime - _lastTime);
        float vSpeedPxPerSec = Mathf.Abs(localNow.y - _lastLocalPos.y) / dt;

        float normSpeed = vSpeedPxPerSec / Mathf.Max(1f, pixelsPerSecondForUnitSpeed);
        _filteredVSpeed = Mathf.Lerp(_filteredVSpeed, normSpeed, speedLerp);

        float emitStrength = (_filteredVSpeed >= minShakeSpeed) ? Mathf.Clamp(_filteredVSpeed, 0f, maxShakeSpeed) : 0f;
        SetParticleEmission(emitStrength);

        if (scalePourManager)
        {
            scalePourManager.OnShakeUpdate(_filteredVSpeed, true, ingredientSubtype);
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

        if (cauldronDropZone != null && cauldronDropZone.isArmed()) cauldronDropZone.Disarm();

        // Restore parent/sibling and layout participation
        bottleRT.SetParent(_origParent as RectTransform, true);
        bottleRT.SetSiblingIndex(_origSiblingIndex);

        if (_layoutElement) _layoutElement.ignoreLayout = false;

        // Force a layout rebuild to avoid a “half frame” snap
        var parentRT = _origParent as RectTransform;
        if (parentRT) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);

        // Now set the anchored position to the known start (after layout rebuild)
        if (_startPosCaptured) bottleRT.anchoredPosition = _startAnchoredPos;

        // Optional: small tilt reset
        var rot = bottleRT.localEulerAngles; rot.z = 0f; bottleRT.localEulerAngles = rot;
    }

    // --- helpers ---

    // NEW: cross-canvas overlap (each rect uses its own canvas camera)
    private static bool RectOverlapsCrossCanvas(RectTransform a, RectTransform b)
    {
        if (!a || !b) return false;

        Camera camA = GetCanvasCamera(a);
        Camera camB = GetCanvasCamera(b);

        Vector3[] ac = new Vector3[4];
        Vector3[] bc = new Vector3[4];
        a.GetWorldCorners(ac);
        b.GetWorldCorners(bc);

        Vector2 aMin = RectTransformUtility.WorldToScreenPoint(camA, ac[0]);
        Vector2 aMax = RectTransformUtility.WorldToScreenPoint(camA, ac[2]);
        Vector2 bMin = RectTransformUtility.WorldToScreenPoint(camB, bc[0]);
        Vector2 bMax = RectTransformUtility.WorldToScreenPoint(camB, bc[2]);

        Rect ra = Rect.MinMaxRect(
            Mathf.Min(aMin.x, aMax.x), Mathf.Min(aMin.y, aMax.y),
            Mathf.Max(aMin.x, aMax.x), Mathf.Max(aMin.y, aMax.y)
        );
        Rect rb = Rect.MinMaxRect(
            Mathf.Min(bMin.x, bMax.x), Mathf.Min(bMin.y, bMax.y),
            Mathf.Max(bMin.x, bMax.x), Mathf.Max(bMin.y, bMax.y)
        );

        return ra.Overlaps(rb);
    }

    private static Camera GetCanvasCamera(RectTransform rt)
    {
        var cv = rt.GetComponentInParent<Canvas>();
        if (cv == null) return null;
        if (cv.renderMode == RenderMode.ScreenSpaceOverlay) return null; // overlay uses null
        return cv.worldCamera != null ? cv.worldCamera : Camera.main;
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
