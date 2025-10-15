using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Integrated cellar draggable + bottle shaker with hover-to-start:
/// - In normal mode (shakerMode = false): behaves like ghost-drag cellar icon.
/// - In shaker mode (shakerMode = true):
///     * Drag the actual bottle.
///     * As soon as the bottle mouth ENTERS the cauldron zone (still holding LMB),
///       ScalePourManager.BeginForIngredient(..., owner: this) is called automatically.
///     * When it LEAVES the zone (still holding), DisarmWithoutFinish(this) is called (no evaluation).
///     * On mouse-up anywhere: stop particles, report zero speed, disarm if needed,
///       snap bottle back to its start position, exit shaker mode.
/// </summary>
public class IngredientDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ingredient Data")]
    public string ingredientName = "Iris Sugar";
    public float pourRatePerSecond = 150f; // seeds ScalePourManager.basePourPerSecond

    [Header("Manager")]
    public ScalePourManager scalePourManager;

    [Header("Drag Visuals (Cellar Mode)")]
    public Canvas canvas;              // assign your UI Canvas
    public Image ghostImagePrefab;     // optional: drag ghost

    [Header("Bottle Shaker (Pour Mode)")]
    public RectTransform bottleRT;           // actual bottle rect (defaults to own RT)
    public RectTransform cauldronZone;       // hit area over the cauldron (RectTransform)
    public ParticleSystem pourParticles;     // particle system at bottle mouth
    [Tooltip("Local offset from bottle center to mouth (pixels). Tune for your sprite.")]
    public Vector2 mouthLocalOffset = new Vector2(0f, 50f);

    [Header("Shake → Pour Tuning")]
    [Tooltip("Pixels/sec of vertical motion that equals speed = 1.0")]
    public float pixelsPerSecondForUnitSpeed = 500f;
    [Tooltip("Min normalized speed to start emitting/pouring")]
    public float minShakeSpeed = 0.15f;
    [Tooltip("Clamp of normalized shake speed")]
    public float maxShakeSpeed = 3f;
    [Range(0f, 1f)]
    [Tooltip("Smoothing for speed spikes (higher = snappier)")]
    public float speedLerp = 0.25f;
    

    // --- Runtime ---
    private RectTransform _rt;
    private CanvasGroup _group;
    private Image _ghostInstance;
    private bool _dragging;
    private bool _shakerMode;                  // false = cellar ghost drag; true = bottle shaker
    private Camera _uiCam;

    // Shaker state
    private Vector2 _lastLocalPos;
    private float _lastTime;
    private float _filteredVSpeed;
    private bool _wasOverCauldron;
    private Vector2 _startAnchoredPos;         // snap-back target

    // Hook to manager (speed → poured amount)
    private Vector2 localNow;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        if (!bottleRT) bottleRT = _rt;

        if (pourParticles) SetParticleEmission(0f);
        _startAnchoredPos = bottleRT.anchoredPosition; // remember starting pos for snap-back
    }

    /// <summary>
    /// Switch between cellar ghost dragging and bottle shaking.
    /// Call this when opening the pouring panel / arming the bottle.
    /// </summary>
    /*public void SetShakerMode(bool enabled,  RectTransform cauldron = null)
    {
        _shakerMode = enabled;
        if (cauldron != null) cauldronZone = cauldron;

        // Reset shaker state
        _filteredVSpeed = 0f;
        _dragging = false;
        _wasOverCauldron = false;
        if (pourParticles) SetParticleEmission(0f);

        // In shaker mode we move the actual bottle; disable ghost if any
        if (_ghostInstance) { Destroy(_ghostInstance.gameObject); _ghostInstance = null; }

        // Ensure Image is raycastable for dragging
        var img = GetComponent<Image>();
        if (img) img.raycastTarget = true;
    }*/

    // ---------------- Drag Interfaces ----------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        _uiCam = eventData.pressEventCamera;

        if (!_shakerMode)
        {
            // Cellar mode: ghost drag & allow drop zones to receive
            _group.blocksRaycasts = false;

            if (ghostImagePrefab != null && canvas != null)
            {
                _ghostInstance = Instantiate(ghostImagePrefab, canvas.transform);
                _ghostInstance.sprite = GetComponent<Image>()?.sprite;
                _ghostInstance.raycastTarget = false;
            }
        }
        else
        {
            // Shaker mode: drag the actual bottle and prep speed calc
            _dragging = true;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    bottleRT.parent as RectTransform, eventData.position, _uiCam, out _lastLocalPos))
                return;

            _lastTime = Time.unscaledTime;
            _filteredVSpeed = 0f;
            _wasOverCauldron = false;

            if (pourParticles) pourParticles.Play(true);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!_shakerMode)
        {
            // Cellar mode: move ghost, or self as fallback
            if (_ghostInstance != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, eventData.position, _uiCam, out var localPos);
                _ghostInstance.rectTransform.anchoredPosition = localPos;
            }
            else
            {
                _rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
            return;
        }

        StartShakerMode(eventData);

    }
    public void StartShakerMode(PointerEventData eventData)
    {

        // Shaker mode: drag actual bottle and compute vertical shake speed
        if (!_dragging || eventData.button != PointerEventData.InputButton.Left) return;

        CauldronDropZone cauldronDropZone = cauldronZone.gameObject.GetComponent<CauldronDropZone>();
        if (!cauldronDropZone.isArmed()) return;

        // Move bottle with pointer
        bottleRT.anchoredPosition = localNow;

        // Vertical speed (px/s)
        float dt = Mathf.Max(0.0001f, Time.unscaledTime - _lastTime);
        float vSpeedPxPerSec = Mathf.Abs(localNow.y - _lastLocalPos.y) / dt;

        // Normalize & smooth
        float normSpeed = vSpeedPxPerSec / Mathf.Max(1f, pixelsPerSecondForUnitSpeed);
        _filteredVSpeed = Mathf.Lerp(_filteredVSpeed, normSpeed, speedLerp);

        // Particles & tilt (only when actually over cauldron and shaking enough)
        float emitStrength = (cauldronDropZone.isArmed() && _filteredVSpeed >= minShakeSpeed)
            ? Mathf.Clamp(_filteredVSpeed, 0f, maxShakeSpeed) : 0f;
        SetParticleEmission(emitStrength);

        // Report shake every frame (manager ignores if not armed)
        if (scalePourManager) scalePourManager.OnShakeUpdate(_filteredVSpeed, cauldronDropZone.isArmed());

        _lastLocalPos = localNow;
        _lastTime = Time.unscaledTime;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        /*if (!_shakerMode)
        {
            // Cellar mode: restore raycasts and clean ghost
            _group.blocksRaycasts = true;
            if (_ghostInstance != null) Destroy(_ghostInstance.gameObject);
            return;
        }

        // Shaker mode: on mouse-up, stop emission & zero speed, possibly disarm
        _dragging = false;
        _filteredVSpeed = 0f;
        SetParticleEmission(0f);
        if (scalePourManager) scalePourManager.OnShakeUpdate(0f, false);

        // If we were over cauldron, disarm (no finish)
        if (scalePourManager && _wasOverCauldron)
            scalePourManager.DisarmWithoutFinish(this);

        // Snap bottle back to its original position and exit shaker mode
        bottleRT.anchoredPosition = _startAnchoredPos;
        SetShakerMode(false, null, null);*/
    }

    // ---------------- Helpers ----------------

    private void SetParticleEmission(float strength)
    {
        if (!pourParticles) return;
        var em = pourParticles.emission;
        float rate = Mathf.Lerp(0f, 200f, Mathf.Clamp01(strength / Mathf.Max(0.001f, maxShakeSpeed)));
        em.rateOverTime = rate;

        // Juice: tilt bottle based on strength
        var rot = bottleRT.localEulerAngles;
        rot.z = Mathf.Lerp(0f, -12f, Mathf.Clamp01(strength));
        bottleRT.localEulerAngles = rot;
    }
}
