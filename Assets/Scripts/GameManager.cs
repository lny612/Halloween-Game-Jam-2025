using System.Collections.Generic;
using UnityEngine;

public enum LoopState { Arrival, Examine, SelectRecipe, Craft, Evaluate, Result }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private ChildProfileContainer childProfileContainer;
    [SerializeField] List<ChildProfile> visitedChild;
    //[SerializeField] CraftingManager crafting;
    //[SerializeField] Evaluator evaluator;

    private List<ChildProfile> childQueue;
    int childIndex = 0;
    LoopState state;

    ChildProfile currentChild;
    RecipeDataContainer selectedRecipe;
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
        childQueue = childProfileContainer.childProfileList;
        Advance();
    }

    public void Advance()
    {

        UIManager.Instance.CloseAllPanels();

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

            case LoopState.Craft:
                UIManager.Instance.StartCraft();
                break;

            case LoopState.Evaluate:
                //evalResult = evaluator.Score(currentChild, selectedRecipe, craftResult);
                state = LoopState.Result;
                Advance();
                break;

            case LoopState.Result:
                UIManager.Instance.ShowResult();
                break;
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
