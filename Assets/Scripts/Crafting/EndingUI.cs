using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [Header("TV + UI")]
    public Image AnchorImage;                                   // The TV/anchor image (sprite shown in the TV)
    [SerializeField] private RectTransform anchorImageRect;     // For TV turn-on effect (auto-assigned from AnchorImage if left null)
    [SerializeField] private GameObject anchorSpeechBubble;     // speech bubble (for endingResultText)
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI endingResultText;
    public TextMeshProUGUI commentText;
    public GameObject commentBubble;
    public GameObject thankYouText;

    [Header("Data")]
    public EndingDataContainer endingDataContainer;
    private List<EndingScripts> endingScriptsList = new List<EndingScripts>();

    [Header("Marks")]
    public GameObject goodMark;
    public GameObject badMark;

    [Header("Controls")]
    public Button nextButton;
    public GameObject helperArrowImage;

    [Header("Wrong Ending Defaults")]
    [TextArea] public string defaultHeadline;
    [TextArea] public string defaultEndingText;
    [TextArea] public string defaultComment = "It seems like I failed to grant the {0} child’s wish.";
    public Sprite defaultImage;

    [Header("Intro Panel")]
    [SerializeField] private GameObject fewHoursLaterPanel;  // shows for 3 seconds at very start
    [SerializeField] private float fewHoursDuration = 3f;    // duration to show the panel

    [Header("TV Turn-On Effect")]
    [Tooltip("How long the vertical expansion takes.")]
    [SerializeField] private float tvOnDuration = 0.32f;
    [Tooltip("Small overshoot factor for a snappy settle (e.g., 1.05 = 5% overshoot).")]
    [SerializeField] private float tvOvershoot = 1.06f;
    [Tooltip("Optional quick flash at the beginning (seconds). Set 0 for none).")]
    [SerializeField] private float tvFlashTime = 0.06f;
    [Tooltip("Alpha during flash (0..1). Only used if flash time > 0.")]
    [SerializeField] private float tvFlashAlpha = 0.9f;

    private int _endingIndex = 0;
    private int _numberOfEndings;
    private List<CraftResult> _craftResults = new List<CraftResult>();

    // Internal state for TV effect
    private bool _tvIsOn = false;
    private Vector3 _tvOriginalScale = Vector3.one;

    public void Awake()
    {
        nextButton.onClick.AddListener(OnNextPressed);
    }

    public void InitializeEndingUI()
    {
        // Ensure rect assigned
        if (AnchorImage != null && anchorImageRect == null)
            anchorImageRect = AnchorImage.rectTransform;

        // Reset early UI state
        if (thankYouText) thankYouText.SetActive(false);   // NEW: hide thank-you at start
        if (commentBubble) commentBubble.SetActive(false);
        if (anchorSpeechBubble) anchorSpeechBubble.SetActive(false);
        if (goodMark) goodMark.SetActive(false);
        if (badMark) badMark.SetActive(false);
        helperArrowImage?.SetActive(false);
        nextButton.interactable = false;

        _craftResults = GameManager.Instance.GetCraftResults();
        _numberOfEndings = _craftResults.Count;
        _endingIndex = 0;

        StopAllCoroutines();
        StartCoroutine(InitializeSequence());
    }

    private IEnumerator InitializeSequence()
    {
        // 1) Show “Few Hours Later” panel for N seconds, then off
        if (fewHoursLaterPanel)
        {
            fewHoursLaterPanel.SetActive(true);
            yield return new WaitForSeconds(fewHoursDuration);
            fewHoursLaterPanel.SetActive(false);
        }

        // 2) Ensure TV is ON with the old-TV effect (only first time)
        if (!_tvIsOn)
            yield return StartCoroutine(TvTurnOnEffect());

        // 3) After TV fully on, show anchor speech bubble (kept on across pages)
        if (anchorSpeechBubble) anchorSpeechBubble.SetActive(true);

        // 4) Start first ending, or disable next if none
        if (_numberOfEndings > 0)
        {
            PlayEnding(_craftResults[_endingIndex]);
        }
        else
        {
            nextButton.interactable = false;
        }
    }

    public void PlayEnding(CraftResult result)
    {
        // Hide per-ending visuals that should wait on typing
        if (commentBubble) commentBubble.SetActive(false);
        if (goodMark) goodMark.SetActive(false);
        if (badMark) badMark.SetActive(false);
        helperArrowImage?.SetActive(false);
        nextButton.interactable = false;

        EndingScripts endingScripts = GetWantedCandyEnding(result.candyName);

        if (result.isMatching)
        {
            if (result.candyGrade == CandyGrade.Divine || result.candyGrade == CandyGrade.Deluxe)
            {
                SetEndingUI(
                    endingScripts.correctHeadline,
                    endingScripts.correctEndingText,
                    endingScripts.correctComment,
                    endingScripts.correctImage,
                    true
                );
            }
            else
            {
                SetEndingUI(
                    endingScripts.wrongHeadline,
                    endingScripts.wrongEndingText,
                    endingScripts.wrongComment,
                    endingScripts.wrongImage,
                    false
                );
            }
        }
        else
        {
            string ordinal = GetOrdinalName(_endingIndex + 1);
            string dynamicComment = string.Format(defaultComment, ordinal);

            SetEndingUI(
                defaultHeadline,
                defaultEndingText,
                dynamicComment,
                defaultImage,
                false
            );
        }

        _endingIndex++; // next page will be this index
    }

    public EndingScripts GetWantedCandyEnding(CandyName searchingCandyName)
    {
        foreach (var ending in endingDataContainer.endingList)
        {
            if (ending.candyName == searchingCandyName)
            {
                return ending;
            }
        }
        return null;
    }

    public void SetEndingUI(string headline, string endingResult, string comment, Sprite image, bool isTrueEnding)
    {
        // Headline and image set immediately
        headlineText.text = headline;

        AnchorImage.sprite = image;

        // Marks shown only after comment fully streams
        if (goodMark) goodMark.SetActive(false);
        if (badMark) badMark.SetActive(false);

        // TV must remain ON; speech bubble must remain ON
        if (!_tvIsOn)
        {
            // Safety: if somehow called early, turn it on now
            StopAllCoroutines();
            StartCoroutine(CoEnsureTvThenRun(endingResult, comment, isTrueEnding));
        }
        else
        {
            // Run the typing sequence: result → (then) comment → (then) marks & sound
            StopAllCoroutines();
            StartCoroutine(CoStreamEndingSequence(endingResult, comment, isTrueEnding));
        }
    }

    private IEnumerator CoEnsureTvThenRun(string endingResult, string comment, bool isTrueEnding)
    {
        yield return StartCoroutine(TvTurnOnEffect());
        if (anchorSpeechBubble) anchorSpeechBubble.SetActive(true);
        yield return StartCoroutine(CoStreamEndingSequence(endingResult, comment, isTrueEnding));
    }

    private IEnumerator CoStreamEndingSequence(string endingResult, string comment, bool isTrueEnding)
    {
        var resultStreamer = endingResultText.GetComponent<StreamingDialogue>();
        var commentStreamer = commentText.GetComponent<StreamingDialogue>();

        // 1) Stream the ending result line and wait for typing to finish
        resultStreamer.PlayLine(endingResult);
        yield return StartCoroutine(WaitForTypeComplete(resultStreamer));

        // 2) Only now show the comment bubble and type the comment
        if (commentBubble) commentBubble.SetActive(true);
        commentStreamer.PlayLine(comment);
        yield return StartCoroutine(WaitForTypeComplete(commentStreamer));

        // 3) After comment finishes, show the correct mark and play sound
        if (isTrueEnding)
        {
            if (goodMark) goodMark.SetActive(true);
            if (badMark) badMark.SetActive(false);
            SoundManager.Instance.PlaySfx(Sfx.StepSuccess);
        }
        else
        {
            if (goodMark) goodMark.SetActive(false);
            if (badMark) badMark.SetActive(true);
            SoundManager.Instance.PlaySfx(Sfx.StepFail);
        }

        // 4) Enable Next: keep it enabled even on the last page so the player can reach "thank you"
        if (_endingIndex < _numberOfEndings)
        {
            helperArrowImage?.SetActive(true);
            nextButton.interactable = true;
        }
        else
        {
            helperArrowImage?.SetActive(false);
            nextButton.interactable = true;   // <-- keep enabled on the last page
        }
    }

    // Waits until StreamingDialogue finishes typing the current line (no user advance needed)
    private IEnumerator WaitForTypeComplete(StreamingDialogue streamer)
    {
        bool done = false;
        System.Action handler = () => done = true;
        streamer.OnTypeComplete += handler;
        while (!done) yield return null;
        streamer.OnTypeComplete -= handler;
    }

    public void OnNextPressed()
    {
        if (_endingIndex == _numberOfEndings)
        {
            // All endings shown → show thank you screen
            if (anchorSpeechBubble) anchorSpeechBubble.SetActive(false);
            if (commentBubble) commentBubble.SetActive(false);
            if (helperArrowImage) helperArrowImage.SetActive(false);
            if (AnchorImage) AnchorImage.gameObject.SetActive(false);
            if (goodMark) goodMark.SetActive(false);
            if (badMark) badMark.SetActive(false);
            if (thankYouText) thankYouText.SetActive(true);
            return;
        }

        // Otherwise, continue to next ending
        SoundManager.Instance.PlaySfx(Sfx.RemoteControl);
        helperArrowImage?.SetActive(false);

        // TV must remain ON, speech bubble remains ON
        if (!_tvIsOn)
        {
            // Safety: ensure it's on
            StartCoroutine(TvTurnOnEffect());
        }
        if (anchorSpeechBubble) anchorSpeechBubble.SetActive(true);

        // Comment bubble should turn OFF until result finishes streaming again
        if (commentBubble) commentBubble.SetActive(false);

        if (_endingIndex < _numberOfEndings)
        {
            PlayEnding(_craftResults[_endingIndex]);
        }
        else
        {
            nextButton.interactable = false;
        }
    }

    // Converts 1 -> "first", 2 -> "second", 3 -> "third", else "Nth"
    private string GetOrdinalName(int number)
    {
        switch (number)
        {
            case 1: return "first";
            case 2: return "second";
            case 3: return "third";
            default: return number + "th";
        }
    }

    // -----------------------------
    // TV old-style turn-on effect
    // -----------------------------
    private IEnumerator TvTurnOnEffect()
    {
        if (anchorImageRect == null)
        {
            _tvIsOn = true; // nothing to animate
            yield break;
        }

        if (!_tvIsOn)
        {
            _tvOriginalScale = anchorImageRect.localScale;

            // Ensure centered pivot for the expansion
            Vector2 originalPivot = anchorImageRect.pivot;
            anchorImageRect.pivot = new Vector2(0.5f, 0.5f);

            // Start as a thin horizontal line
            anchorImageRect.localScale = new Vector3(_tvOriginalScale.x, 0.02f, _tvOriginalScale.z);

            // Optional small flash (no news sound here)
            Color? originalColor = null;
            if (AnchorImage != null && tvFlashTime > 0f)
            {
                originalColor = AnchorImage.color;
                var c = AnchorImage.color;
                c.a = Mathf.Clamp01(tvFlashAlpha);
                AnchorImage.color = c;
            }

            // Expand with overshoot
            float t = 0f;
            float overshootTarget = Mathf.Max(1f, tvOvershoot);
            while (t < tvOnDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / tvOnDuration);
                float eased = EaseOutBack(p, 1.70158f);
                float y = Mathf.Lerp(0.02f, overshootTarget, eased);
                anchorImageRect.localScale = new Vector3(_tvOriginalScale.x, y, _tvOriginalScale.z);
                yield return null;
            }

            // Settle to exact scale 1.0 in Y (relative to original X/Z)
            float settleTime = tvOnDuration * 0.25f;
            t = 0f;
            float startY = anchorImageRect.localScale.y;
            while (t < settleTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / settleTime);
                float eased = EaseOutCubic(p);
                float y = Mathf.Lerp(startY, 1f, eased);
                anchorImageRect.localScale = new Vector3(_tvOriginalScale.x, y, _tvOriginalScale.z);
                yield return null;
            }
            anchorImageRect.localScale = new Vector3(_tvOriginalScale.x, 1f, _tvOriginalScale.z);

            // Fade back from flash
            if (AnchorImage != null && tvFlashTime > 0f)
            {
                t = 0f;
                float fadeTime = Mathf.Max(0.03f, tvFlashTime * 0.6f);
                Color start = AnchorImage.color;
                Color end = originalColor ?? AnchorImage.color;
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    float p = Mathf.Clamp01(t / fadeTime);
                    AnchorImage.color = Color.Lerp(start, end, p);
                    yield return null;
                }
                AnchorImage.color = end;
            }

            // Restore pivot (optional)
            anchorImageRect.pivot = originalPivot;

            _tvIsOn = true;
        }
    }

    // Easing
    private float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    private float EaseOutBack(float x, float s)
    {
        float inv = x - 1f;
        return 1f + (inv * inv * ((s + 1f) * inv + s));
    }
}
