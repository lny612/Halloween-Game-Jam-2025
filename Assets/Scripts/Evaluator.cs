using UnityEngine;

public struct EvalResult { public float exec; public float synergy; public float total; public string summary; }

public class Evaluator : MonoBehaviour
{
    /*public EvalResult Score(ChildProfile child, RecipeDefinition recipe, CraftResult craft)
    {
        float exec = craft.steps.Count == 0 ? 0f : craft.steps.Average(s => s.accuracy);
        float overlap = recipe.targetTags.Intersect(child.tags).Count();
        float denom = Mathf.Max(1, recipe.targetTags.Length);
        float synergy = overlap / denom; // 0..1

        float total = exec * 0.6f + synergy * 0.4f;
        string sum = $"Exec:{exec:F2} Syn:{synergy:F2} Total:{total:F2}";
        return new EvalResult { exec = exec, synergy = synergy, total = total, summary = sum };
    }*/
}