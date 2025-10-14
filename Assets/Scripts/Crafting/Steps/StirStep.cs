using UnityEngine;

[CreateAssetMenu(fileName = "StirStep", menuName = "RecipeStep/Stir")]
public class StirStep : RecipeStep
{
    public override StepType stepType => StepType.Stir;
}
