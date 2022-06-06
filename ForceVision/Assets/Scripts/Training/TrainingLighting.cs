using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TrainingLighting : MonoBehaviour
{
    [SerializeField] Light lightSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        RenderSettings.skybox.SetFloat("_Exposure", lightSource.intensity*0.4f);
        
    }
}
