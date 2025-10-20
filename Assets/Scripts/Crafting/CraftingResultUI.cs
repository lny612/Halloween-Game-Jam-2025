using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingResultUI : MonoBehaviour
{
    public TextMeshProUGUI candyGrade;
    public TextMeshProUGUI candyName;
    public Image candyImage;
    public List<Sprite> gradeFrames = new List<Sprite>();
    public ParticleSystem sparkleEffect;


    public void SetResult(CandyGrade resultCandyGrade, RecipeDefinition recipeDefinition)
    {
        candyGrade.text = resultCandyGrade.ToString();
        candyName.text = recipeDefinition.recipeName;
        candyImage.sprite = recipeDefinition.recipeImage;
        SetFrame(resultCandyGrade);
        this.gameObject.SetActive(true);
        sparkleEffect.gameObject.SetActive(true);
        SoundManager.Instance.PlaySfx(Sfx.CandyDone);
        sparkleEffect.Play();

    }

    public void SetFrame(CandyGrade candyGrade)
    {
        if (candyGrade == CandyGrade.Divine)
        {
            candyImage.GetComponent<Image>().sprite = gradeFrames[4];
        }
        else if (candyGrade == CandyGrade.Deluxe)
        {
            candyImage.GetComponent<Image>().sprite = gradeFrames[3];
        }
        else if (candyGrade == CandyGrade.Sweet)
        {
            candyImage.GetComponent<Image>().sprite = gradeFrames[2];
        }
        else if (candyGrade == CandyGrade.Sticky)
        {
            candyImage.GetComponent<Image>().sprite = gradeFrames[1];
        }
        else // Burnt
        {
            candyImage.GetComponent<Image>().sprite = gradeFrames[0];
        }
    }
}
