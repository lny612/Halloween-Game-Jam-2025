using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FloatingMotion : MonoBehaviour
{
    [Header("Float Motion Settings")]
    [Tooltip("How far (in pixels) the UI element moves up and down.")]
    public float floatAmplitude = 15f;

    [Tooltip("How fast the UI element oscillates.")]
    public float floatFrequency = 1f;

    [Tooltip("Randomize phase offset so multiple elements don't move identically.")]
    public bool randomizePhase = true;

    private RectTransform _rectTransform;
    private Vector2 _startAnchoredPos;
    private float _phaseOffset;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startAnchoredPos = _rectTransform.anchoredPosition;
        _phaseOffset = randomizePhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        float newY = Mathf.Sin((Time.time + _phaseOffset) * floatFrequency) * floatAmplitude;
        Vector2 pos = _startAnchoredPos;
        pos.y += newY;
        _rectTransform.anchoredPosition = pos;
        //Debug.Log("pos " + pos);
    }
}
