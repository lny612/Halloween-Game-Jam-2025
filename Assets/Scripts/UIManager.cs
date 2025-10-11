using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject ArrivalPanel;
    [SerializeField] private GameObject ExaminationPanel;
    [SerializeField] private GameObject RecipePanel;
    [SerializeField] private GameObject CraftingPanel;
    [SerializeField] private GameObject ResultPanel;

    [Header("Prefabs")]
    [SerializeField] private GameObject ChildPrefab;
    [SerializeField] private ChildUIUpdate childUI;
    [SerializeField] private RecipeUI RecipeUI;

    [Header("Buttons")]
    [SerializeField] private Button ProceedToExamineButton;
    [SerializeField] private Button ProceedToRecipeButton;
    [SerializeField] private Button ProceedToCraftingButton;
    [SerializeField] private Button ProceedToResultButton;
    [SerializeField] private Button ProceedToArrivalButton;


    public static UIManager Instance { get; private set; }
    public GameObject door, recipes, craft, result;
    public TextMeshProUGUI doorText, examineText, resultText;
    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;
    private bool isExaminePressed;

    System.Action onDoorContinue, onExamineContinue, onResultNext;
    System.Action<RecipeDefinition> onPickRecipe;

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
        ProceedToCraftingButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Craft));
        ProceedToResultButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(LoopState.Result));
        //ProceedToArrivalButton.onClick.AddListener(() => OnContinueClicked(LoopState.Examine));
    }

    public void ShowDoor()
    {
        // TODO: show arrival panel and play animation
        // temp
        // TODO: door must be opened when there's a show door animation - trick or treat text display - player opens the door
        ArrivalPanel.SetActive(true);
    }

    public void OnExamineButtonPressed()
    {
        isExaminePressed = !isExaminePressed;

        if (isExaminePressed)
        {
            childUI.OnNormal();
        }
        else
        {
            childUI.OnExamined();
        }
    }

    public void ShowVisitor()
    {
        ExaminationPanel.SetActive(true);
        childUI.InitializeChild();
    }

    public void DisplayRecipe()
    {
        RecipePanel.SetActive(true);
        RecipeUI.Initialize();
    }

    public void StartCraft()
    {
        CraftingPanel.SetActive(true);
        // TODO: start crafting process
    }

}