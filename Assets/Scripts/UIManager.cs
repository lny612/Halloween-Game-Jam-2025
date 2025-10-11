using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject door, recipes, craft, result;
    public TMPro.TextMeshProUGUI doorText, examineText, resultText;
    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;

    System.Action onDoorContinue, onExamineContinue, onResultNext;
    System.Action<RecipeDefinition> onPickRecipe;

    public void ShowDoor(ChildProfile c, System.Action onContinue)
    {
        HideAll(); door.SetActive(true);
        doorText.text = $"{c.childName} knocks.\nDesire? (hidden)\nInsecurity? (hidden)";
        onDoorContinue = onContinue;
        // Hook to button OnClick → CallDoorContinue();
    }

    public void CallDoorContinue() => onDoorContinue?.Invoke();

    public void ShowExamine(ChildProfile c, System.Action onContinue)
    {
        HideAll(); door.SetActive(true); // reuse or separate an Examine panel
        doorText.text = $"{c.childName}\nDesire: {c.desire}\nFear: {c.insecurity}";
        onExamineContinue = onContinue;
    }
    public void CallExamineContinue() => onExamineContinue?.Invoke();

    public void ShowRecipes(List<RecipeDefinition> options, System.Action<RecipeDefinition> onPick)
    {
        HideAll(); recipes.SetActive(true);
        onPickRecipe = onPick;
        // Clear children then instantiate buttons; each button .onClick → Pick(recipe)
    }
    public void Pick(RecipeDefinition r) => onPickRecipe?.Invoke(r);

    public void ShowCraft() { HideAll(); craft.SetActive(true); }

    public void ShowResult(ChildProfile c, RecipeDefinition r, EvalResult e, System.Action onNext)
    {
        HideAll(); result.SetActive(true);
        resultText.text = $"{c.childName} got {r.recipeName}\n{e.summary}";
        onResultNext = onNext;
    }
    public void CallNext() => onResultNext?.Invoke();

    void HideAll() { door.SetActive(false); recipes.SetActive(false); craft.SetActive(false); result.SetActive(false); }
}