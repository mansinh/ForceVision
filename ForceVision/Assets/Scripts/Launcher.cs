using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;



public class Launcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if PLATFORM_ANDROID
            if(!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif
        SceneManager.LoadScene("Main");
    }

}
