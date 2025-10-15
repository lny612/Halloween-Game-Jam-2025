using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScalePourManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI title;         // "Add Iris Sugar"
    public TextMeshProUGUI amountText;    // "87/100 g"
    public TextMeshProUGUI hintText;      // "Hold LMB to pour, RMB to confirm"
    public Image amountFill;              // optional: a horizontal fill bar (0..1)

    [Header("Input")]
    [Tooltip("Hold Left Mouse to pour; release to stop.")]
    public int pourMouseButton = 0;         // 0 = Left
    [Tooltip("Optional confirm if you want explicit confirm; set to -1 to disable confirm.")]
    public int confirmMouseButton = 1;      // 1 = Right (set to -1 to disable)

    // Runtime state
    private string _ingredient;
    private float _target;
    private string _unit;
    private float _tolerance;
    private float _rate;
    private float _timeLimit;

    private float _currentAmount;
    private float _startTime;
    private bool _armed;                   // becomes true after drop
    private bool _pouring;                 // true while LMB held & armed

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }

    void Update()
    {
        if (!_armed || IsComplete) return;

        // Start/stop pouring strictly from mouse hold
        _pouring = Input.GetMouseButton(pourMouseButton);

        if (_pouring)
        {
            _currentAmount += _rate * Time.deltaTime;
            UpdateUI();
        }

        // Optional manual confirm (Right Click)
        if (confirmMouseButton >= 0 && Input.GetMouseButtonDown(confirmMouseButton))
        {
            CompleteByTolerance();
            return;
        }

        // Timeout auto-complete (if you set timeLimit > 0)
        if (_timeLimit > 0f && Time.time - _startTime >= _timeLimit)
        {
            CompleteByTolerance();
        }
    }

    /// <summary>
    /// Called by the CauldronDropZone when the correct ingredient is dropped.
    /// Arms the scale for pouring. You can call this directly if you don't use drag/drop.
    /// </summary>
    public void BeginForIngredient(string ingredient, float targetAmount, string unit, float tolerance, float pourRatePerSecond, float timeLimit)
    {
        _ingredient = ingredient;
        _target = targetAmount;
        _unit = unit;
        _tolerance = Mathf.Abs(tolerance);
        _rate = Mathf.Max(1f, pourRatePerSecond);
        _timeLimit = timeLimit;

        _currentAmount = 0f;
        _startTime = Time.time;

        IsComplete = false;
        WasSuccessful = false;
        _armed = true;        // now ready to pour
        _pouring = false;

        if (title) title.text = $"Add {_ingredient}";
        if (hintText) hintText.text = (confirmMouseButton >= 0)
            ? $"Drag ingredient to cauldron to start. Hold LEFT click to pour, RIGHT click to confirm.\nTarget: {_target}{_unit} (±{_tolerance})"
            : $"Drag ingredient to cauldron to start. Hold LEFT click to pour.\nAuto-finishes on timeout.\nTarget: {_target}{_unit} (±{_tolerance})";

        UpdateUI();
        gameObject.SetActive(true); // ensure panel visible
    }

    public void ForceFinish(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
        _armed = false;
        _pouring = false;
    }

    private void CompleteByTolerance()
    {
        float error = Mathf.Abs(_currentAmount - _target);
        WasSuccessful = error <= _tolerance;
        IsComplete = true;
        _armed = false;
        _pouring = false;
        UpdateUI(); // color flash can happen here if you add it
    }

    private void UpdateUI()
    {
        // Color hint: green if within tolerance, else white
        bool ok = Mathf.Abs(_currentAmount - _target) <= _tolerance;

        if (amountText)
        {
            // Show 0 decimals for grams/ml; tweak as needed
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
            amountFill.color = ok ? new Color(0.6f, 1f, 0.6f, 1f) : new Color(1f, 0.85f, 0.45f, 1f);
        }
    }
}
