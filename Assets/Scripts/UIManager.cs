using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject ArrivalPanel;
    [SerializeField] private GameObject ExaminationPanel;
    [SerializeField] private GameObject RecipePanel;
    [SerializeField] private GameObject CraftingPanel;
    [SerializeField] private GameObject ResultPanel;
    [SerializeField] private GameObject EndingPanel;

    [Header("Prefabs")]
    [SerializeField] private ChildUI childUI;
    [SerializeField] private RecipeUI recipeUI;
    [SerializeField] private EndingUI endingUI;

    [Header("Buttons")]
    [SerializeField] private Button ProceedToExamineButton;
    [SerializeField] private Button ProceedToRecipeButton;
    [SerializeField] private Button ProceedToCraftingButton;
    [SerializeField] private Button ProceedToResultButton;
    [SerializeField] private Button ProceedToArrivalButton;

    [Header("Scripts")]
    [SerializeField] private KnockBubble knockBubble;


    public static UIManager Instance { get; private set; }
    private bool isExaminePressed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        ProceedToExamineButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Examine));
        ProceedToRecipeButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.SelectRecipe));
        //ProceedToCraftingButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Craft));
        ProceedToResultButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Result));
        ProceedToArrivalButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Arrival));
        //ProceedToArrivalButton.onClick.AddListener(() => OnContinueClicked(LoopState.Examine));
    }

    public void ShowDoor()
    {
        // TODO: show arrival panel and play knocking animation
        ArrivalPanel.SetActive(true);
        knockBubble.Play();
    }

    public void CloseAllPanels()
    {
        ArrivalPanel.SetActive(false);
        ExaminationPanel.SetActive(false);
        RecipePanel.SetActive(false);
        CraftingPanel.SetActive(false);
        ResultPanel.SetActive(false);
    }

    public void EnableProceedToRecipeButton()
    {
        ProceedToRecipeButton.interactable = true;
    }

    public void ShowVisitor()
    {
        ExaminationPanel.SetActive(true);
        childUI.InitializeChild();
        ProceedToRecipeButton.interactable = false;
    }

    public void DisplayRecipe()
    {
        RecipePanel.SetActive(true);
        recipeUI.Initialize();
    }

    public void StartCraft()
    {
        CraftingPanel.SetActive(true);
        // TODO: start crafting process
    }

    public void ShowResult()
    {
        ResultPanel.SetActive(true);
    }

    public void ShowEnding()
    {
        EndingPanel.SetActive(true);
        endingUI.InitializeEndingUI();
    }

}