using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Description;
    [SerializeField] private TextMeshProUGUI RecipeSteps;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private RecipeDataContainer recipeDataContainer;

    private int currentPage = 0;
    public void Initialize()
    {
        SetInformation(currentPage);
    }

    public void SetInformation(int page)
    {
        var currentRecipe = recipeDataContainer.recipeList[page];
        Name.text = currentRecipe.recipeName;
        Description.text = currentRecipe.descriptionText;
        RecipeSteps.text = currentRecipe.recipeText;
    }

    public void OnNextButtonPressed()
    {
        if (currentPage < recipeDataContainer.recipeList.Count - 1)
        {
            currentPage++;
            SetInformation(currentPage);
        }

        else if (currentPage == recipeDataContainer.recipeList.Count - 1)
        {
            nextButton.interactable = false;
        }
        else
        {
            backButton.interactable = true;
            nextButton.interactable = true;
        }
    }

    public void OnBackButtonPressed()
    {
        if (currentPage > 0)
        {
            currentPage--;
            SetInformation(currentPage);
        }

        if (currentPage == 0)
        {
            backButton.interactable = false;
        }
        else
        {
            backButton.interactable = true;
            nextButton.interactable = true;

        }
    }
}
