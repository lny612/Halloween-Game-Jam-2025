using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    public Image AnchorImage;
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI endingResultText;
    public TextMeshProUGUI commentText;
    public EndingDataContainer endingDataContainer;
    private List<EndingScripts> endingScriptsList = new List<EndingScripts>();

    public GameObject goodMark;
    public GameObject badMark;

    public Button nextButton;
    public GameObject helperArrowImage;

    [Header("Wrong Ending")]
    [TextArea] public string defaultHeadline;
    [TextArea] public string defaultEndingText;
    [TextArea] public string defaultComment = "It seems like I failed to grant the {0} child’s wish.";
    public Sprite defaultImage;

    private int _endingIndex = 0;
    private int _numberOfEndings;

    private List<CraftResult> _craftResults = new List<CraftResult>();

    public void InitializeEndingUI()
    {
        _craftResults = GameManager.Instance.GetCraftResults();
        _numberOfEndings = _craftResults.Count;

        _endingIndex = 0;
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
        EndingScripts endingScripts = GetWantedCandyEnding(result.candyName);

        // if the candy is what the child wanted
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
        // if the candy is not what the child wanted
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

        // Move index to the next ending to be shown when user presses Next
        _endingIndex++;
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
        nextButton.interactable = false;

        headlineText.text = headline;
        AnchorImage.sprite = image;

        if (isTrueEnding)
        {
            goodMark.SetActive(true);
            badMark.SetActive(false);
            SoundManager.Instance.PlaySfx(Sfx.StepSuccess);
        }
        else
        {
            goodMark.SetActive(false);
            badMark.SetActive(true);
            SoundManager.Instance.PlaySfx(Sfx.StepFail);
        }

        StopAllCoroutines();
        StartCoroutine(CoStreamEndingSequence(endingResult, comment));
    }

    private System.Collections.IEnumerator CoStreamEndingSequence(string endingResult, string comment)
    {
        var resultStreamer = endingResultText.GetComponent<StreamingDialogue>();
        var commentStreamer = commentText.GetComponent<StreamingDialogue>();

        // Stream the ending result line and wait for typing to finish
        resultStreamer.PlayLine(endingResult);
        yield return StartCoroutine(WaitForTypeComplete(resultStreamer));

        // Stream the comment line and wait for typing to finish
        commentStreamer.PlayLine(comment);
        yield return StartCoroutine(WaitForTypeComplete(commentStreamer));

        if (_endingIndex < _numberOfEndings)
        {
            helperArrowImage.SetActive(true);
            nextButton.interactable = true;
        }
        else
        {
            nextButton.interactable = false;
        }
    }

    private System.Collections.IEnumerator WaitForTypeComplete(StreamingDialogue streamer)
    {
        bool done = false;
        System.Action handler = () => done = true;
        streamer.OnTypeComplete += handler;
        while (!done) yield return null;
        streamer.OnTypeComplete -= handler;
    }

    public void OnNextPressed()
    {
        helperArrowImage.SetActive(false);

        if (_endingIndex < _numberOfEndings)
        {
            PlayEnding(_craftResults[_endingIndex]);
        }
        else
        {
            nextButton.interactable = false;
        }
    }

    // Converts 1 -> "first", 2 -> "second", 3 -> "third", etc.
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
}
