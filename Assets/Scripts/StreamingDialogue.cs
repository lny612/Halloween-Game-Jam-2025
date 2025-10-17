// StreamingDialogue_NoCursor.cs
// Same behavior as before, but with NO cursor glyph or blink.

using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Skipping")]
    [Tooltip("Press to instantly complete the current line; press again to advance.")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [Tooltip("Allow mouse button to act like advance key.")]
    [SerializeField] private bool mouseAdvances = true;

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
    private bool _lineComplete;
    private bool _isPlaying;
    private StringBuilder _buffer = new StringBuilder();

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
        _lineComplete = false;
    }

    public void PlayLine(string line, Action onFinished = null)
    {
        PlayLines(new List<string> { line }, onFinished);
    }

    public void PlayLines(IList<string> lines, Action onFinished = null)
    {
        StopAll();
        _run = StartCoroutine(RunLines(lines, onFinished));
    }

    private IEnumerator RunLines(IList<string> lines, Action onFinished)
    {
        _isPlaying = true;

        for (int i = 0; i < lines.Count; i++)
        {
            yield return StartCoroutine(TypeLine(lines[i]));
            // Wait for advance to go to next line
            yield return StartCoroutine(WaitForAdvance());
        }

        _isPlaying = false;
        onFinished?.Invoke();
    }

    private IEnumerator TypeLine(string line)
    {
        _lineComplete = false;
        _buffer.Clear();

        textUI.text = "";

        int blipCounter = 0;

        for (int i = 0; i < line.Length; i++)
        {
            // Handle TMP rich text tags instantly (do not type them one by one)
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
            blipCounter++;

            textUI.text = _buffer.ToString();

            // Play blip
            if (blipClip && blipSource && blipEveryNChars > 0 && blipCounter >= blipEveryNChars)
            {
                blipCounter = 0;
                blipSource.PlayOneShot(blipClip);
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

            // If player presses advance, instantly complete the line
            if (PressedAdvance())
            {
                textUI.text = line;
                break;
            }

            yield return new WaitForSeconds(delay);
        }

        // Line complete
        _lineComplete = true;
        textUI.text = _buffer.ToString();

        if (OnTypeComplete != null) OnTypeComplete.Invoke();
    }

    private IEnumerator WaitForAdvance()
    {
        while (true)
        {
            if (PressedAdvance())
            {
                // If line was still streaming, the first press completed it.
                // If it's already complete, the press advances to next line (exit).
                if (_lineComplete) break;
                _lineComplete = true;
            }
            yield return null;
        }
    }

    private bool PressedAdvance()
    {
        bool key = Input.GetKeyDown(advanceKey);
        bool mouse = mouseAdvances && Input.GetMouseButtonDown(0);
        return key || mouse;
    }
}
