using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingResultUI : MonoBehaviour
{
    public TextMeshProUGUI candyGrade;
    public TextMeshProUGUI candyName;
    public Image candyImage;

    public void SetResult(CandyGrade resultCandyGrade, RecipeDefinition recipeDefinition)
    {
        candyGrade.text = resultCandyGrade.ToString();
        candyName.text = recipeDefinition.recipeName;
        candyImage.sprite = recipeDefinition.recipeImage;
    }
}
