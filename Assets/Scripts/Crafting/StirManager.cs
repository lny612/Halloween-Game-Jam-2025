using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Potion Craft–style stirring:
/// - Click-and-hold LMB on the stir zone to "grab" the rod.
/// - Drag left/right; horizontal movement adds "stir stacks".
/// - Gauge fills from 0..stacksPerStir (default 20). Every 20 stacks counts as 1 "stir".
/// - When currentStirs >= targetStirs (from Begin), complete.
/// </summary>
public class StirManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI")]
    [Tooltip("Clickable/drag area in which stirring is valid (the pot area).")]
    public RectTransform stirZone;

    [Tooltip("Optional: a handle/rod sprite that follows the cursor within the zone.")]
    public RectTransform rodHandle;

    [Tooltip("Optional: fill image for the per-stir gauge (0..1).")]
    public Image gaugeFill;

    [Header("Stir Tuning")]
    [Tooltip("How many stacks make ONE 'stir' (20 = every 20 stacks counts as 1).")]
    public int stacksPerStir = 20;

    [Tooltip("How many pixels of horizontal drag = 1 stack. Lower = easier, Higher = harder.")]
    public float pixelsPerStack = 12f;

    [Tooltip("Clamp the rodHandle position to the stirZone rect.")]
    public bool clampHandleToZone = true;

    [Header("Input")]
    public int stirMouseButton = 0; // 0 = LMB

    // --- Runtime state ---
    private int _targetStirs;
    private float _timeLimit;
    private float _startTime;

    private int _currentStirs;          // how many full stirs achieved
    private float _currentStacks;         // 0..stacksPerStir-ε (resets when >= stacksPerStir)
    private bool _isDragging;
    private Vector2 _lastPointerLocal;    // last local pos inside stirZone
    private Camera _uiCam;                // cached event camera

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }

    // --- Public API ---
    public void Begin(int requiredStirs, float _minIntervalIgnored, float timeLimit)
    {
        // _minIntervalIgnored kept for compatibility; not used in drag model.
        _targetStirs = Mathf.Max(1, requiredStirs);
        _timeLimit = timeLimit;

        _currentStirs = 0;
        _currentStacks = 0f;
        IsComplete = false;
        WasSuccessful = false;
        _startTime = Time.time;

        UpdateGauge();

        // optional: center rod at start
        if (rodHandle && stirZone)
            rodHandle.anchoredPosition = Vector2.zero;
    }

    // IPointer interfaces (require a GraphicRaycaster on the Canvas + EventSystem in scene)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != (PointerEventData.InputButton)stirMouseButton) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                stirZone, eventData.position, eventData.pressEventCamera, out _lastPointerLocal))
            return;

        _uiCam = eventData.pressEventCamera;
        _isDragging = true;

        // Snap rod to pointer
        if (rodHandle)
            rodHandle.anchoredPosition = ClampIfNeeded(_lastPointerLocal);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        if (eventData.button != (PointerEventData.InputButton)stirMouseButton) return;

        // Current pointer local pos in the zone
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                stirZone, eventData.position, _uiCam, out var localNow))
            return;

        // Horizontal movement (abs dx) adds stacks
        float dx = Mathf.Abs(localNow.x - _lastPointerLocal.x);
        if (dx > 0f)
        {
            float stacksGained = dx / Mathf.Max(1f, pixelsPerStack);
            _currentStacks += stacksGained;

            // Convert stacks -> completed "stirs"
            while (_currentStacks >= stacksPerStir)
            {
                _currentStacks -= stacksPerStir;
                _currentStirs++;

                if (_currentStirs >= _targetStirs)
                {
                    Complete(true);
                    return;
                }
            }

            UpdateGauge();
        }

        _lastPointerLocal = localNow;

        // Move the rod visually
        if (rodHandle)
            rodHandle.anchoredPosition = ClampIfNeeded(localNow);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != (PointerEventData.InputButton)stirMouseButton) return;
        _isDragging = false;
    }

    void Update()
    {
        if (IsComplete) return;

        // Time limit handling (optional)
        if (_timeLimit > 0f && Time.time - _startTime >= _timeLimit)
        {
            Complete(_currentStirs >= _targetStirs);
        }
    }

    // --- Helpers ---
    private void Complete(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
        _isDragging = false;
        UpdateGauge(); // final visual
    }

    public void ForceFinish(bool success)
    {
        Complete(success);
    }

    private void UpdateGauge()
    {
        if (!gaugeFill) return;

        // Gauge shows the *current sub-stir progress* (0..1 of stacksPerStir)
        float t = (_targetStirs <= 0) ? 0f : Mathf.Clamp01(_currentStacks / Mathf.Max(1, stacksPerStir));
        gaugeFill.type = Image.Type.Filled;
        gaugeFill.fillMethod = Image.FillMethod.Horizontal;
        gaugeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        gaugeFill.fillAmount = t;

        // Optional color cue: green when sub-gauge is nearly a full stir
        gaugeFill.color = (t >= 0.95f) ? new Color(0.6f, 1f, 0.6f, 1f)
                                       : new Color(1f, 0.9f, 0.6f, 1f);
    }

    private Vector2 ClampIfNeeded(Vector2 local)
    {
        if (!clampHandleToZone || stirZone == null) return local;
        var r = stirZone.rect;
        local.x = Mathf.Clamp(local.x, r.xMin, r.xMax);
        local.y = Mathf.Clamp(local.y, r.yMin, r.yMax);
        return local;
    }
}
