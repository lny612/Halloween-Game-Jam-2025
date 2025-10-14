using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingManager : MonoBehaviour
{
    [Header("Data")]
    public RecipeDefinition activeRecipe;

    [Header("References")]
    [Tooltip("Parent with HorizontalLayoutGroup that will hold one StepSlotUI per step.")]
    public Transform conveyorParent;
    [Tooltip("Prefab with StepSlotUI component (icon, fill, tick/X).")]
    public StepSlotUI stepSlotPrefab;

    [Header("Controllers")]
    public StirManager stirController;             // hook UI panel for stirring
    public ScalePourManager scalePourController;   // hook UI panel for pouring

    [Header("Events")]
    public UnityEvent onAllStepsFinished;             // recipe sequence done

    private List<StepSlotUI> _slots = new List<StepSlotUI>();
    private bool _running;

    void Start()
    {
        // You can call BeginRecipe(activeRecipe) from GameLoop, or auto-start if assigned:
        if (activeRecipe != null) BeginRecipe(activeRecipe);
    }

    public void BeginRecipe(RecipeDefinition recipe)
    {
        if (recipe == null) { Debug.LogError("CraftingManager: No recipe!"); return; }

        // Clear old
        foreach (Transform t in conveyorParent) Destroy(t.gameObject);
        _slots.Clear();

        activeRecipe = recipe;

        // Build conveyor UI
        foreach (var step in activeRecipe.steps)
        {
            var slot = Instantiate(stepSlotPrefab, conveyorParent);
            slot.Setup(step.icon, step.instruction, step.timeLimit);
            _slots.Add(slot);
        }

        // Run sequence
        if (!_running) StartCoroutine(RunRecipe());
    }

    private IEnumerator RunRecipe()
    {
        _running = true;

        for (int i = 0; i < activeRecipe.steps.Length; i++)
        {
            RecipeStep step = activeRecipe.steps[i];
            StepSlotUI slot = _slots[i];

            slot.SetActive(true);

            bool success = false;
            float elapsed = 0f;
            slot.SetFill(0f);
            slot.ShowNeutral();

            // Start the appropriate sub-minigame
            switch (step.stepType)
            {
                case StepType.Add:
                    scalePourController.gameObject.SetActive(true);
                    scalePourController.Begin(step.ingredientName,
                                              step.targetAmount,
                                              step.unit,
                                              step.tolerance,
                                              step.pourRatePerSecond,
                                              step.timeLimit);
                    break;

                case StepType.Stir:
                    stirController.gameObject.SetActive(true);
                    stirController.Begin(step.stirRequiredCount,
                                         step.stirMinInterval,
                                         step.timeLimit);
                    break;
            }

            // While the step time runs, animate the slot’s fill (left?right), and poll sub-controller result
            while (elapsed < step.timeLimit)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / step.timeLimit);
                slot.SetFill(tNorm);

                // Check completion
                if (step.stepType == StepType.Add)
                {
                    if (scalePourController.IsComplete)
                    {
                        success = scalePourController.WasSuccessful;
                        break;
                    }
                }
                else if (step.stepType == StepType.Stir)
                {
                    if (stirController.IsComplete)
                    {
                        success = stirController.WasSuccessful;
                        break;
                    }
                }

                yield return null;
            }

            // Time ended: if not completed, finalize with fail (ScalePour/Stir handle internal scoring too)
            if (step.stepType == StepType.Add && !scalePourController.IsComplete)
            {
                scalePourController.ForceFinish(false);
                success = false;
            }
            else if (step.stepType == StepType.Stir && !stirController.IsComplete)
            {
                stirController.ForceFinish(false);
                success = false;
            }

            // Close sub-panels
            scalePourController.gameObject.SetActive(false);
            stirController.gameObject.SetActive(false);

            // Tick / X and freeze slot
            if (success) slot.ShowTick();
            else slot.ShowCross();
            slot.SetFill(1f);

            // short beat between steps
            yield return new WaitForSeconds(0.25f);
        }

        _running = false;
        onAllStepsFinished?.Invoke();
    }
}
