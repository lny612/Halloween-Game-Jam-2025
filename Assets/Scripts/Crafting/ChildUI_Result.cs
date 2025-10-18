using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChildUI_Result : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _farewellText;
    [SerializeField] private Image _childImage;

    [Header("Streaming (auto-filled if left null)")]
    private StreamingDialogue _speechStreamer;

    void Awake()
    {
        if (!_speechStreamer && _farewellText)
            _speechStreamer = _farewellText.GetComponent<StreamingDialogue>();
    }

    public void InitializeResultUI()
    {
        var currentChild = GameManager.Instance.GetCurrentChild();
        _childImage.sprite = currentChild.childImage;

        if (_speechStreamer)
        {
            _speechStreamer.PlayLine(currentChild.farewell);
        }

    }
}
