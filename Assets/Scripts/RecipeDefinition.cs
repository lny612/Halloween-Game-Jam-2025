using UnityEngine;

[CreateAssetMenu(menuName = "CandyJam/Recipe")]
public class RecipeDefinition : ScriptableObject
{
    public string recipeName;
    [TextArea] public string flavorText;
    public string[] targetTags;      // tags it’s “meant” for (used in scoring later)
    public CraftStepDefinition[] steps;     // ordered steps
}