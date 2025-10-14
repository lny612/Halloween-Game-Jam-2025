using UnityEngine;

[CreateAssetMenu(fileName = "AddIngredientStep", menuName = "RecipeStep/Add Ingredient")]
public class AddIngredientStep : RecipeStep
{
    public override StepType stepType => StepType.Add;
}