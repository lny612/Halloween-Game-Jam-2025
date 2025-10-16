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
    public Sprite recipeImage;
    [TextArea] public string descriptionText;
    [TextArea] public string recipeText;
    public RecipeStep[] steps;     // ordered steps
}

