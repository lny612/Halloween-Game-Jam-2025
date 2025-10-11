using System.Collections.Generic;
using UnityEngine;

public enum LoopState { Arrival, Examine, SelectRecipe, Craft, Evaluate, Result }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] List<ChildProfile> childQueue;
    [SerializeField] List<ChildProfile> visitedChild;
    [SerializeField] CraftingManager crafting;
    [SerializeField] Evaluator evaluator;

    int childIndex = 0;
    LoopState state;

    ChildProfile currentChild;
    RecipeDefinition selectedRecipe;
    //CraftResult craftResult;
    EvalResult evalResult;

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
        state = LoopState.Arrival;
        Advance();
    }

    public void Advance()
    {
        switch (state)
        {
            case LoopState.Arrival:
                RandomlySelectChild();
                UIManager.Instance.ShowDoor();
                break;

            case LoopState.Examine:
                UIManager.Instance.ShowVisitor();
                break;

            case LoopState.SelectRecipe:
                UIManager.Instance.DisplayRecipe();
                break;

            /*case LoopState.Craft:
                ui.ShowCraft();
                crafting.Run(selectedRecipe, onDone: (cr) => { craftResult = cr; state = LoopState.Evaluate; Advance(); });
                break;

            case LoopState.Evaluate:
                evalResult = evaluator.Score(currentChild, selectedRecipe, craftResult);
                state = LoopState.Result;
                Advance();
                break;

            case LoopState.Result:
                ui.ShowResult(currentChild, selectedRecipe, evalResult, onNext: () => {
                    childIndex++;
                    state = LoopState.Arrival;
                    Advance();
                });
                break;*/
        }
    }

    private void RandomlySelectChild()
    {
        int randomIndex = Random.Range(0, childQueue.Count);
        while (visitedChild.Contains(childQueue[randomIndex]))
        {
            randomIndex = Random.Range(0, childQueue.Count);
        }
        
        visitedChild.Add(childQueue[randomIndex]);
        currentChild = childQueue[randomIndex];
    }

    public void ChangeGameState(LoopState newState)
    {
        state = newState;
        Advance();
    }

    public ChildProfile GetCurrentChild()
    {
        return currentChild;
    }
    //List<RecipeDef> FilterRecipesFor(ChildProfile c, List<RecipeDef> all) => all; // Day1: no filtering
}
