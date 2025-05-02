using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimit : MonoBehaviour
{
    [SerializeField] private bool limitFrameRate;
    [SerializeField] private int limit;

    void Start()
    {
        if (limitFrameRate){
            Application.targetFrameRate = limit;
        } else {
            Application.targetFrameRate = -1; // unlimited
        }
    }
}
