using TMPro;
using UnityEngine;

public class CraftingResultUI : MonoBehaviour
{
    public TextMeshProUGUI candyGrade;
    public TextMeshProUGUI candyName;
    public Sprite candyImage;

    public void SetResult(GameManager.CandyGrade resultCandyGrade, RecipeDefinition recipeDefinition)
    {
        candyGrade.text = resultCandyGrade.ToString();
        candyName.text = recipeDefinition.recipeName;
        candyImage = recipeDefinition.recipeImage;
    }
}
