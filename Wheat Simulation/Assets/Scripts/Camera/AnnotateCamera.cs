using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnnotateCamera : MonoBehaviour
{
    Camera mainCamera;
    Camera annotateCamera;

    private void Start(){
        mainCamera = Camera.main;
        annotateCamera = gameObject.GetComponent<Camera>();
        annotateCamera.enabled = false;
    }

    public void SwapCameras(){
        // Start annotation
        if (Wheat.wheatIsAnnotated){
            mainCamera.enabled = false;
            annotateCamera.enabled = true;
            annotateCamera.transform.position = mainCamera.transform.position;
        }
        // End annotation
        else {
            mainCamera.enabled = true;
            annotateCamera.enabled = false;
            mainCamera.transform.position = annotateCamera.transform.position;
        }
    }
}
