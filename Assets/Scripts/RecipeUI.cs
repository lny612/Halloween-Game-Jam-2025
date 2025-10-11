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

    [SerializeField] List<RecipeDefinition> allRecipes;
    private int currentPage = 0;
    public void Initialize()
    {
        SetInformation(currentPage);
    }

    public void SetInformation(int page)
    {
        Name.text = allRecipes[page].recipeName;
        Description.text = allRecipes[page].descriptionText;
        RecipeSteps.text = allRecipes[page].recipeText;
    }

    public void OnNextButtonPressed()
    {
        if (currentPage < allRecipes.Count - 1)
        {
            currentPage++;
            SetInformation(currentPage);
        }

        else if (currentPage == allRecipes.Count - 1)
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
