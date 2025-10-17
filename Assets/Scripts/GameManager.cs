using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private ChildProfileContainer childProfileContainer;
    [SerializeField] List<ChildProfile> visitedChild;

    [Header("Private variables")]
    private List<ChildProfile> childQueue;
    private LoopState state;
    private ChildProfile currentChild;
    private RecipeDefinition _currentRecipe;
    private int _roundNumber = 1;
    private int _currentRound = 0;
    private List<CraftResult> craftResults = new List<CraftResult>();
    private float _recipePerformance;
    private float _boilingPerformance;


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

    public CandyGrade DetermineRank(CandyName candyName)
    {
        // Normalize "bad time": 0..1 of session spent at zero quality
        float zeroFrac = Mathf.Clamp01(_boilingPerformance);

        // Penalty curve (tweakables):
        // - maxPenalty: max % reduction applied to successRatio if zeroFrac == 1
        // - curve: <1 makes small zero time matter more, >1 makes it matter less
        //const float maxPenalty = 0.35f; // up to -35% to success score
        //const float curve = 0.75f;      // slightly front-load penalty for early mistakes

        //float penalty = Mathf.Lerp(0f, maxPenalty, Mathf.Pow(zeroFrac, curve));
        float penalty = 0;
        float adjusted = Mathf.Clamp01(_recipePerformance * (1f - penalty));

        // Same thresholds as before, but using adjusted score
        CandyGrade grade;
        if (adjusted >= 1.0f) grade = CandyGrade.Divine;
        else if (adjusted >= 0.75f) grade = CandyGrade.Deluxe;
        else if (adjusted >= 0.5f) grade = CandyGrade.Sweet;
        else if (adjusted >= 0.25f) grade = CandyGrade.Sticky;
        else grade = CandyGrade.Burnt;

        CraftResult result = new CraftResult
        {
            candyName = candyName,
            candyGrade = grade,
            isMatching = IsCandyMatching(candyName)
        };
        craftResults.Add(result);

        Debug.Log($"[CandyGrade] success={_recipePerformance:0.00}, zeroFrac={zeroFrac:0.00}, adjusted={adjusted:0.00} → {grade}");

        return grade;
    }

    #region getter
    public bool IsCandyMatching(CandyName candyName)
    {
        return candyName == currentChild.matchingCandy;
    }

    public void SetRecipePerformance(float successRatio)
    {
        _recipePerformance = successRatio;
    }

    public void SetBoilingPerformance(float successRatio)
    {
        _boilingPerformance = successRatio;
    }

    public List<CraftResult> GetCraftResults()
    {
        return craftResults;
    }
    #endregion getter
}
