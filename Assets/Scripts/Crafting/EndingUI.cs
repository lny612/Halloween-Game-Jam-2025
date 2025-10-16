using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI endingResultText;
    public TextMeshProUGUI commentText;
    public Image endingImage;
    public EndingDataContainer endingDataContainer;
    private List<EndingScripts> endingScriptsList = new List<EndingScripts>();

    public void InitializeEndingUI()
    {
        var craftResults = GameManager.Instance.GetCraftResults();

        foreach (var result in craftResults)
        {
            EndingScripts endingScripts= GetWantedCandyEnding(result.candyName);

            if (result.isMatching)
            {
                SetEndingUI(endingScripts.correctHeadline, endingScripts.correctEndingText, endingScripts.correctComment, endingScripts.correctImage);
            }
            else
            {
                SetEndingUI(endingScripts.wrongHeadline, endingScripts.wrongEndingText, endingScripts.wrongComment, endingScripts.wrongImage);
            }
        }
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
    public void SetEndingUI(string headline, string endingResult, string comment, Sprite image)
    {
        headlineText.text = headline;
        endingResultText.text = endingResult;
        commentText.text = comment;
        endingImage.sprite = image;
    }

}
