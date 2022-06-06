using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteInEditMode]
public class AlignWithView : MonoBehaviour
{
    private void Update()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return;
        transform.position = sceneView.camera.transform.position;
        transform.rotation = sceneView.camera.transform.rotation;
    }
}
