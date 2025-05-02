using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class InstantiateWheat : MonoBehaviour
{
    public static InstantiateWheat IW;
    
    // When the wheat is randomly generated, it will be shifted downward and rotated slightly.
    public float downTranslationMin = 0.1f;
    public float downTranslationMax = 0.75f;
    public float xRotationMax = 15f;
    public float yRotationMax = 360f;
    public float zRotationMax = 15f;

    // Parent object of the instantiated wheats
    public Transform parent;

    // UI text field that sets the amount of wheat objects to place when placing large amounts of wheat
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // Position finder
    [SerializeField] private PositionFinder positionFinder;

    void Awake(){
        if (IW != null && IW != this){
            GameObject.Destroy(IW);
        } else {
            IW = this;
        }

        DontDestroyOnLoad(this);
    }

public void TryGenerateWheat(
    Vector3 requestedPosition, 
    int remainingAttempts = 0, bool placeIfNoRemainingAttempts = true, // If remaining attempts > 0, it will retry placing if it collides, otherwise it just places the wheat.
    PositionFinder.FieldLayout retryPositionLayout = PositionFinder.FieldLayout.Uniform) 
    {
        if (ObjectPooler.GetNumberOfPoolsOfType(ObjectPooler.PoolType.Wheat) > 0)
        {
            // Get the prefab, position, and rotation
            Vector3 position = MoveDown(requestedPosition);
            Quaternion rotation = GetRandomRotation();

            // Instantiate the wheat
            GameObject newWheat = ObjectPooler.SpawnFromPoolOfType(ObjectPooler.PoolType.Wheat, position, rotation);
            newWheat.transform.SetParent(parent);

            if (remainingAttempts > 0 || !placeIfNoRemainingAttempts){
                bool overlapping = false;
                foreach (WheatData wheatData in newWheat.GetComponentsInChildren<WheatData>())
                {
                    if (wheatData.IsOverlappingWheat())
                    {
                        overlapping = true;
                        newWheat.SetActive(false); // Instead of Destroy
                        break;
                    }
                }
                if (overlapping && remainingAttempts > 0)
                {
                    TryGenerateWheat(positionFinder.GetPositionFromPattern(retryPositionLayout), 
                        remainingAttempts-1, placeIfNoRemainingAttempts, retryPositionLayout);
                }
            }
        }
        else
        {
            Debug.Log("Error: No wheat prefabs found!");
        }
    }

    private Vector3 MoveDown(Vector3 requestedPosition){
        float downTranslation = Random.Range(downTranslationMin, downTranslationMax);
        Vector3 position = requestedPosition - new Vector3(0, downTranslation, 0);
        return position;
    }

    private Quaternion GetRandomRotation(){
        float xRotation = Random.Range(-xRotationMax, xRotationMax);
        float yRotation = Random.Range(-yRotationMax, yRotationMax);
        float zRotation = Random.Range(-zRotationMax, zRotationMax);
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        return rotation;
    }

    public void LoopAddWheat(int quantity, PositionFinder.FieldLayout shape = PositionFinder.FieldLayout.Uniform, int maxPlaceAttempts = 5)
    {
        for (int i = 0; i < quantity; i++){
            // Place in a position obtained from GetPositionInWheatBounds, or retry if that position is occupied
            Vector3 position = positionFinder.GetPositionFromPattern(shape);
            TryGenerateWheat(position, maxPlaceAttempts, retryPositionLayout:shape);
        }
    }
}
