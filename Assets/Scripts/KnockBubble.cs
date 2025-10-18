using System.Collections;
using UnityEngine;
using TMPro;

public class KnockBubble : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text label;
    public AudioSource sfx;
    public AudioClip knockClip;
    public Transform doorPos;

    [Header("Timing")]
    public int repeat = 2;
    public float flashIn = 0.08f;
    public float hold = 0.12f;
    public float flashOut = 0.1f;
    public float gap = 0.15f;

    void Awake()
    {
        group.alpha = 0f;
    }

    public void Play()
    {
        StopAllCoroutines();
        transform.position = doorPos.position;
        gameObject.SetActive(true);
        StartCoroutine(FlashSequence());
    }

    IEnumerator FlashSequence()
    {
        for (int i = 0; i < repeat; i++)
        {
            if (knockClip && sfx) sfx.PlayOneShot(knockClip);

            // fade in
            for (float t = 0; t < flashIn; t += Time.deltaTime)
            {
                group.alpha = t / flashIn;
                yield return null;
            }

            yield return new WaitForSeconds(hold);

            // fade out
            for (float t = 0; t < flashOut; t += Time.deltaTime)
            {
                group.alpha = 1 - t / flashOut;
                yield return null;
            }

            group.alpha = 0f;
            if (i < repeat - 1) yield return new WaitForSeconds(gap);
        }

        gameObject.SetActive(false);
    }
}