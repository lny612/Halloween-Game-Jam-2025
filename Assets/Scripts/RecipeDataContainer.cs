using System.Collections.Generic;
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
    public CraftStepDefinition[] steps;     // ordered steps
}