// StreamingDialogue_NoCursor_Unskippable_SingleLine.cs
// Streams a single line of dialogue with timing, punctuation pauses, and optional sound.
// No skipping, no advancing — just type once and stop.

using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class StreamingDialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textUI;

    [Header("Speed")]
    [Tooltip("Base speed in characters per second.")]
    [SerializeField] private float charsPerSecond = 40f;
    [Tooltip("While this key is held, streaming is faster.")]
    [SerializeField] private KeyCode holdToSpeedUpKey = KeyCode.LeftShift;
    [SerializeField] private float speedUpMultiplier = 2.5f;

    [Header("Punctuation Pauses (seconds)")]
    [SerializeField] private float commaPause = 0.08f;
    [SerializeField] private float periodPause = 0.18f;
    [SerializeField] private float longPause = 0.25f; // ! ? ; : …

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource blipSource;
    [SerializeField] private AudioClip blipClip;
    [Tooltip("Play a blip every N visible characters (0 = off).")]
    [SerializeField] private int blipEveryNChars = 2;

    public event System.Action OnTypeComplete;

    private Coroutine _run;
    private bool _isPlaying;
    private readonly StringBuilder _buffer = new StringBuilder();

    void Reset()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    void Awake()
    {
        if (!textUI) textUI = GetComponent<TextMeshProUGUI>();
    }

    public void StopAll()
    {
        if (_run != null) StopCoroutine(_run);
        _run = null;
        _isPlaying = false;
    }

    /// <summary>
    /// Starts streaming the given line of text.
    /// </summary>
    public void PlayLine(string line, Action onFinished = null)
    {
        StopAll();
        _run = StartCoroutine(TypeLine(line, onFinished));
    }

    private IEnumerator TypeLine(string line, Action onFinished)
    {
        _isPlaying = true;
        _buffer.Clear();
        textUI.text = "";

        int blipCounter = 0;

        for (int i = 0; i < line.Length; i++)
        {
            // Handle TMP rich text tags instantly
            if (line[i] == '<')
            {
                int tagEnd = line.IndexOf('>', i);
                if (tagEnd >= 0)
                {
                    _buffer.Append(line, i, tagEnd - i + 1);
                    i = tagEnd;
                    textUI.text = _buffer.ToString();
                    continue;
                }
            }

            // Append next visible char
            _buffer.Append(line[i]);
            textUI.text = _buffer.ToString();

            // Blip sound
            if (blipClip && blipSource && blipEveryNChars > 0)
            {
                blipCounter++;
                if (blipCounter >= blipEveryNChars)
                {
                    blipCounter = 0;
                    blipSource.PlayOneShot(blipClip);
                }
            }

            // Compute delay
            float cps = charsPerSecond * (Input.GetKey(holdToSpeedUpKey) ? speedUpMultiplier : 1f);
            if (cps < 1f) cps = 1f; // safety
            float delay = 1f / cps;

            // Extra punctuation pause
            char c = line[i];
            if (c == ',') delay += commaPause;
            else if (c == '.') delay += periodPause;
            else if (c == '!' || c == '?' || c == ';' || c == ':' || c == '…') delay += longPause;

            // Wait per-frame so speedup can apply dynamically
            float t = 0f;
            while (t < delay)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        _isPlaying = false;
        OnTypeComplete?.Invoke();
        onFinished?.Invoke();
    }
}
