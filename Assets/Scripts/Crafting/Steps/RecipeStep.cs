using UnityEngine;

public abstract class RecipeStep : ScriptableObject
{
    public abstract StepType stepType { get; }
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
    public float tolerance = 5f;

    [Header("Stir")]
    public int stirRequiredCount = 5;
    public float stirMinInterval = 0.15f; // anti-mash delay between stirs
}

public enum IngredientType { Water, Sugar, Essence }


public enum StepType { Stir, Add, Wait }