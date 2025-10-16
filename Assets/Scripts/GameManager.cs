using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private ChildProfileContainer childProfileContainer;
    [SerializeField] List<ChildProfile> visitedChild;
    //[SerializeField] CraftingManager crafting;
    //[SerializeField] Evaluator evaluator;

    [Header("Private variables")]
    private List<ChildProfile> childQueue;
    private int childIndex = 0;
    private LoopState state;
    private ChildProfile currentChild;
    private RecipeDefinition _currentRecipe;
    //CraftResult craftResult;
    private EvalResult evalResult;
    private int _roundNumber = 1;
    private int _currentRound = 0;
    public List<CandyGrade> resultCandyGrades;


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
                if (_currentRound < _roundNumber)
                {
                    _currentRound++;
                    RandomlySelectChild();
                    UIManager.Instance.ShowDoor();
                }
                else
                {
                    ChangeGameState(LoopState.Ending);
                }

                break;

            case LoopState.Examine:

                UIManager.Instance.ShowVisitor();
                break;

            case LoopState.SelectRecipe:
                UIManager.Instance.DisplayRecipe();
                break;

            case LoopState.Craft:
                UIManager.Instance.StartCraft();
                CraftingManager.Instance.BeginRecipe(_currentRecipe);
                break;

            case LoopState.Evaluate:
                //evalResult = evaluator.Score(currentChild, selectedRecipe, craftResult);
                ChangeGameState(LoopState.Result);
                break;

            case LoopState.Result:
                UIManager.Instance.ShowResult();
                break;

            case LoopState.Ending:
                UIManager.Instance.ShowEnding();
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

    public void SetRecipe(RecipeDefinition selectedRecipe)
    {
        _currentRecipe = selectedRecipe;
    }
    public ChildProfile GetCurrentChild()
    {
        return currentChild;
    }

    public CandyGrade DetermineRank(float successRatio)
    {
        if (successRatio >= 1.0f)
        {
            resultCandyGrades.Add(CandyGrade.Divine);
            return CandyGrade.Divine;
        }
        else if (successRatio >= 0.75f)
        {
            resultCandyGrades.Add(CandyGrade.Deluxe);
            return CandyGrade.Deluxe;
        }
        else if (successRatio >= 0.5f)
        {
            resultCandyGrades.Add(CandyGrade.Sweet);
            return CandyGrade.Sweet;
        }

        else if (successRatio >= 0.25f)
        {
            resultCandyGrades.Add(CandyGrade.Sticky);
            return CandyGrade.Sticky;
        }
        else
        {
            resultCandyGrades.Add(CandyGrade.Burnt);
            return CandyGrade.Burnt;
        }
    }
}
