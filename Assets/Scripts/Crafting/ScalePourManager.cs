using UnityEngine;
using TMPro;

public class ScalePourManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI hintText;

    [Header("Input")]
    [Tooltip("Hold Left Mouse to pour, Right Mouse to confirm")]
    public int pourMouseButton = 0;  // 0 = Left Mouse
    public int confirmMouseButton = 1; // 1 = Right Mouse

    // State
    private string _ingredient;
    private float _target;
    private string _unit;
    private float _tolerance;
    private float _rate;
    private float _timeLimit;

    private float _currentAmount;
    private float _startTime;

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }

    public void Begin(string ingredient, float targetAmount, string unit, float tolerance, float pourRatePerSecond, float timeLimit)
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

        if (title) title.text = $"Add {_ingredient}";
        UpdateUI();

        if (hintText)
            hintText.text = $"Hold LEFT Click to pour, RIGHT Click to confirm\nTarget: {_target}{_unit} (±{_tolerance})";
    }

    void Update()
    {
        if (IsComplete) return;

        // Pour with left mouse hold
        bool pouring = Input.GetMouseButton(pourMouseButton);
        if (pouring)
        {
            _currentAmount += _rate * Time.deltaTime;
            UpdateUI();
        }

        // Confirm with right mouse click
        if (Input.GetMouseButtonDown(confirmMouseButton))
        {
            float error = Mathf.Abs(_currentAmount - _target);
            WasSuccessful = error <= _tolerance;
            IsComplete = true;
            return;
        }

        // Timeout → auto-complete as fail/success
        if (Time.time - _startTime >= _timeLimit)
        {
            float error = Mathf.Abs(_currentAmount - _target);
            WasSuccessful = error <= _tolerance;
            IsComplete = true;
        }
    }

    public void ForceFinish(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
    }

    void UpdateUI()
    {
        if (amountText) amountText.text = $"{_currentAmount:F0}/{_target:F0} {_unit}";
    }
}
