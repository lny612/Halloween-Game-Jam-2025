using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Candy-making boiling minigame (Stardew-style).
/// - Hold the key to heat the cauldron; release to cool (gravity-like fall).
/// - Keep the heat marker inside the moving ideal temperature zone.
/// - Stay inside to build candy quality; leave it and quality drains.
/// - Win when quality reaches 1.
/// </summary>
public class CauldronBoilMinigame : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Vertical lane representing the cauldron temperature scale.")]
    public RectTransform temperatureLane;

    [Tooltip("Rect for the player’s heat marker (moves up/down).")]
    public RectTransform heatMarker;

    [Tooltip("Rect for the ideal temperature zone (moves unpredictably).")]
    public RectTransform idealZone;

    [Tooltip("Slider or Image fill for candy quality progress (0..1).")]
    public Slider qualityMeter;

    [Header("Heat Marker (feel)")]
    public float markerHeight = 120f;
    public float heatRiseAcceleration = 1400f;
    public float coolingGravity = 1200f;
    public float maxTemperatureVelocity = 900f;
    [Range(0f, 1f)] public float velocityDamping = 0.15f;

    [Header("Ideal Zone (difficulty)")]
    [Tooltip("How reactive the zone is (higher = moves faster).")]
    public float zoneVolatility = 0.9f;
    [Range(0f, 1f)] public float zoneRangeFraction = 0.55f;
    public Vector2 zoneRetargetInterval = new Vector2(0.6f, 1.8f);
    public float zoneHeight = 70f;

    [Header("Quality Scoring")]
    public float qualityGainRate = 0.45f;
    public float qualityLossRate = 0.75f;
    public float timeLimit = 0f;

    [Header("Input")]
    public KeyCode heatKey = KeyCode.Space;
    public bool useMouse0 = true;

    [Header("Events")]
    public UnityEvent onCandyPerfect;
    public UnityEvent onCandyBurned;

    // Internal state
    float laneHeight;
    float markerY;
    float markerVelocity;
    float zoneY;
    float zoneTargetY;
    float retargetTimer;
    float retargetCooldown;
    float quality;
    float elapsed;
    bool boiling;

    RectTransform _markerRT;
    RectTransform _zoneRT;

    void Awake()
    {
        _markerRT = heatMarker;
        _zoneRT = idealZone;
    }

    void OnEnable() => StartBoiling();

    public void StartBoiling()
    {
        laneHeight = temperatureLane.rect.height;
        markerY = laneHeight * 0.15f;
        markerVelocity = 0f;
        zoneY = laneHeight * Random.Range(0.3f, 0.7f);
        zoneTargetY = zoneY;

        quality = 0f;
        elapsed = 0f;
        qualityMeter?.SetValueWithoutNotify(0f);

        ScheduleNextZoneShift();
        UpdateVisualsImmediate();
        boiling = true;
    }

    public void StopBoiling() => boiling = false;

    void Update()
    {
        if (!boiling) return;

        float dt = Time.deltaTime;
        elapsed += dt;

        // 1) Heat control
        bool heating = (Input.GetKey(heatKey)) || (useMouse0 && Input.GetMouseButton(0));
        float accel = heating ? heatRiseAcceleration : -coolingGravity;
        markerVelocity += accel * dt;
        markerVelocity = Mathf.Lerp(markerVelocity,
            Mathf.Clamp(markerVelocity, -maxTemperatureVelocity, maxTemperatureVelocity),
            1f - velocityDamping);

        markerY += markerVelocity * dt;

        float halfMarker = markerHeight * 0.5f;
        markerY = Mathf.Clamp(markerY, halfMarker, laneHeight - halfMarker);
        if (markerY == halfMarker && markerVelocity < 0) markerVelocity = 0;
        if (markerY == laneHeight - halfMarker && markerVelocity > 0) markerVelocity = 0;

        // 2) Ideal zone wandering
        retargetTimer += dt;
        if (retargetTimer >= retargetCooldown) ScheduleNextZoneShift();

        float lerpSpeed = Mathf.Lerp(2f, 7.5f, zoneVolatility);
        zoneY = Mathf.Lerp(zoneY, zoneTargetY, 1f - Mathf.Exp(-lerpSpeed * dt));

        float halfZone = zoneHeight * 0.5f;
        zoneY = Mathf.Clamp(zoneY, halfZone, laneHeight - halfZone);

        // 3) Quality accumulation
        bool inIdealRange = CheckOverlap(markerY, markerHeight, zoneY, zoneHeight);
        float rate = inIdealRange ? qualityGainRate : -qualityLossRate;
        quality = Mathf.Clamp01(quality + rate * dt);
        if (qualityMeter != null) qualityMeter.value = quality;

        // 4) End conditions
        if (quality >= 1f)
        {
            boiling = false;
            onCandyPerfect?.Invoke();
        }
        else if (timeLimit > 0f && elapsed >= timeLimit)
        {
            boiling = false;
            onCandyBurned?.Invoke();
        }

        // 5) Visual update
        UpdateVisualsImmediate();
    }

    void ScheduleNextZoneShift()
    {
        retargetTimer = 0f;
        retargetCooldown = Random.Range(zoneRetargetInterval.x, zoneRetargetInterval.y);

        float mid = laneHeight * 0.5f;
        float amp = laneHeight * zoneRangeFraction * 0.5f;
        float spike = Random.Range(-amp, amp);
        zoneTargetY = Mathf.Clamp(mid + spike, zoneHeight * 0.5f, laneHeight - zoneHeight * 0.5f);
    }

    bool CheckOverlap(float markerCenter, float markerSize, float zoneCenter, float zoneSize)
    {
        float mMin = markerCenter - markerSize * 0.5f;
        float mMax = markerCenter + markerSize * 0.5f;
        float zMin = zoneCenter - zoneSize * 0.5f;
        float zMax = zoneCenter + zoneSize * 0.5f;
        return !(mMax < zMin || mMin > zMax);
    }

    void UpdateVisualsImmediate()
    {
        if (_markerRT != null)
        {
            Vector2 p = _markerRT.anchoredPosition;
            p.y = markerY - (laneHeight * 0.5f);
            _markerRT.anchoredPosition = p;

            Vector2 sz = _markerRT.sizeDelta;
            sz.y = markerHeight;
            _markerRT.sizeDelta = sz;
        }

        if (_zoneRT != null)
        {
            Vector2 z = _zoneRT.anchoredPosition;
            z.y = zoneY - (laneHeight * 0.5f);
            _zoneRT.anchoredPosition = z;

            Vector2 zs = _zoneRT.sizeDelta;
            zs.y = zoneHeight;
            _zoneRT.sizeDelta = zs;
        }
    }

    public void SetDifficulty(float volatility01, float range01)
    {
        zoneVolatility = Mathf.Clamp01(volatility01);
        zoneRangeFraction = Mathf.Clamp01(range01);
    }

    public void ForceFail()
    {
        if (!boiling) return;
        boiling = false;
        onCandyBurned?.Invoke();
    }
}
