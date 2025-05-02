using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRefProbe : MonoBehaviour
{
    public ReflectionProbe reflectionProbe;
    public float timeInterval = 3.0f;
    float time = 0f;
    
    void Update()
    {
        time += Time.deltaTime;
        if (time >= timeInterval){
            time = 0;
            reflectionProbe.RenderProbe();
        }
    }
}
