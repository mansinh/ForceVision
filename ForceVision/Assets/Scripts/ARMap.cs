using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARMap : MonoBehaviour
{
    [SerializeField] MeshRenderer renderer;
    // Start is called before the first frame update
    public void SetTexture(Texture2D texture) {
        renderer.material.mainTexture = texture;
    }
}
