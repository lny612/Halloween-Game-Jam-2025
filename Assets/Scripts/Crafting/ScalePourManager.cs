using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Scale / pouring manager (UI-only):
/// - Arm via BeginForIngredient(owner) when bottle enters cauldron zone.
/// - Receive normalized shake speed via OnShakeUpdate(speed, overCauldron).
/// - Adds poured amount each frame proportional to speed.
/// - Optional auto-stop window, manual confirm, and timeout.
/// </summary>
public class ScalePourManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI title;         // "Add Iris Sugar"
    public TextMeshProUGUI amountText;    // "87/100 g"
    //public Image amountFill;              // horizontal fill bar (0..1), optional

    [Header("Pour Model")]
    [Tooltip("Base ml/g per second at shake speed = 1.0.")]
    public float basePourPerSecond = 80f;

    [Tooltip("Curve on shake speed (1 = linear, >1 rewards hard shakes, <1 flattens).")]
    public float speedPower = 1.0f;

    [Tooltip("If true, only add amount when bottle is over cauldron.")]
    public bool requireOverCauldron = true;

    [Header("Confirm / Timeout")]
    [Tooltip("Right-click to confirm (set -1 to disable manual confirm).")]
    public int confirmMouseButton = 1; // 1 = Right Mouse
    [Tooltip("Auto-complete if within this absolute window of target (0 = off).")]
    public float autoStopWhenAtTargetWithin = 0f; // e.g., 0.5

    // --------- Runtime state ---------
    private string _ingredientName = "";
    private IngredientSubtype _ingredient = IngredientSubtype.None;
    private float _target = 0f;
    private string _unit = "ml";
    private float _tolerance = 0f;
    private float _timeLimit = 0f;

    private float _currentAmount;
    private float _startTime;

    private bool _armed;
    private bool _overCauldron;
    private float _currentShakeSpeed; // normalized [0,∞)

    private IngredientDraggable _owner;

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }
    public bool IsArmed => _armed;

    // Called by the draggable each drag frame
    public void OnShakeUpdate(float shakeSpeed, bool overCauldron, IngredientSubtype ingredientSubtype)
    {
        if (ingredientSubtype != _ingredient) return;
        if (!_armed || IsComplete) return;
        _currentShakeSpeed = shakeSpeed;
        _overCauldron = overCauldron;
    }

    /// <summary> Seed this step from the recipe definition (target, unit, tolerance, etc.). </summary>
    public void InitializeCurrentRecipeStep(AddIngredientStep currentRecipeStep)
    {
        _ingredientName = currentRecipeStep.ingredientName;
        _ingredient = currentRecipeStep.ingredientSubType;
        _target = currentRecipeStep.targetAmount;
        _unit = currentRecipeStep.unit;
        _tolerance = Mathf.Abs(currentRecipeStep.tolerance);
        _timeLimit = currentRecipeStep.timeLimit;

        IsComplete = false;
        WasSuccessful = false;
        _currentAmount = 0f;

        if (title) title.text = $"Add {_ingredientName}";
        if (amountText) amountText.text = $"0/{_target} {_unit}";
        UpdateUI();
    }

    /// <summary>
    /// Arm pouring as soon as the bottle enters the zone (UI overlap).
    /// </summary>
    public void BeginForIngredient(IngredientDraggable ingredient)
    {
        _owner = ingredient;
        _armed = true;
        _startTime = Time.time;
        // If you want per-ingredient baseline, set basePourPerSecond here.
        Debug.Log($"[ScalePour] ARMED by {(_owner ? _owner.name : "NULL")}, target={_target}{_unit}, tol=±{_tolerance}");
        UpdateUI();
    }

    /// <summary> Disarm (hover-out) without finish. </summary>
    public void DisarmWithoutFinish(IngredientDraggable requester)
    {
        if (_owner != null && requester != _owner) return;
        _armed = false;
        _owner = null;
        _currentShakeSpeed = 0f;
        _overCauldron = false;
        UpdateUI();
        Debug.Log("[ScalePour] DISARM (no finish)");
    }

    public void ForceFinish(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
        _armed = false;
        _owner = null;
        UpdateUI();
        Debug.Log($"[ScalePour] FORCE FINISH → success={success}");
    }

    void OnEnable()
    {
        _currentShakeSpeed = 0f;
        _overCauldron = false;
    }

    void Update()
    {
        if (!_armed || IsComplete) return;

        // Integrate poured amount ~ speed each frame
        if (_currentShakeSpeed > 0f && (!requireOverCauldron || _overCauldron))
        {
            float speedFactor = Mathf.Pow(_currentShakeSpeed, Mathf.Max(0.2f, speedPower));
            float pourPerSec = basePourPerSecond * speedFactor;
            float delta = pourPerSec * Time.deltaTime;

            _currentAmount = Mathf.Max(0f, _currentAmount + delta);
            UpdateUI();

            Debug.Log($"[ScalePour] +{delta:0.00} {_unit} (speed={_currentShakeSpeed:0.00}, rate={pourPerSec:0.0}/s) → {_currentAmount:0.00}/{_target:0.00} {_unit}");

            // Optional auto-stop
            if (autoStopWhenAtTargetWithin > 0f)
            {
                float err = Mathf.Abs(_currentAmount - _target);
                if (err <= Mathf.Max(0.001f, autoStopWhenAtTargetWithin))
                {
                    CompleteByTolerance();
                    return;
                }
            }
        }

        // Manual confirm
        if (confirmMouseButton >= 0 && Input.GetMouseButtonDown(confirmMouseButton))
        {
            CompleteByTolerance();
            return;
        }

        // Timeout
        if (_timeLimit > 0f && Time.time - _startTime >= _timeLimit)
        {
            CompleteByTolerance();
        }
    }

    public void CompleteByTolerance()
    {
        float error = Mathf.Abs(_currentAmount - _target);
        WasSuccessful = error <= _tolerance;
        IsComplete = true;
        _armed = false;
        _owner = null;
        UpdateUI();
        Debug.Log($"[ScalePour] COMPLETE → success={WasSuccessful}, final={_currentAmount:0.00}/{_target:0.00} {_unit} (±{_tolerance})");
    }

    private void UpdateUI()
    {
        bool ok = Mathf.Abs(_currentAmount - _target) <= _tolerance;

        if (amountText)
        {
            amountText.text = $"{_currentAmount:F0}/{_target:F0} {_unit}";
            amountText.color = ok ? new Color(0.6f, 1f, 0.6f) : Color.white;
        }

        /*if (amountFill)
        {
            float normalized = Mathf.Clamp01(_currentAmount / Mathf.Max(1f, _target));
            amountFill.type = Image.Type.Filled;
            amountFill.fillMethod = Image.FillMethod.Horizontal;
            amountFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            amountFill.fillAmount = normalized;
            amountFill.color = ok ? new Color(0.6f, 1f, 0.6f, 1f)
                                  : new Color(1f, 0.85f, 0.45f, 1f);
        }*/

        if (title && !string.IsNullOrEmpty(_ingredientName))
            title.text = $"Add {_ingredientName}";
    }
}
