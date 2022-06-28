using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
    public bool IsOpen {
        get {
            return isOpen;
        }
        set {
            isOpen = value;
            foreach (GameObject affectedWall in affectedWalls) {
                affectedWall.SetActive(!isOpen);
            }
        }
    }
    [SerializeField] private bool isOpen = false;
    [SerializeField] Transform doorContainer;
    [SerializeField] GameObject[] affectedWalls;
    private AudioSource audioSource;
void Start() {
        IsOpen = isOpen;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (!isOpen && doorContainer.transform.localScale.z < 1) {
            doorContainer.transform.localScale += 6 * Vector3.forward * Time.deltaTime;
        }

        if (isOpen && doorContainer.transform.localScale.z > 0) {
            doorContainer.transform.localScale -= 6 * Vector3.forward * Time.deltaTime;
        } else if (doorContainer.transform.localScale.z < 0) {
            doorContainer.transform.localScale = new Vector3(1, 1, 0);
        }
    }
    private void OnMouseDown() {
        IsOpen = !IsOpen;
        if (audioSource)
            audioSource.Play();
    }
}
