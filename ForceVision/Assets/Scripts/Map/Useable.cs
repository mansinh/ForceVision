using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Useable : MonoBehaviour
{
    [SerializeField] Material onMaterial, offMaterial;
    [SerializeField] MeshRenderer meshRenderer;
    private AudioSource audioSource;
    public bool IsOn {
        get {
            return isOn;
        }
        set {
            isOn = value;
            Debug.Log("MOUSE DOWN USEABLE " + isOn);
            meshRenderer.material  = isOn ? onMaterial : offMaterial;
        }
    }
    [SerializeField] private bool isOn = false;

    void Start() {
        IsOn = isOn;
        audioSource = GetComponent<AudioSource>();
    }
    private void OnMouseUp() {
       
        IsOn = !isOn;
        if (audioSource)
            audioSource.Play();
    }
}
