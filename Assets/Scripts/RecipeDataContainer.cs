using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDataContainer", menuName = "Recipe/Recipe")]
public class RecipeDataContainer : ScriptableObject
{
    public List<RecipeDefinition> recipeList;
}
[System.Serializable]
public class RecipeDefinition
{
    public string recipeName;
    [TextArea] public string descriptionText;
    [TextArea] public string recipeText;
    public RecipeStep[] steps;     // ordered steps
}

public abstract class RecipeStep : ScriptableObject
{
    public abstract StepType Type { get; }
    public string instruction;   // “Stir 5 times”, “Add Blue at 4.5s”
    [Header("Common")]
    public Sprite icon;
    [Tooltip("Seconds allowed for this step")]
    public float timeLimit = 10f;

    [Header("Add Ingredient")]
    public string ingredientName;         // e.g., "Water", "Iris Sugar", "Horse Essence"
    public IngredientType ingredientType;
    public float targetAmount;            // e.g., 200 (ml) or 100 (g)
    public string unit = "ml";            // "ml" or "g"
    public float pourRatePerSecond = 100; // how fast the amount grows while holding

    [Header("Stir")]
    public int stirRequiredCount = 5;
    public float stirMinInterval = 0.15f; // anti-mash delay between stirs

    public string SetUnit(IngredientType type)
    {
        switch (type)
        {
            case IngredientType.Water:
                unit = "ml";
                break;
            case IngredientType.Sugar:
                unit = "g";
                break;
            case IngredientType.Essence:
                unit = "drops";
                break;
        }

        return "ml";
    }

}

public enum IngredientType { Water, Sugar, Essence}


public enum StepType { Stir, Add, Wait }