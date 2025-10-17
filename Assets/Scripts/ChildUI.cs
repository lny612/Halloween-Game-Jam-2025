using UnityEngine;
using TMPro;
public class ChildUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _speechText;
    [SerializeField] private TextMeshProUGUI _desireText;
    [SerializeField] private TextMeshProUGUI _insecurityText;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private GameObject _desireBubble;
    [SerializeField] private GameObject _insecurityBubble;

    [SerializeField] private GameObject DesireParticle;
    [SerializeField] private GameObject InsecurityParticle;

    [Header("Streaming (auto-filled if left null)")]
    private StreamingDialogue _speechStreamer;
    private ChildProfile currentChild;
    private bool isDesireExamined = false;
    private bool isInsecurityExamined = false;

    void Awake()
    {
        // If you didn’t drag the streamers in the Inspector, auto-get them from the same objects as the TMP texts
        if (!_speechStreamer && _speechText) _speechStreamer = _speechText.GetComponent<StreamingDialogue>();
    }

    public void InitializeChild()
    {
        currentChild = GameManager.Instance.GetCurrentChild();
        _speechText.text = currentChild.greeting;
        _desireText.text = currentChild.desire;
        _insecurityText.text = currentChild.insecurity;

        _speechStreamer.PlayLine(currentChild.greeting);

    }

    public void OnExamineButtonPressed()
    {
        _speechBubble.SetActive(false);

        // particles set to active
        DesireParticle.SetActive(true);
        var particleSystemDesire = DesireParticle.GetComponent<ParticleSystem>();
        particleSystemDesire.Play(true);

        InsecurityParticle.SetActive(true);
        var particleSystemInsecurity = InsecurityParticle.GetComponent<ParticleSystem>();
        particleSystemInsecurity.Play(true);
    }

    public void OnHoverDesireEnter()
    {
        _desireBubble.SetActive(true);
        isDesireExamined = true;
        TurnOnCraftButton();
    }

    public void OnHoverDesireExit()
    {
        _desireBubble.SetActive(false);
    }

    // Called by hover dots
    public void OnHoverInsecurityEnter()
    {
        _insecurityBubble.SetActive(true);
        isInsecurityExamined = true;
        TurnOnCraftButton();
    }
    public void OnHoverInsecurityExit()
    {
        _insecurityBubble.SetActive(false);
    }

    public void TurnOnCraftButton()
    {
        if(isDesireExamined && isInsecurityExamined)
        {
            UIManager.Instance.EnableProceedToRecipeButton();
        }
    }

    
}
