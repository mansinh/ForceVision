using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeOut : MonoBehaviour
{
    float duration = 3;
    float alpha = 1;
    Image image;
    // Start is called before the first frame update
    void Start() {
       
    }
    
    public void OnEnable() {
        image = GetComponent<Image>();
        image.color = Color.red;
        alpha = 1;
    }

    // Update is called once per frame
    void Update()
    {
        alpha -= Time.deltaTime/duration;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
        if (alpha < 0) {
            gameObject.SetActive(false);
        }
    }
}
