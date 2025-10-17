// SpiritSightController.cs
// Toggle magical "inspect vision" with URP Volume blending, UI overlay, audio low-pass, and timescale dip.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpiritSightController : MonoBehaviour
{
    [Header("Post-Processing (URP)")]
    [SerializeField] private Volume globalVolume;          // Global Volume with your Spirit profile
    [SerializeField] private float targetWeight = 0.45f;      // How intense the look gets
    [SerializeField] private float blendDuration = 0.6f;   // Fade in/out time (seconds)

    [Header("UI Overlay")]
    [SerializeField] private CanvasGroup spiritOverlay;     // Fullscreen runes/nebula overlay (alpha fades)
    [SerializeField] private float overlayMaxAlpha = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioLowPassFilter lowPass;    // On main audio source or listener
    [SerializeField] private int lowPassCutoff = 1000;      // Hz when enabled

    [Header("Time Warp")]
    [SerializeField] private bool dipTimeScale = true;
    [SerializeField] private float dippedScale = 0.95f;

    [Header("Extras")]
    [SerializeField]
    private AnimationCurve ease =          // Nice ease in/out
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float _baseTimeScale = 1f;
    private Coroutine _fxRoutine;
    private bool _active;

    void Reset()
    {
        globalVolume = FindAnyObjectByType<Volume>();
        lowPass = FindAnyObjectByType<AudioLowPassFilter>();
        spiritOverlay = FindAnyObjectByType<CanvasGroup>();
    }

    void Awake()
    {
        if (globalVolume) globalVolume.weight = 0f;
        if (spiritOverlay) spiritOverlay.alpha = 0f;
        if (lowPass) lowPass.enabled = false;
        _baseTimeScale = Time.timeScale;
    }

    // Hook these to your Inspect button/events:
    public void EnterSpiritSight()
    {
        if (_active) return;
        _active = true;
        StartBlend(entering: true);
    }

    public void ExitSpiritSight()
    {
        if (!_active) return;
        _active = false;
        StartBlend(entering: false);
    }

    public void ToggleSpiritSight()
    {
        if (_active) ExitSpiritSight();
        else EnterSpiritSight();
    }

    private void StartBlend(bool entering)
    {
        if (_fxRoutine != null) StopCoroutine(_fxRoutine);
        _fxRoutine = StartCoroutine(BlendRoutine(entering));
    }

    private IEnumerator BlendRoutine(bool entering)
    {
        float startW = globalVolume ? globalVolume.weight : 0f;
        float endW = entering ? targetWeight : 0f;

        float startA = spiritOverlay ? spiritOverlay.alpha : 0f;
        float endA = entering ? overlayMaxAlpha : 0f;

        // Prep audio/time on enter
        if (entering)
        {
            if (lowPass)
            {
                lowPass.enabled = true;
                lowPass.cutoffFrequency = lowPassCutoff;
            }
            if (dipTimeScale)
                Time.timeScale = dippedScale;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, blendDuration);
            float k = ease.Evaluate(Mathf.Clamp01(t));

            if (globalVolume) globalVolume.weight = Mathf.Lerp(startW, endW, k);
            if (spiritOverlay) spiritOverlay.alpha = Mathf.Lerp(startA, endA, k);

            yield return null;
        }

        // Cleanup on exit
        if (!entering)
        {
            if (globalVolume) globalVolume.weight = 0f;
            if (spiritOverlay) spiritOverlay.alpha = 0f;
            if (lowPass) lowPass.enabled = false;
            if (dipTimeScale) Time.timeScale = _baseTimeScale;
        }
    }

    public void ForceDisableSpiritSight()
    {
        // stop any ongoing blend
        if (_fxRoutine != null)
        {
            StopCoroutine(_fxRoutine);
            _fxRoutine = null;
        }

        // immediately reset all effects
        if (globalVolume) globalVolume.weight = 0f;
        if (spiritOverlay) spiritOverlay.alpha = 0f;
        if (lowPass) lowPass.enabled = false;
        if (dipTimeScale) Time.timeScale = _baseTimeScale;

        _active = false;
    }
}
