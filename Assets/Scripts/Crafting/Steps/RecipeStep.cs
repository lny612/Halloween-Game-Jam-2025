using UnityEngine;

public abstract class RecipeStep : ScriptableObject
{
    public abstract StepType stepType { get; }
    public string instruction;   // “Stir 5 times”, “Add Blue at 4.5s”
    [Header("Common")]
    public Sprite icon;
    [Tooltip("Seconds allowed for this step")]
    public float timeLimit = 10f;
}

public enum IngredientType { Water, Sugar, Essence }
public enum StepType { Stir, Add, Wait }