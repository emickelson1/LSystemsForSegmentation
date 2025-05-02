using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthBoundariesGizmo : MonoBehaviour
{
    // Boundaries of wheat growth
    public GameObject boundary1;
    public GameObject boundary2;
    
    // Draw a cube between the boundaries
    void OnDrawGizmosSelected(){
        Vector3 pos1 = boundary1.GetComponent<Transform>().position;
        Vector3 pos2 = boundary2.GetComponent<Transform>().position;
        Vector3 difference = pos2-pos1;

        Vector3 center = pos1+difference/2;
        Vector3 scale = new Vector3(Mathf.Abs(difference.x), Mathf.Abs(difference.y), Mathf.Abs(difference.z));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, scale);
    }
}
