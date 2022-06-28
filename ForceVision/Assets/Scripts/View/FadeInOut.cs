using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasGroup))]
public class FadeInOut : MonoBehaviour
{
    [SerializeField] private float duration;
    private float time;
    private CanvasGroup canvasGroup;
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void ToggleFade() {
        if (canvasGroup.alpha == 1) {
            TriggerFadeOut();
        } else {
            TriggerFadeIn();
        }
    }
    public void TriggerFadeIn() {
        StartCoroutine("FadeIn");
    }
    public void TriggerFadeOut() {
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeIn() {
        time = 0;
        while (time < duration) {
            canvasGroup.alpha = time / duration;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        canvasGroup.alpha = 1;
    }
    IEnumerator FadeOut() {
        time = 0;
        while (time < duration) {
            canvasGroup.alpha = 1 - time / duration;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        canvasGroup.alpha = 0;
    }
}
