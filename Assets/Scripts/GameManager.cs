using System.Collections.Generic;
using UnityEngine;

public enum LoopState { Arrival, Examine, SelectRecipe, Craft, Evaluate, Result }

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager ui;
    [SerializeField] List<ChildProfile> childQueue;
    [SerializeField] List<RecipeDefinition> allRecipes;
    [SerializeField] CraftingManager crafting;
    [SerializeField] Evaluator evaluator;

    int childIndex = 0;
    LoopState state;

    ChildProfile currentChild;
    RecipeDefinition selectedRecipe;
    //CraftResult craftResult;
    EvalResult evalResult;

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
                currentChild = childQueue[childIndex % childQueue.Count];
                ui.ShowDoor(currentChild, onContinue: () => { state = LoopState.Examine; Advance(); });
                break;

            case LoopState.Examine:
                ui.ShowExamine(currentChild, onContinue: () => { state = LoopState.SelectRecipe; Advance(); });
                break;

            /*case LoopState.SelectRecipe:
                var candidates = FilterRecipesFor(currentChild, allRecipes); // simple: return all for Day 1
                ui.ShowRecipes(candidates, onPick: (r) => { selectedRecipe = r; state = LoopState.Craft; Advance(); });
                break;

            case LoopState.Craft:
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

    //List<RecipeDef> FilterRecipesFor(ChildProfile c, List<RecipeDef> all) => all; // Day1: no filtering
}
