using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UnderbrushHandler : MonoBehaviour
{
    // HashSet of all underbrush objects in the scene
    private static HashSet<GameObject> underbrushObjects = new HashSet<GameObject>();


    // Mesh to prefab conversion details
    static private string meshPath = "Assets/Resources/Meshes/Ground Cover Models";
    static private string prefabPath = "Assets/Resources/Prefabs/Ground Cover Models/";
    [SerializeField] private float scaleModifier;


    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // When placing underbrush, move it up or down by an amount within these values
    [SerializeField] private float yDifferenceMin;
    [SerializeField] private float yDifferenceMax;
    
    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    [SerializeField] private PositionFinder positionFinder;

    [SerializeField] private ObjectPooler objectPooler;


    public void InstantiateUnderbrush(Vector3 position, Quaternion rotation, bool forceGrounded){
        // Raise it to a random elevation between 0 and the yDifferenceMax, unless it is forced to be grounded (usually the case for weeds, sticks, etc.)
        if (!forceGrounded){
            position += new Vector3(0f, Random.Range(yDifferenceMin, yDifferenceMax), 0f);
        }

        // Instantiate a new object and add it to the hashset
        ObjectPooler.PoolType poolType = forceGrounded ? ObjectPooler.PoolType.Weeds : ObjectPooler.PoolType.Underbrush;

        GameObject placedUnderbrush = ObjectPooler.SpawnFromPoolOfType(ObjectPooler.PoolType.Underbrush, position, rotation);
        placedUnderbrush.transform.SetParent(parentObject.transform);

        underbrushObjects.Add(placedUnderbrush);
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiateUnderbrushInBounds(int quantity, PositionFinder.FieldLayout shape = PositionFinder.FieldLayout.Uniform, bool forceGrounded = false)
    {
        for (int i = 0; i < quantity; i++){
            Vector3 position = positionFinder.GetPositionFromPattern(shape);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            InstantiateUnderbrush(position, rotation, forceGrounded);
        }
    }

    public static HashSet<GameObject> getAllUnderbrush(){
        return underbrushObjects;
    }
}
