using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCutscene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject wizardTextBubble;
    [SerializeField] private StreamingDialogue anchorTextStreamer;
    [SerializeField] private StreamingDialogue wizardTextStreamer;

    [Header("Panels")]
    [SerializeField] private GameObject tvPanel;   // Set active at start of cutscene
    [SerializeField] private GameObject anchorSpeechBubble; // NEW: anchor’s “talking” bubble

    [Header("Anchor TV Image (for old-TV turn-on effect)")]
    [Tooltip("RectTransform of the TV/anchor image that should vertically expand like an old TV turn-on.")]
    [SerializeField] private RectTransform anchorImageRect;
    [Tooltip("Optional: Image component to briefly fade/flash during TV turn-on.")]
    [SerializeField] private Image anchorImage;

    [Header("Dialogue Lines")]
    [TextArea]
    [SerializeField]
    private string anchorLine =
        "It’s Halloween night! Kids in costumes are out trick-or-treating — share the sweetness!";
    [TextArea]
    [SerializeField]
    private string wizardLine =
        "Guess I should get the door.";

    [Header("Timings")]
    [SerializeField] private float preKnockDelay = 0.05f;     // wait after anchor finishes typing
    [SerializeField] private float afterKnockDelay = 0.5f;    // wait after knock before wizard speaks
    [SerializeField] private float afterWizardDelay = 0.5f;   // buffer before switching to Arrival

    [Header("TV Turn-On Effect")]
    [Tooltip("How long the vertical expansion takes.")]
    [SerializeField] private float tvOnDuration = 0.32f;
    [Tooltip("Small overshoot factor for a snappy settle (e.g., 1.05 = 5% overshoot).")]
    [SerializeField] private float tvOvershoot = 1.06f;
    [Tooltip("Optional quick flash at the beginning (seconds). Set 0 for none.")]
    [SerializeField] private float tvFlashTime = 0.06f;
    [Tooltip("Alpha during flash (0..1).")]
    [SerializeField] private float tvFlashAlpha = 0.9f;

    public void PlayCutscene()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // 1) Show TV panel and play subtle “news start” sound
        if (tvPanel) tvPanel.SetActive(true);
        SoundManager.Instance.PlaySfx(Sfx.NewsStart, false, 0.1f);

        // 2) Run old-TV turn-on effect on the anchor image BEFORE the anchor speaks
        yield return StartCoroutine(TvTurnOnEffect());

        // 3) Once TV is on, reveal anchor’s bubble + play a short “popup” sound
        if (anchorSpeechBubble)
        {
            anchorSpeechBubble.SetActive(true);
            SoundManager.Instance.PlaySfx(Sfx.StepSuccess, false, 0.3f);
        }

        // 4) Anchor speaks and we WAIT until the line finishes TYPING
        anchorTextStreamer.PlayLine(anchorLine);
        yield return StartCoroutine(WaitForTypeComplete(anchorTextStreamer));

        // 5) Wait pre-knock delay
        yield return new WaitForSeconds(preKnockDelay);

        // 6) Knock SFX
        SoundManager.Instance.PlaySfx(Sfx.Knock);
        yield return new WaitForSeconds(afterKnockDelay);

        // 7) Wizard speaks (show bubble, then stream) and WAIT until typing is done
        if (wizardTextBubble) wizardTextBubble.SetActive(true);
        wizardTextStreamer.PlayLine(wizardLine);
        yield return StartCoroutine(WaitForTypeComplete(wizardTextStreamer));

        // 8) Buffer, then transition
        yield return new WaitForSeconds(afterWizardDelay);

        SoundManager.Instance.PlayBGM(Bgm.Title, true, 0.5f);
        GameManager.Instance.ChangeGameState(LoopState.Arrival);
    }

    /// <summary>
    /// Old-TV vertical expansion: start as a thin horizontal line, expand to full height with a tiny overshoot, then settle.
    /// </summary>
    private IEnumerator TvTurnOnEffect()
    {
        if (anchorImageRect == null)
            yield break;

        Vector2 originalPivot = anchorImageRect.pivot;
        anchorImageRect.pivot = new Vector2(0.5f, 0.5f);

        Vector3 originalScale = anchorImageRect.localScale;
        anchorImageRect.localScale = new Vector3(1f, 0.02f, 1f);

        Color? originalColor = null;
        if (anchorImage != null && tvFlashTime > 0f)
        {
            originalColor = anchorImage.color;
            var c = anchorImage.color;
            c.a = Mathf.Clamp01(tvFlashAlpha);
            anchorImage.color = c;
        }

        // --- Expansion Phase ---
        float t = 0f;
        float overshootTarget = Mathf.Max(1f, tvOvershoot);
        while (t < tvOnDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / tvOnDuration);
            float eased = EaseOutBack(p, 1.70158f);
            float y = Mathf.Lerp(0.02f, overshootTarget, eased);
            anchorImageRect.localScale = new Vector3(1f, y, 1f);
            yield return null;
        }

        // --- Settle Phase ---
        float settleTime = tvOnDuration * 0.25f;
        t = 0f;
        float startY = anchorImageRect.localScale.y;
        while (t < settleTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleTime);
            float eased = EaseOutCubic(p);
            float y = Mathf.Lerp(startY, 1f, eased);
            anchorImageRect.localScale = new Vector3(1f, y, 1f);
            yield return null;
        }
        anchorImageRect.localScale = new Vector3(1f, 1f, 1f);

        // --- Fade back from flash ---
        if (anchorImage != null && originalColor.HasValue)
        {
            t = 0f;
            float fadeTime = Mathf.Max(0.03f, tvFlashTime * 0.6f);
            Color start = anchorImage.color;
            Color end = originalColor.Value;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeTime);
                anchorImage.color = Color.Lerp(start, end, p);
                yield return null;
            }
            anchorImage.color = end;
        }

        anchorImageRect.pivot = originalPivot;
    }

    private IEnumerator WaitForTypeComplete(StreamingDialogue streamer)
    {
        bool done = false;
        System.Action handler = () => done = true;

        streamer.OnTypeComplete += handler;
        while (!done) yield return null;
        streamer.OnTypeComplete -= handler;
    }

    // --- Easing helpers ---
    private float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    private float EaseOutBack(float x, float s)
    {
        float inv = x - 1f;
        return 1f + (inv * inv * ((s + 1f) * inv + s));
    }
}
