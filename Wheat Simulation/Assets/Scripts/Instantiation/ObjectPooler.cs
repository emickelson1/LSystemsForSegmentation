using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooler : MonoBehaviour
{
    public enum PoolType {
        Wheat = 0,
        Underbrush = 1,
        Weeds = 2
    }
    
    private static Dictionary<PoolType, List<Queue<GameObject>>> poolsOfTypeDict = new Dictionary<PoolType, List<Queue<GameObject>>>();


    // Initialize pools of a given pool type for each gameobject in the PoolPrefabs array input.
    public static void InitializePools(PoolType poolType, GameObject[] poolPrefabs, int expectedInstanceCount){
        List<Queue<GameObject>> pools = new List<Queue<GameObject>>();
        poolsOfTypeDict[poolType] = pools;

        int instancesPerObject = 25; // default
        if (poolPrefabs.Length != 0){
            instancesPerObject = expectedInstanceCount / poolPrefabs.Length;
        }

        foreach (GameObject prefab in poolPrefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < instancesPerObject; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            pools.Add(objectPool);
        }
    }

    // Same as InitializePools but accepts path to the prefabs instead of a prefab array
    public static void InitializePoolsFromDirectory(PoolType poolType, string prefabsPath, int expectedInstanceCount){
        string relativePath = prefabsPath.Replace("\\", "/").Split(new[] { "Assets/Resources/" }, System.StringSplitOptions.None)[1];
        if (expectedInstanceCount > 0){
            GameObject[] poolPrefabs = Resources.LoadAll<GameObject>(relativePath);
            InitializePools(poolType, poolPrefabs, expectedInstanceCount);
        }
    }

    // // Same as InitializePools but gets prefabs from a list of prefab directory paths
    // public static void InitializePoolsFromDirectories(PoolType poolType, string[] prefabsPaths, int expectedInstanceCount){
    //     int numberOfPrefabsPaths = prefabsPaths.Length;
    //     List<GameObject> poolPrefabsList = new List<GameObject>();
    //     foreach (string prefabsPath in prefabsPaths){
    //         poolPrefabsList.AddRange(Resources.LoadAll<GameObject>(prefabsPath));
    //     }
    //     GameObject[] poolPrefabs = poolPrefabsList.ToArray();
    //     InitializePools(poolType, poolPrefabs, expectedInstanceCount / numberOfPrefabsPaths);
    // }

    public static GameObject SpawnFromPoolOfType(PoolType poolType, Vector3 position, Quaternion rotation)
    {
        List<Queue<GameObject>> pools = poolsOfTypeDict[poolType];
        Queue<GameObject> pool = pools[UnityEngine.Random.Range(0, pools.Count-1)];
        GameObject objectToSpawn = pool.Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        pool.Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public static void ClearPoolsOfType(PoolType poolType)
    {
        if (poolsOfTypeDict.ContainsKey(poolType))
        {
            List<Queue<GameObject>> pools = poolsOfTypeDict[poolType];

            foreach (Queue<GameObject> pool in pools)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            pools.Clear();
            poolsOfTypeDict.Remove(poolType);
        }
    }

    public static void ClearAllPools(){
        foreach (PoolType poolType in (PoolType[]) Enum.GetValues(typeof(PoolType))){
            ClearPoolsOfType(poolType);
        }
    }

    public static int GetNumberOfPoolsOfType(PoolType type){
        return poolsOfTypeDict[type].Count;
    }
}
