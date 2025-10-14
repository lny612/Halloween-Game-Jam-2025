using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepSlotUI : MonoBehaviour
{
    public Image icon;
    public Image timeFill;
    public GameObject tick;
    public GameObject cross;
    public TextMeshProUGUI label; // optional

    public void Setup(Sprite stepIcon, string stepName, float timeLimit)
    {
        if (icon) icon.sprite = stepIcon;
        if (label) label.text = stepName;
        ShowNeutral();
        SetFill(0f);
    }

    public void SetActive(bool on) => gameObject.SetActive(on);

    public void SetFill(float t01)
    {
        if (timeFill != null)
        {
            timeFill.type = Image.Type.Filled;
            timeFill.fillMethod = Image.FillMethod.Horizontal;
            timeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            timeFill.fillAmount = Mathf.Clamp01(t01);
        }
    }

    public void ShowTick()
    {
        if (tick) tick.SetActive(true);
        if (cross) cross.SetActive(false);
    }

    public void ShowCross()
    {
        if (tick) tick.SetActive(false);
        if (cross) cross.SetActive(true);
    }

    public void ShowNeutral()
    {
        if (tick) tick.SetActive(false);
        if (cross) cross.SetActive(false);
    }
}
