using System.Collections;
using UnityEngine;

public class TutorialCutscene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject wizardTextBubble;
    [SerializeField] private StreamingDialogue anchorTextStreamer;
    [SerializeField] private StreamingDialogue wizardTextStreamer;

    [Header("Panels")]
    [SerializeField] private GameObject tvPanel;   // Set active at start of cutscene

    [Header("Dialogue Lines")]
    [TextArea]
    [SerializeField]
    private string anchorLine =
        "It’s Halloween night! Kids in costumes are out trick-or-treating — share the sweetness!";
    [TextArea]
    [SerializeField]
    private string wizardLine =
        "I should go check out the door.";

    [Header("Timings")]
    [SerializeField] private float preKnockDelay = 0.05f;     // wait after anchor finishes typing
    [SerializeField] private float afterKnockDelay = 0.5f;   // wait after knock before wizard speaks
    [SerializeField] private float afterWizardDelay = 0.5f;  // buffer before switching to Arrival

    public void PlayCutscene()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // 1) Show TV panel
        if (tvPanel) tvPanel.SetActive(true);

        // 2) Anchor speaks and we WAIT until the line finishes TYPING (no need for user advance)
        anchorTextStreamer.PlayLine(anchorLine);
        yield return StartCoroutine(WaitForTypeComplete(anchorTextStreamer));

        // 3) Wait pre-knock delay
        yield return new WaitForSeconds(preKnockDelay);

        // 4) Knock SFX
        SoundManager.Instance.PlaySfx(Sfx.Knock);

        // Optional small pause after knock
        yield return new WaitForSeconds(afterKnockDelay);

        // 5) Wizard speaks (show bubble, then stream) and WAIT until typing is done
        if (wizardTextBubble) wizardTextBubble.SetActive(true);
        wizardTextStreamer.PlayLine(wizardLine);
        yield return StartCoroutine(WaitForTypeComplete(wizardTextStreamer));

        // Optional buffer after wizard line
        yield return new WaitForSeconds(afterWizardDelay);

        // 6) Switch to Arrival
        GameManager.Instance.ChangeGameState(LoopState.Arrival);

    }

    private IEnumerator WaitForTypeComplete(StreamingDialogue streamer)
    {
        bool done = false;
        System.Action handler = () => done = true;

        streamer.OnTypeComplete += handler;
        // If the line happened to complete in the same frame, we still yield once.
        while (!done) yield return null;
        streamer.OnTypeComplete -= handler;
    }
}
