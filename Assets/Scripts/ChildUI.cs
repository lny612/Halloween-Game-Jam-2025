using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] private SpiritSightController spiritSightController;

    [Header("Buttons")]
    [SerializeField] private Button _inspectButton;

    [Header("Streaming (auto-filled if left null)")]
    private StreamingDialogue _speechStreamer;

    private ChildProfile currentChild;
    private bool isDesireExamined = false;
    private bool isInsecurityExamined = false;
    private bool isExamineButtonPressed = false;

    void Awake()
    {
        if (!_speechStreamer && _speechText)
            _speechStreamer = _speechText.GetComponent<StreamingDialogue>();
    }

    public void InitializeChild()
    {
        currentChild = GameManager.Instance.GetCurrentChild();

        // reset state
        isDesireExamined = false;
        isInsecurityExamined = false;
        isExamineButtonPressed = false;

        _speechBubble.SetActive(true);
        _desireBubble.SetActive(false);
        _insecurityBubble.SetActive(false);

        DesireParticle.SetActive(false);
        InsecurityParticle.SetActive(false);

        // set base text (fallbacks)
        _speechText.text = currentChild.greeting;
        _desireText.text = currentChild.desire;
        _insecurityText.text = currentChild.insecurity;

        // gate the Inspect button until streaming finishes
        if (_inspectButton) _inspectButton.interactable = false;

        if (_speechStreamer)
        {
            // avoid duplicate subscriptions if InitializeChild is called again
            _speechStreamer.OnTypeComplete -= EnableInspectOnce;
            _speechStreamer.OnTypeComplete += EnableInspectOnce;

            _speechStreamer.PlayLine(currentChild.greeting);
        }
        else
        {
            if (_inspectButton) _inspectButton.interactable = true; // fallback
        }

    }

    private void EnableInspectOnce()
    {
        if (_inspectButton) _inspectButton.interactable = true;
        // unsubscribe so it fires only once
        if (_speechStreamer) _speechStreamer.OnTypeComplete -= EnableInspectOnce;
    }

    private void OnDisable()
    {
        if (_speechStreamer) _speechStreamer.OnTypeComplete -= EnableInspectOnce;
    }

    public void OnExamineButtonPressed()
    {
        if (!isExamineButtonPressed)
        {
            isExamineButtonPressed = true;
            spiritSightController.EnterSpiritSight();
            StartCoroutine(HandleExamineSequence());
        }
        else
        {
            isExamineButtonPressed = false;
            spiritSightController.ExitSpiritSight();
            _speechBubble.SetActive(true);
            DesireParticle.SetActive(false);
            InsecurityParticle.SetActive(false);
        }
    }

    private IEnumerator HandleExamineSequence()
    {
        yield return new WaitForSeconds(0.6f);

        _speechBubble.SetActive(false);

        DesireParticle.SetActive(true);
        var particleSystemDesire = DesireParticle.GetComponent<ParticleSystem>();
        if (particleSystemDesire) particleSystemDesire.Play(true);

        InsecurityParticle.SetActive(true);
        var particleSystemInsecurity = InsecurityParticle.GetComponent<ParticleSystem>();
        if (particleSystemInsecurity) particleSystemInsecurity.Play(true);
    }

    public void OnHoverDesireEnter()
    {
        _desireBubble.SetActive(true);
        isDesireExamined = true;
        TurnOnCraftButton();
    }
    public void OnHoverDesireExit() { _desireBubble.SetActive(false); }

    public void OnHoverInsecurityEnter()
    {
        _insecurityBubble.SetActive(true);
        isInsecurityExamined = true;
        TurnOnCraftButton();
    }
    public void OnHoverInsecurityExit() { _insecurityBubble.SetActive(false); }

    public void TurnOnCraftButton()
    {
        if (isDesireExamined && isInsecurityExamined)
            UIManager.Instance.EnableProceedToRecipeButton();
    }
}
