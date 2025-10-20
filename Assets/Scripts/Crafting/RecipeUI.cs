using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Description;
    [SerializeField] private TextMeshProUGUI RecipeSteps;
    [SerializeField] private Image CandyImage;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startCraftButton;
    [SerializeField] private RecipeDataContainer recipeDataContainer;

    [SerializeField] private GameObject inspectButtonHover;
    [SerializeField] private GameObject craftCandyButtonHover;


    private int currentPage = 0;
    public void Initialize()
    {
        inspectButtonHover.SetActive(false);
        craftCandyButtonHover.SetActive(false);
        SetInformation(currentPage);
    }

    public void SetInformation(int page)
    {
        var currentRecipe = recipeDataContainer.recipeList[page];
        Name.text = currentRecipe.recipeName;
        Description.text = currentRecipe.descriptionText;
        SetRecipeText(currentRecipe);
        CandyImage.sprite = currentRecipe.recipeImage;
    }

    private void SetRecipeText(RecipeDefinition currentRecipe)
    {
        RecipeSteps.text = "";
        for (int i = 0; i < currentRecipe.steps.Length; i++)
        {
            RecipeSteps.text += (i + 1).ToString() + ". " + currentRecipe.steps[i].instruction + "\n";
        }
    }

    public void OnNextButtonPressed()
    {
        if (currentPage < recipeDataContainer.recipeList.Count - 1)
        {
            currentPage++;
            backButton.interactable = true;
            SetInformation(currentPage);
        }

        else if (currentPage == recipeDataContainer.recipeList.Count - 1)
        {
            backButton.interactable = true;
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
            backButton.interactable = true;
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

    public void OnRecipeSelectButtonPressed()
    {
        GameManager.Instance.SetRecipe(recipeDataContainer.recipeList[currentPage]);
        GameManager.Instance.ChangeGameState(LoopState.Craft);
    }
}
