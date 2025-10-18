using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private ChildProfileContainer childProfileContainer;
    [SerializeField] List<ChildProfile> visitedChild;
    [SerializeField] SpiritSightController spiritSightController;

    [Header("Private variables")]
    private List<ChildProfile> childQueue;
    private LoopState state;
    private ChildProfile currentChild;
    private RecipeDefinition _currentRecipe;
    private int _roundNumber = 5;
    private int _currentRound = 0;
    private List<CraftResult> craftResults = new List<CraftResult>();
    private int _scoreCounter = 0;

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
                    visitedChild.Add(childQueue[_currentRound]);
                    currentChild = childQueue[_currentRound];
                    UIManager.Instance.ShowDoor();
                    _currentRound++;
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
                spiritSightController.ForceDisableSpiritSight();
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

    public void ChangeGameState(LoopState newState)
    {
        if(newState.Equals(LoopState.Examine))
        {
            SoundManager.Instance.PlaySfx(Sfx.DoorCreak);
        }
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

    public CandyGrade DetermineRank(CandyName candyName)
    {
        // Same thresholds as before, but using adjusted score
        CandyGrade grade;
        if (_scoreCounter == 10) grade = CandyGrade.Divine;
        else if (_scoreCounter >= 8) grade = CandyGrade.Deluxe;
        else if (_scoreCounter >= 5) grade = CandyGrade.Sweet;
        else if (_scoreCounter >= 2) grade = CandyGrade.Sticky;
        else grade = CandyGrade.Burnt;

        CraftResult result = new CraftResult
        {
            candyName = candyName,
            candyGrade = grade,
            isMatching = IsCandyMatching(candyName)
        };
        craftResults.Add(result);

        return grade;
    }

    #region getter
    public bool IsCandyMatching(CandyName candyName)
    {
        return candyName == currentChild.matchingCandy;
    }

    public void SetRecipePerformance(float successRatio)
    {
        if (successRatio >= 1f) _scoreCounter += 5;
        else if (successRatio >= 0.75f) _scoreCounter += 4;
        else if (successRatio >= 0.5f) _scoreCounter += 3;
        else if (successRatio >= 0.25f) _scoreCounter += 2;
        else _scoreCounter += 1;
    }

    public void SetBoilingPerformance(float successRatio)
    {
        if (successRatio >= 1f) _scoreCounter += 1;
        else if(successRatio >= 0.75f) _scoreCounter += 2;
        else if (successRatio >= 0.5f) _scoreCounter += 3;
        else if (successRatio >= 0.25f) _scoreCounter += 4;
        else _scoreCounter += 5;

    }

    public List<CraftResult> GetCraftResults()
    {
        return craftResults;
    }
    #endregion getter
}
