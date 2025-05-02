
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PositionFinder : MonoBehaviour
{
    // Boundaries of wheat growth
    [SerializeField] private GameObject bound0Object;
    [SerializeField] private GameObject bound1Object;
    [HideInInspector] public Vector3 bound0;
    [HideInInspector] public Vector3 bound1;
    
    public enum FieldLayout {
        Uniform = 0,
        EightRows = 1,
    }
    
    void Start(){
        bound0 = bound0Object.transform.position;
        bound1 = bound1Object.transform.position;
    }

    public Vector3 GetPositionFromPattern(FieldLayout shape){

        // Place the wheat randomly in a position in the field.
        if (shape == FieldLayout.Uniform){
            return GetPositionWithUniformLayout(bound0, bound1);
        } 
        else if (shape == FieldLayout.EightRows){
            return GetPositionWithEightRowLayout(bound0, bound1);
        }
        
        return new Vector3(0f, 5f, 0f); // Place everything in the center if the shape is invalid
    }

    private Vector3 GetPositionWithUniformLayout(Vector3 first, Vector3 second){
            // X and Z positions are always assumed to be valid, because ground exists everywhere
            float x = Random.Range(first.x, second.x);
            float z = Random.Range(first.z, second.z);
            float y_highest = Mathf.Max(first.y, second.y);
            float y_difference = Mathf.Abs(first.y - second.y);
            Vector3 abovePosition = new Vector3(x, y_highest, z);
            Vector3 groundPosition = GetGroundBelowPosition(abovePosition, y_difference);

            return groundPosition;
    }

    private Vector3 GetGroundBelowPosition(Vector3 originalPosition, float maxRaycastDistance){
        // The Y position must be found by casting a ray downwards, in case the ground is not level
        Ray ray = new Ray(origin: originalPosition, direction: Vector3.down);
        RaycastHit hit;

        // Should only interact with the ground layer
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, Wheat.groundLayerMask)){
            return hit.point;
        } 
        else {
            return new Vector3(0,0,0);
        }
    }


    Vector3 previousBound0;
    Vector3 previousBound1;
    Vector3[][] subBounds; //subBounds only needs to be calculated after the bounds are moved
    private Vector3 GetPositionWithEightRowLayout(Vector3 bound0, Vector3 bound1) {
        int numberOfRows = 8;
        
        if (previousBound0 != bound0 || previousBound1 != bound1){
            previousBound0 = bound0;
            previousBound1 = bound1;

            // Calculate the height and width of the main bounding box
            float rowHeight = Mathf.Abs(bound0.z - bound1.z) / (2f * numberOfRows);
            Vector3 basePosition = new Vector3(Mathf.Min(bound0.x, bound1.x), Mathf.Min(bound0.y, bound1.y), Mathf.Min(bound0.z, bound1.z));
            Vector3 endPosition = new Vector3(Mathf.Max(bound0.x, bound1.x), Mathf.Max(bound0.y, bound1.y), Mathf.Max(bound0.z, bound1.z));

            // Create sub-boxes that wheat can be spawned in
            subBounds = new Vector3[numberOfRows][];
            for (int i = 0; i < numberOfRows; i++) {
                subBounds[i] = new Vector3[2];
                subBounds[i][0] = new Vector3(basePosition.x, basePosition.y, basePosition.z + 2 * i * rowHeight);
                subBounds[i][1] = new Vector3(endPosition.x, endPosition.y, basePosition.z + (2 * i + 1) * rowHeight);
            }
        }

        // Select a random sub-bound
        int boxChoice = Random.Range(0, numberOfRows);
        Vector3[] bounds = subBounds[boxChoice];

        // Generate a position within the selected sub-bound
        return GetPositionWithUniformLayout(bounds[0], bounds[1]);
    }
}

