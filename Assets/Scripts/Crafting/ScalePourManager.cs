using UnityEngine;
using TMPro;

public class ScalePourManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI hintText;

    [Header("Input")]
    public KeyCode pourKey = KeyCode.Space;
    public bool useMouse0 = true;
    public KeyCode confirmKey = KeyCode.Return;

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
        if (hintText) hintText.text = $"Hold Space/Click to pour, Enter to confirm\nTarget: {_target}{_unit} (±{_tolerance})";
    }

    void Update()
    {
        if (IsComplete) return;

        // Pour
        bool pouring = Input.GetKey(pourKey) || (useMouse0 && Input.GetMouseButton(0));
        if (pouring)
        {
            _currentAmount += _rate * Time.deltaTime;
            UpdateUI();
        }

        // Confirm
        if (Input.GetKeyDown(confirmKey))
        {
            float error = Mathf.Abs(_currentAmount - _target);
            WasSuccessful = error <= _tolerance;
            IsComplete = true;
            return;
        }

        // Timeout → auto-complete as fail/success based on tolerance
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
