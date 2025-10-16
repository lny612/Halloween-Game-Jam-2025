using UnityEngine;

[CreateAssetMenu(fileName = "AddIngredientStep", menuName = "RecipeStep/Add Ingredient")]
public class AddIngredientStep : RecipeStep
{
    public override StepType stepType => StepType.Add;

    [Header("Add Ingredient")]
    public string ingredientName;         // e.g., "Water", "Iris Sugar", "Horse Essence"
    public IngredientSubtype ingredientSubType;
    public float targetAmount;            // e.g., 200 (ml) or 100 (g)
    public string unit = "ml";            // "ml" or "g"
    public float tolerance = 5f;
}