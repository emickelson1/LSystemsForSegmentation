using System.Collections.Generic;
using UnityEngine;

public class WeedHandler : MonoBehaviour {
    // HashSet of all underbrush objects in the scene
    [SerializeField] private List<GameObject> dryGrass = new List<GameObject>();
    [SerializeField] private List<GameObject> wildGrass = new List<GameObject>();


    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    [SerializeField] private PositionFinder positionFinder;

    [SerializeField] private ObjectPooler objectPooler;
    
    public enum WeedType {
        dryGrass = 0,
        wildGrass = 1
    }

    public List<GameObject> GetAvailableWeeds(List<WeedType> availableWeedTypes){
        List<GameObject> availableWeeds = new List<GameObject>();
        if (availableWeedTypes.Contains(WeedType.dryGrass)){
            availableWeeds.AddRange(dryGrass);
        }
        if (availableWeedTypes.Contains(WeedType.wildGrass)){
            availableWeeds.AddRange(wildGrass);
        }
        return availableWeeds;
    }

    private List<GameObject> GetAllWeeds(){
        List<GameObject> availableWeeds = new List<GameObject>();

        availableWeeds.AddRange(dryGrass);
        availableWeeds.AddRange(wildGrass);

        return availableWeeds;
    }

    public void InstantiateWeed(Vector3 position, Quaternion rotation){
        GameObject placedUnderbrush = ObjectPooler.SpawnFromPoolOfType(ObjectPooler.PoolType.Weeds, position, rotation);
        placedUnderbrush.transform.SetParent(parentObject.transform);
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiateWeedsInBounds(int quantity)
    {
        for (int i = 0; i < quantity; i++){
            Vector3 position = positionFinder.GetPositionFromPattern(PositionFinder.FieldLayout.Uniform);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            InstantiateWeed(position, rotation);
        }
    }
}