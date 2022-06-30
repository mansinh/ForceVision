using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
public class AboutPanel : MonoBehaviour {
    [SerializeField] private Button logoButton;
    [SerializeField] private FadeInOut aboutPanel;
    private AudioSource audioSource;
    [SerializeField] private AudioClip selectSound;
    // Start is called before the first frame update
    void Start() {
        audioSource = GetComponent<AudioSource>();
        logoButton.onClick.AddListener(() => {
            SelectSound();
            aboutPanel.ToggleFade();
        });
    }

    private void SelectSound() {
        if (selectSound)
            audioSource.PlayOneShot(selectSound);
    }
}
