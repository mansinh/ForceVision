using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoController : MonoBehaviour {
    [SerializeField] Button previous, next;
    [SerializeField] Image photo;
    [SerializeField] Sprite[] photos;
   
    int index = 0;
    void Start() {
        previous.onClick.AddListener(() => {
            Switch(-1);
        });
        next.onClick.AddListener(() => {
            Switch(1);
        });
        photo.sprite = photos[index];
    }

    private void Switch(int direction) {
        index += direction;
        index = index % photos.Length;
        index = Mathf.Abs(index);
        photo.sprite = photos[index];
    }

    // Update is called once per frame
    void Update() {

    }
}
