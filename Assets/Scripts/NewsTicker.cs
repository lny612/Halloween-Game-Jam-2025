using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple TV-style headline ticker:
/// - Scrolls text to the left at a constant speed
/// - Seamless loop using a duplicate of the same text
/// - Works with TextMeshProUGUI inside a masked viewport
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class NewsTicker : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The masked viewport (e.g., an Image with Mask or RectMask2D).")]
    [SerializeField] private RectTransform viewport;

    [Tooltip("The template TextMeshProUGUI. Its text/font/style will be used. Place it as a child of 'content'.")]
    [SerializeField] private TextMeshProUGUI templateText;

    [Tooltip("Parent container that holds the two text clones.")]
    [SerializeField] private RectTransform content;

    [Header("Scroll")]
    [Tooltip("Pixels per second.")]
    [SerializeField] private float speed = 80f;

    [Tooltip("Gap between the end of the first headline and the start of the second (pixels).")]
    [SerializeField] private float spacing = 60f;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    private TextMeshProUGUI _a, _b;
    private RectTransform _rtA, _rtB;
    private float _segmentWidth; // width of one text segment + spacing
    private bool _built;

    private void Awake()
    {
        if (!viewport) viewport = GetComponent<RectTransform>();
        if (!content) content = viewport;
    }

    private void OnEnable()
    {
        BuildIfNeeded();
    }

    private void Update()
    {
        if (!_built || _segmentWidth <= 1f) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float dx = speed * dt;

        // Move both segments left
        ShiftLeft(_rtA, dx);
        ShiftLeft(_rtB, dx);

        // If a segment has fully passed the left boundary, wrap it to the right of the other
        WrapIfNeeded(_rtA, _rtB);
        WrapIfNeeded(_rtB, _rtA);
    }

    /// <summary>
    /// Changes the headline at runtime (rebuilds widths/positions).
    /// </summary>
    public void SetHeadline(string text)
    {
        if (!templateText) return;

        templateText.text = text;
        _built = false;
        BuildIfNeeded();
    }

    // ---------- internals ----------

    private void BuildIfNeeded()
    {
        if (!templateText) { Debug.LogWarning("[NewsTicker] Template Text is not set."); return; }

        // Destroy old clones if rebuilding
        if (_a) DestroyImmediate(_a.gameObject);
        if (_b) DestroyImmediate(_b.gameObject);

        // Create first clone (A)
        _a = Instantiate(templateText, content);
        _a.gameObject.name = "Ticker_A";
        _a.raycastTarget = false;

        // Create second clone (B)
        _b = Instantiate(templateText, content);
        _b.gameObject.name = "Ticker_B";
        _b.raycastTarget = false;

        _rtA = _a.rectTransform;
        _rtB = _b.rectTransform;

        // Force layout update so preferredWidth is accurate
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rtA);

        // Calculate segment width (text width + spacing)
        float textWidth = Mathf.Ceil(_a.preferredWidth);
        if (textWidth < 1f) textWidth = 1f; // safety
        _segmentWidth = textWidth + spacing;

        // Size texts to their preferred width so positions are exact
        Vector2 sizeA = _rtA.sizeDelta;
        sizeA.x = textWidth;
        _rtA.sizeDelta = sizeA;

        Vector2 sizeB = _rtB.sizeDelta;
        sizeB.x = textWidth;
        _rtB.sizeDelta = sizeB;

        // Anchor/pivot center-left for easier positioning
        _rtA.anchorMin = _rtA.anchorMax = new Vector2(0f, 0.5f);
        _rtA.pivot = new Vector2(0f, 0.5f);
        _rtB.anchorMin = _rtB.anchorMax = new Vector2(0f, 0.5f);
        _rtB.pivot = new Vector2(0f, 0.5f);

        // Place A at x=0, B right after A with spacing
        _rtA.anchoredPosition = new Vector2(0f, 0f);
        _rtB.anchoredPosition = new Vector2(_segmentWidth, 0f);

        _built = true;
    }

    private void ShiftLeft(RectTransform rt, float dx)
    {
        var p = rt.anchoredPosition;
        p.x -= dx;
        rt.anchoredPosition = p;
    }

    private void WrapIfNeeded(RectTransform rt, RectTransform other)
    {
        // When this segment has fully moved past the left edge beyond its own width,
        // push it to the right edge of the other segment.
        if (rt.anchoredPosition.x <= -_segmentWidth)
        {
            rt.anchoredPosition = new Vector2(other.anchoredPosition.x + _segmentWidth, rt.anchoredPosition.y);
        }
    }
}
