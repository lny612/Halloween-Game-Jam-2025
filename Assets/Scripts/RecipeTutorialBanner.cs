using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecipeTutorialBanner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The UI element (e.g., Image or panel) that moves.")]
    public RectTransform bannerRect;

    [Header("Animation Settings")]
    [Tooltip("How far down the banner moves (in anchored Y units).")]
    public float dropDistance = 300f;
    [Tooltip("How long it takes to drop down.")]
    public float dropDuration = 0.3f;
    [Tooltip("How long the banner stays visible before going up.")]
    public float holdDuration = 3f;
    [Tooltip("How long it takes to slide back up.")]
    public float retractDuration = 0.3f;

    private Vector2 _startPos;
    private Vector2 _targetPos;
    private Coroutine _routine;

    void Awake()
    {
        if (bannerRect == null)
            bannerRect = GetComponent<RectTransform>();

        _startPos = bannerRect.anchoredPosition;
        _targetPos = _startPos - new Vector2(0, dropDistance);

        // Ensure it's hidden off-screen at start
        bannerRect.anchoredPosition = _startPos;
    }

    public void PlayBanner()
    {
        this.gameObject.SetActive(true);
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(BannerRoutine());
    }

    private IEnumerator BannerRoutine()
    {
        // Drop down
        yield return StartCoroutine(AnimatePosition(_startPos, _targetPos, dropDuration));

        // Stay visible
        yield return new WaitForSeconds(holdDuration);

        // Go back up
        yield return StartCoroutine(AnimatePosition(_targetPos, _startPos, retractDuration));

        _routine = null;
    }

    private IEnumerator AnimatePosition(Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            bannerRect.anchoredPosition = Vector2.Lerp(from, to, EaseOutCubic(p));
            yield return null;
        }
        bannerRect.anchoredPosition = to;
    }

    // Smooth easing for more natural movement
    private float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }
}
