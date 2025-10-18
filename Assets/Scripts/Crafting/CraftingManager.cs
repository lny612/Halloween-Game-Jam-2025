using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingManager : MonoBehaviour
{
    [Header("Data")]
    private RecipeDefinition activeRecipe;

    [Header("References")]
    [Tooltip("Parent with HorizontalLayoutGroup that will hold one StepSlotUI per step")]
    public Transform conveyorParent;
    [Tooltip("Prefab with StepSlotUI component")]
    public StepSlotUI stepSlotPrefab;

    [Header("Controllers")]
    public StirManager stirManager;
    public ScalePourManager scalePourManager;
    public CraftingResultUI craftingResultUI;
    public CauldronBoilMinigame cauldronBoilMinigame;


    [Header("Conveyor Movement")]
    [Tooltip("Units per second to move the conveyor left while crafting runs.")]
    public float conveyorMoveSpeed = 20f;

    public static CraftingManager Instance { get; private set; }
    private List<StepSlotUI> _slots = new List<StepSlotUI>();
    private bool _running;
    private Vector3 _initialConveyorPos;
    private int successCount = 0;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (conveyorParent != null)
            _initialConveyorPos = conveyorParent.localPosition;
    }

    void Update()
    {
        if (_running && conveyorParent != null)
        {
            Vector3 pos = conveyorParent.localPosition;
            pos.x -= conveyorMoveSpeed * Time.deltaTime; // move left
            conveyorParent.localPosition = pos;
        }
    }

    public void BeginRecipe(RecipeDefinition recipe)
    {

        SoundManager.Instance.StartBoilingLoop();
        if (recipe == null)
        {
            Debug.LogError("CraftingManager: No recipe!");
            return;
        }

        // Reset position
        if (conveyorParent != null)
            conveyorParent.localPosition = _initialConveyorPos;

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

        // Close Result UI
        craftingResultUI.gameObject.SetActive(false);

        // Run sequence
        if (!_running) StartCoroutine(RunRecipe());

    }

    private IEnumerator RunRecipe()
    {
        _running = true;
        successCount = 0;

        for (int i = 0; i < activeRecipe.steps.Length; i++)
        {
            RecipeStep step = activeRecipe.steps[i];
            StepSlotUI slot = _slots[i];

            slot.SetActive(true);
            bool success = false;
            float elapsed = 0f;
            slot.SetFill(0f);
            slot.ShowNeutral();

            switch (step.stepType)
            {
                case StepType.Add when step is AddIngredientStep addIngredientStep:
                    scalePourManager.InitializeCurrentRecipeStep(addIngredientStep);
                    scalePourManager.gameObject.SetActive(true);
                    break;

                case StepType.Stir when step is StirStep stirStep:
                    stirManager.gameObject.SetActive(true);
                    stirManager.Begin(stirStep.stirRequiredCount,
                                      stirStep.stirMinInterval,
                                      stirStep.timeLimit);
                    break;
            }

            SoundManager.Instance.StartLoopSfx(Sfx.TimeTicking, 0.9f);

            // run timer
            while (elapsed < step.timeLimit)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / step.timeLimit);
                slot.SetFill(tNorm);

                if (step.stepType == StepType.Add && scalePourManager.IsComplete)
                {
                    success = scalePourManager.WasSuccessful;
                    break;
                }
                else if (step.stepType == StepType.Stir && stirManager.IsComplete)
                {
                    success = stirManager.WasSuccessful;
                    break;
                }

                yield return null;
            }

            SoundManager.Instance.StopLoopSfx(Sfx.TimeTicking, 0.08f);

            // time ended → finalize appropriately
            if (step.stepType == StepType.Add)
            {
                if (!scalePourManager.IsComplete)
                {
                    // evaluate by tolerance instead of auto-failing
                    scalePourManager.CompleteByTolerance();
                }
                success = scalePourManager.WasSuccessful;
            }
            else if (step.stepType == StepType.Stir)
            {
                if (!stirManager.IsComplete)
                {
                    // keep your existing behavior (or add a similar tolerance finish if you have one)
                    stirManager.ForceFinish(false);
                }
                success = stirManager.WasSuccessful;
            }

            // close sub-panels
            scalePourManager.gameObject.SetActive(false);
            stirManager.gameObject.SetActive(false);

            // show result + count ONCE
            if (success)
            {
                SoundManager.Instance.PlaySfx(Sfx.StepSuccess);
                slot.ShowTick();
                successCount++;
            }
            else
            {
                SoundManager.Instance.PlaySfx(Sfx.StepFail);
                slot.ShowCross();
            }
            slot.SetFill(1f);

            yield return new WaitForSeconds(0.25f);
            
        }

        OnAllStepsFinished();
        _running = false;
    }

    public void OnAllStepsFinished()
    {
        SoundManager.Instance.StopBoilingLoop();
        Debug.Log("[Crafting] successCount=" + successCount);
        Debug.Log("[Crafting] activeRecipeLength=" + activeRecipe.steps.Length);
        cauldronBoilMinigame.StopBoiling();
        GameManager.Instance.SetRecipePerformance(successCount / activeRecipe.steps.Length);
        var resultCandyGrade = GameManager.Instance.DetermineRank(activeRecipe.candyName);
        craftingResultUI.SetResult(resultCandyGrade, activeRecipe);
        craftingResultUI.gameObject.SetActive(true);
    }
}
