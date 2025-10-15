using UnityEngine;

[CreateAssetMenu(fileName = "StirStep", menuName = "RecipeStep/Stir")]
public class StirStep : RecipeStep
{
    public override StepType stepType => StepType.Stir;

    [Header("Stir")]
    public int stirRequiredCount = 5;
    public float stirMinInterval = 0.15f; // anti-mash delay between stirs
}
