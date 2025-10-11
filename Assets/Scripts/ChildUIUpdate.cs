using UnityEngine;
using TMPro;
public class ChildUIUpdate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _speechText;
    [SerializeField] private TextMeshProUGUI _desireText;
    [SerializeField] private TextMeshProUGUI _insecurityText;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private GameObject _desireBubble;
    [SerializeField] private GameObject _insecurityBubble;

    public void InitializeChild()
    {
        ChildProfile currentChild = GameManager.Instance.GetCurrentChild();
        _speechText.text = currentChild.greeting;
        _desireText.text = currentChild.desire;
        _insecurityText.text = currentChild.insecurity;
    }

    public void OnExamined()
    {
        _speechBubble.SetActive(false);
        _desireBubble.SetActive(true);
        _insecurityBubble.SetActive(true);
    }

    public void OnNormal()
    {
        _speechBubble.SetActive(true);
        _desireBubble.SetActive(false);
        _insecurityBubble.SetActive(false);
    }
}
