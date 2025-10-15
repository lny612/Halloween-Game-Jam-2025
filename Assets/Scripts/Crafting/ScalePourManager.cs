using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NUnit.Framework.Constraints;

/// <summary>
/// Scale / pouring manager that:
/// - Is armed by BeginForIngredient(...) (called as soon as bottle mouth enters the cauldron zone).
/// - Receives shake updates from IngredientDraggable via OnShakeUpdate(speed, overCauldron).
/// - Converts shake speed into poured amount per second (optionally only when over the cauldron).
/// - Can be disarmed without finishing when the bottle leaves the zone (hover-out).
/// - Finishes on right-click confirm, timeout, or auto-stop window.
/// UI: title, amountText, hintText, amountFill.
/// </summary>
public class ScalePourManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI title;         // "Add Iris Sugar"
    public TextMeshProUGUI amountText;    // "87/100 g"
    public TextMeshProUGUI hintText;      // instruction text
    public Image amountFill;              // horizontal fill bar (0..1), optional

    [Header("Pour Model")]
    [Tooltip("Base ml/g per second at shake speed = 1.0 (recipe’s pourRatePerSecond seeds this).")]
    public float basePourPerSecond = 80f;

    [Tooltip("Curve on shake speed (1 = linear, >1 rewards hard shakes, <1 flattens).")]
    public float speedPower = 1.0f;

    [Tooltip("If true, only add amount when IngredientDraggable reports bottle mouth over the cauldron.")]
    public bool requireOverCauldron = true;

    [Header("Confirm / Timeout")]
    [Tooltip("Right-click to confirm (set -1 to disable manual confirm).")]
    public int confirmMouseButton = 1; // 1 = Right Mouse
    [Tooltip("Auto-complete if within this absolute window of target (0 = off).")]
    public float autoStopWhenAtTargetWithin = 0f; // e.g., 0.5

    // --------- Runtime state ---------
    private string _ingredient;
    private float _target;
    private string _unit;
    private float _tolerance;
    private float _timeLimit;

    private float _currentAmount;
    private float _startTime;

    private bool _armed;
    private bool _overCauldron;
    private float _currentShakeSpeed; // normalized shake speed reported by IngredientDraggable

    // Track which ingredient armed us (so only that one can disarm)
    private IngredientDraggable _owner;

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }
    public bool IsArmed => _armed;

    // -------------------------------------------
    //  SHAKE FEED (called every drag frame by the bottle)
    // -------------------------------------------
    public void OnShakeUpdate(float shakeSpeed, bool overCauldron)
    {
        if (!_armed || IsComplete) return;
        _currentShakeSpeed = shakeSpeed;
        _overCauldron = overCauldron;
    }
    /// <summary>
    /// Start this pour step for the given ingredient, and remember who armed us.
    /// Intended to be called immediately when the bottle enters the cauldron zone (no mouse-up required).
    /// </summary>

    public void InitializeCurrentRecipeStep(RecipeStep currentRecipeStep)
    {
        _target = currentRecipeStep.targetAmount;
        _unit = currentRecipeStep.unit;
        _tolerance = Mathf.Abs(currentRecipeStep.tolerance);
        _timeLimit = currentRecipeStep.timeLimit;

        IsComplete = false;
        WasSuccessful = false;

        //Set Detailed Step
        if (title) title.text = $"Add {_ingredient}";
        if (hintText)
        {
            string confirmHint = (confirmMouseButton >= 0) ? "Right-Click to confirm." : "";
            hintText.text = $"Drag bottle over the cauldron.\nShake it UP↕DOWN to pour. {confirmHint}\nTarget: {_target}{_unit} (±{_tolerance})";
        }

        UpdateUI();
    }

    public void BeginForIngredient(IngredientDraggable ingredient)
    {
        _ingredient = ingredient.name;
        
        // recipe-specific baseline (so different ingredients feel different)
        basePourPerSecond = Mathf.Max(1f, ingredient.pourRatePerSecond);

        _currentAmount = 0f;
        _startTime = Time.time;
        _armed = true;
        _overCauldron = false;
        _currentShakeSpeed = 0f;
    }

    /// <summary>
    /// Stop/pause the step without evaluating success/fail.
    /// Use when the bottle LEAVES the cauldron zone while still holding LMB.
    /// Only the owner that armed the manager can disarm it.
    /// </summary>
    public void DisarmWithoutFinish(IngredientDraggable requester)
    {
        if (_owner != null && requester != _owner) return; // ignore strangers

        _armed = false;
        _owner = null;
        _currentShakeSpeed = 0f;
        _overCauldron = false;
        // Keep UI if you want the player to see current progress, or hide it:
        // gameObject.SetActive(false);
        UpdateUI();
    }

    public void ForceFinish(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
        _armed = false;
        _owner = null;
        UpdateUI();
    }

    void OnEnable()
    {
        // reset session state when enabled
        _currentShakeSpeed = 0f;
        _overCauldron = false;
    }

    void Update()
    {
        if (!_armed || IsComplete) return;

        // Convert shake speed -> poured amount (per second)
        if (_currentShakeSpeed > 0f && (!requireOverCauldron || _overCauldron))
        {
            float speedFactor = Mathf.Pow(_currentShakeSpeed, Mathf.Max(0.2f, speedPower));
            float pourPerSec = basePourPerSecond * speedFactor;

            _currentAmount += pourPerSec * Time.deltaTime;
            UpdateUI();

            // Optional auto-stop if close enough
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

        // Manual confirm: Right Mouse
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

    private void CompleteByTolerance()
    {
        float error = Mathf.Abs(_currentAmount - _target);
        WasSuccessful = error <= _tolerance;
        IsComplete = true;
        _armed = false;
        _owner = null;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool ok = Mathf.Abs(_currentAmount - _target) <= _tolerance;

        if (amountText)
        {
            amountText.text = $"{_currentAmount:F0}/{_target:F0} {_unit}";
            amountText.color = ok ? new Color(0.6f, 1f, 0.6f) : Color.white;
        }

        if (amountFill)
        {
            float normalized = Mathf.Clamp01(_currentAmount / Mathf.Max(1f, _target));
            amountFill.type = Image.Type.Filled;
            amountFill.fillMethod = Image.FillMethod.Horizontal;
            amountFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            amountFill.fillAmount = normalized;
            amountFill.color = ok ? new Color(0.6f, 1f, 0.6f, 1f)
                                  : new Color(1f, 0.85f, 0.45f, 1f);
        }
    }
}
