using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RotateToCamera : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 toCamera = Camera.main.transform.position - transform.position;
       
        transform.eulerAngles = new Vector3(0,Mathf.Atan2(toCamera.x,toCamera.z)*Mathf.Rad2Deg,0);
    }
}
