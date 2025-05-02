using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

public class TerrainHandler : MonoBehaviour {
    [SerializeField] private GameObject terrainObject;
    private Vector3 initialPosition;

    // Terrain layer categories
    [SerializeField] private List<TerrainLayer> darkTerrainLayers = new List<TerrainLayer>();
    [SerializeField] private List<TerrainLayer> dimTerrainLayers = new List<TerrainLayer>();
    [SerializeField] private List<TerrainLayer> brightTerrainLayers = new List<TerrainLayer>();
    [SerializeField] private List<TerrainLayer> dryTerrainLayers = new List<TerrainLayer>();
    [SerializeField] private List<TerrainLayer> wetTerrainLayers = new List<TerrainLayer>();
    

    // public enum TerrainType {
    //     dimRedCoarse = 0,
    //     darkBrownMulch = 1,
    //     brightDryMud = 2,
    //     dimBrownGrass = 3,
    //     brightDryStraw = 4
    // }

    public enum TerrainType {
        dark = 0,
        dim = 1,
        bright = 2,
        dry = 3,
        wet = 4
    }

    public static List<TerrainType> GetAllTerrainTypes(){
        return new List<TerrainType>(){TerrainType.dark, TerrainType.dim, TerrainType.bright, TerrainType.dry, TerrainType.wet};
    }

    private List<TerrainLayer> GetAvailableTerrainLayers(List<TerrainType> availableTerrainTypes){
        List<TerrainLayer> availableTerrainLayers = new List<TerrainLayer>();
        if (availableTerrainTypes.Contains(TerrainType.dark)){
            availableTerrainLayers.AddRange(darkTerrainLayers);
        }
        if (availableTerrainTypes.Contains(TerrainType.dim)){
            availableTerrainLayers.AddRange(dimTerrainLayers);
        }
        if (availableTerrainTypes.Contains(TerrainType.bright)){
            availableTerrainLayers.AddRange(brightTerrainLayers);
        }
        if (availableTerrainTypes.Contains(TerrainType.dry)){
            availableTerrainLayers.AddRange(dryTerrainLayers);
        }
        if (availableTerrainTypes.Contains(TerrainType.wet)){
            availableTerrainLayers.AddRange(wetTerrainLayers);
        }
        return availableTerrainLayers;
    }

    void Start(){
        initialPosition = terrainObject.transform.position;
    }

    public void ResetTerrainPosition(){
        terrainObject.transform.position = initialPosition;
    }

    public void MoveTerrainPosition(float maxMove){
        Vector3 deltaPosition = new Vector3(Random.Range(-maxMove, maxMove), Random.Range(-maxMove, maxMove), Random.Range(-maxMove, maxMove));
        Vector3 newPosition = initialPosition + deltaPosition;
        terrainObject.transform.position = newPosition;
    }

    // public void ChangeTexturesRandom(){
    //     // Create a new array of terrain layers
    //     TerrainLayer[] newActiveTerrainLayers = new TerrainLayer[activeTerrainLayers.Length];

    //     // Set each value of the new array to a random terrain layer in the list
    //     for(int i = 0; i < activeTerrainLayers.Length; i++){
    //         int choiceIndex = Random.Range(0, terrainLayers.Count);
    //         newActiveTerrainLayers[i] = terrainLayers[choiceIndex];
    //     }

    //     // Set the terrain layers of the terrain object to the new array.
    //     terrainObject.GetComponent<Terrain>().terrainData.terrainLayers = newActiveTerrainLayers;
    // }

    public void SwapGroundTextures(List<TerrainType> availableTerrainTypes){
        TerrainLayer[] activeTerrainLayers = terrainObject.GetComponent<Terrain>().terrainData.terrainLayers;

        List<TerrainLayer> availableTerrainLayers = GetAvailableTerrainLayers(availableTerrainTypes);
        if (availableTerrainLayers.Count > 0){
            // Create a new array of terrain layers
            TerrainLayer[] newActiveTerrainLayers = new TerrainLayer[activeTerrainLayers.Length];

            // Set each value of the new array to a random terrain layer in the list
            for(int i = 0; i < activeTerrainLayers.Length; i++){
                int choiceIndex = Random.Range(0, availableTerrainLayers.Count);
                newActiveTerrainLayers[i] = availableTerrainLayers[choiceIndex];
            }

            terrainObject.GetComponent<Terrain>().terrainData.terrainLayers = newActiveTerrainLayers;
        } else {
            Debug.LogError("No terrain layers were found.");
        }
    }
}