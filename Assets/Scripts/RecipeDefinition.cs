using UnityEngine;

[CreateAssetMenu(menuName = "CandyJam/Recipe")]
public class RecipeDefinition : ScriptableObject
{
    public string recipeName;
    [TextArea] public string descriptionText;
    [TextArea] public string recipeText;
    public CraftStepDefinition[] steps;     // ordered steps
}