using UnityEngine;

public abstract class CraftStepDefinition : ScriptableObject
{
    public string instruction;   // “Stir 5 times”, “Add Blue at 4.5s”
    public float timeWindowStart; // for timing steps; Day1 can ignore
    public float timeWindowEnd;

    public abstract StepType Type { get; }
}

public enum StepType { Stir, Add, Wait }