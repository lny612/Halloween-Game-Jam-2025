using UnityEngine;
using TMPro;

public class StirManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text prompt;
    public TMP_Text progressText;

    [Header("Input")]
    public KeyCode stirKey = KeyCode.Space;
    public bool useMouse0 = true;

    // State
    private int _targetStirs;
    private float _minInterval;
    private float _timeLimit;
    private int _currentStirs;
    private float _lastStirTime;
    private float _startTime;

    public bool IsComplete { get; private set; }
    public bool WasSuccessful { get; private set; }

    public void Begin(int requiredStirs, float minInterval, float timeLimit)
    {
        _targetStirs = requiredStirs;
        _minInterval = Mathf.Max(0f, minInterval);
        _timeLimit = timeLimit;

        _currentStirs = 0;
        _lastStirTime = -999f;
        _startTime = Time.time;
        IsComplete = false;
        WasSuccessful = false;

        if (prompt) prompt.text = $"Stir {requiredStirs} times";
        UpdateUI();
    }

    void Update()
    {
        if (IsComplete) return;

        // input
        bool pressed = Input.GetKeyDown(stirKey) || (useMouse0 && Input.GetMouseButtonDown(0));
        if (pressed)
        {
            float now = Time.time;
            if (now - _lastStirTime >= _minInterval)
            {
                _currentStirs++;
                _lastStirTime = now;
                UpdateUI();

                if (_currentStirs >= _targetStirs)
                {
                    IsComplete = true;
                    WasSuccessful = true;
                    return;
                }
            }
        }

        // time
        if (Time.time - _startTime >= _timeLimit)
        {
            IsComplete = true;
            WasSuccessful = _currentStirs >= _targetStirs;
        }
    }

    public void ForceFinish(bool success)
    {
        IsComplete = true;
        WasSuccessful = success;
    }

    void UpdateUI()
    {
        if (progressText) progressText.text = $"{_currentStirs}/{_targetStirs}";
    }
}
