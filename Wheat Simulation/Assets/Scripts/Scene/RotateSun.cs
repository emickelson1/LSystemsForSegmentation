using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSun : MonoBehaviour
{
    public float minXRotation = 10f;
    public float maxXRotation = 170f;
    
    public bool controlsOwnOrbit = true;
    public float currentTime;
    
    void Start(){
        currentTime = 75f;
    }

    void Update()
    {
        // If the sun is controlling its own orbit, add time.deltaTime to its rotation every frame.
        if (controlsOwnOrbit){
            currentTime += Time.deltaTime;
            if (currentTime >= maxXRotation){
                currentTime = minXRotation;
            }
        }
        
        SetPosition(currentTime);
    }

    void SetPosition(float currentTime){
        float effectiveTime = currentTime;
        while (effectiveTime > maxXRotation){
            effectiveTime -= (maxXRotation - minXRotation);
        }

        this.gameObject.transform.eulerAngles = new Vector3(effectiveTime, -270f, -180f);
    }
}
