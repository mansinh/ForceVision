using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RotateToCamera : MonoBehaviour
{
    [SerializeField] bool isPolar = true;
    void LateUpdate()
    {
        //Vector3 toCamera = Camera.main.transform.position - transform.position;
        Vector3 toCamera = Camera.main.transform.forward;
        if (isPolar) {
            transform.eulerAngles = new Vector3(0, Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg, 0);
        } else { 
            transform.forward = toCamera;
        }

    }
}
