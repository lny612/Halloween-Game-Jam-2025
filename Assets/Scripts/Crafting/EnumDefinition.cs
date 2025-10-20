public enum LoopState { Tutorial, Arrival, Examine, SelectRecipe, Craft, Evaluate, Result, Ending }
public enum CandyGrade { Burnt, Sticky, Sweet, Deluxe, Divine }
public enum IngredientType { Water, Sugar, Essence }
public enum StepType { Stir, Add, Wait }
public enum IngredientSubtype { None, Water1, Water2, Water3, Sugar1, Sugar2, Sugar3, Essence1, Essence2, Essence3, Essence4, Essence5, Essence6}
public enum CandyName { Candy1, Candy2, Candy3, Candy4, Candy5, Candy6}

public class CraftResult
{
    public CandyName candyName;
    public CandyGrade candyGrade;
    public bool isMatching;
}