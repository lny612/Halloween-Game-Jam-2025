// StartBannerController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartBannerController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Parent panel (full-screen) on a Screen Space - Overlay or Camera Canvas")]
    public RectTransform bannerPanel;

    [Tooltip("Prefab with TextMeshProUGUI for a single letter")]
    public TextMeshProUGUI letterPrefab;

    [Tooltip("Horizontal spacing between letters in pixels")]
    public float letterSpacing = 80f;

    [Header("Motion")]
    [Tooltip("Upward speed in pixels/second for the flying letters")]
    public float flyUpSpeed = 1600f;
    public float initialDelay = 0.25f;

    [Tooltip("Delay (sec) between each letter's launch")]
    public float delayBetweenLetters = 0.08f;

    [Tooltip("How far offscreen (pixels) past the top before destroying")]
    public float offscreenMargin = 200f;

    private readonly char[] _letters = new[] { 'S', 'T', 'A', 'R', 'T' };
    private readonly List<RectTransform> _spawned = new();

    /// <summary>
    /// Plays the START banner once. Calls onComplete after the last letter is destroyed.
    /// </summary>
    public void Play(Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(PlayRoutine(onComplete));
    }

    private IEnumerator PlayRoutine(Action onComplete)
    {
        // Freeze everything else by just… doing nothing here. Your CraftingManager won’t start until we call onComplete.

        // SFX (use your actual SoundManager signature / enum casing)
        // Example, matching your other calls: SoundManager.Instance.PlaySfx(Sfx.StartCrafting);
        SoundManager.Instance.PlaySfx(Sfx.StartCraft);

        SpawnLettersCentered();
        yield return new WaitForSeconds(initialDelay);
        // Launch letters one-by-one upward and destroy
        for (int i = 0; i < _spawned.Count; i++)
        {
            yield return StartCoroutine(FlyUpAndDestroy(_spawned[i]));
            yield return new WaitForSeconds(delayBetweenLetters);
        }

        // Cleanup + hide panel
        _spawned.Clear();
        gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    private void SpawnLettersCentered()
    {
        // Clear old (if any)
        foreach (Transform c in bannerPanel) Destroy(c.gameObject);
        _spawned.Clear();

        float totalWidth = (_letters.Length - 1) * letterSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < _letters.Length; i++)
        {
            var letter = Instantiate(letterPrefab, bannerPanel);
            letter.text = _letters[i].ToString();
            letter.alpha = 1f;

            var rt = (RectTransform)letter.transform;
            rt.anchoredPosition = new Vector2(startX + i * letterSpacing, 0f);
            _spawned.Add(rt);
        }
    }

    private IEnumerator FlyUpAndDestroy(RectTransform rt)
    {

        float canvasHalfHeight = ((RectTransform)bannerPanel).rect.height * 0.5f;
        float targetY = canvasHalfHeight + offscreenMargin;

        // small pop: you can uncomment if you want
        // rt.localScale = Vector3.one * 1.2f;
        // yield return new WaitForSeconds(0.03f);

        while (rt.anchoredPosition.y < targetY)
        {
            var p = rt.anchoredPosition;
            p.y += flyUpSpeed * Time.deltaTime;
            rt.anchoredPosition = p;
            yield return null;
        }

        Destroy(rt.gameObject);
    }
}
