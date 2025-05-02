using System.IO;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using UnityEngine;
using UnityEditor.Build;
using System.Linq;


[Serializable]
public class Schedule {
    public List<Field> fields = new List<Field>();
    public List<Event> events = new List<Event>();
    public List<Domain> domains = new List<Domain>();

    public void add(object item) {
        if (item is Event event_){
            events.Add(event_);
        }
        else if (item is Field field){
            fields.Add(field);
        }
        else if (item is Domain domain){
            domains.Add(domain);

            // Check that event exists
            List<string> eventNames = events.Select(ev => ev.name).ToList();
            foreach (string domainEventName in domain.eventNames){
                if (!eventNames.Contains(domainEventName)){
                    Debug.LogError($"Possible error: Could not find event by name '{domainEventName}' in existing event names: '{eventNames}'");
                }
            }
            
            // Check that field exists
            List<string> fieldNames = fields.Select(field => field.name).ToList();
            if (!fieldNames.Contains(domain.fieldName)){
                Debug.LogError($"Possible error: Could not find field by name '{domain.fieldName}' in existing field names: '{fieldNames}'");
            }
        }
    }
}

[Serializable]
public class Field {
    public string name;
    public PositionFinder.FieldLayout layout;
    public List<WeedHandler.WeedType> weedTypes;
    public int wheatCount;
    public int underbrushCount;
    public int weedCount;
    public int wheatPlaceAttempts;

    // Inputs
    public Field(
        string name,
        PositionFinder.FieldLayout layout  = PositionFinder.FieldLayout.Uniform, 
        List<WeedHandler.WeedType> weedTypes = null,
        int wheatCount          = 2000, 
        int underbrushCount     = 20000, 
        int weedCount           = 1000,
        int wheatPlaceAttempts  = 5
        )
        {
            this.name = name;
            this.layout = layout;
            this.weedTypes = weedTypes;
            this.wheatCount = wheatCount;
            this.underbrushCount = underbrushCount;
            this.weedCount = weedCount;
            this.wheatPlaceAttempts = wheatPlaceAttempts;
        }

    public void Build(){
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // Clear previous layout
        ObjectPooler.ClearAllPools();

        // Get wheat prefabs directory. If not found, set wheat count to 0.
        string wheatPrefabsDirectory = GetPrefabDirectory("Wheat Models", name);
        if(wheatPrefabsDirectory == null){
            Debug.Log("Wheat prefab directory not found at " + wheatPrefabsDirectory);
            wheatCount = 0;
        }
        // Instantiate wheat
        if (wheatCount > 0){
            ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Wheat, wheatPrefabsDirectory, wheatCount);
            InstantiateWheat instantiateWheat = Object.FindObjectOfType<InstantiateWheat>();
            instantiateWheat.LoopAddWheat(wheatCount, layout, wheatPlaceAttempts);
        }

        // Get underbrush prefabs directory. If not found, set underbrush count to 0.
        string underbrushPrefabsDirectory = GetPrefabDirectory("Ground Cover Models", name);
        if(underbrushPrefabsDirectory == null){ // Need to do something similar for weeds
            Debug.Log("Underbrush prefab directory not found at " + underbrushPrefabsDirectory);
            underbrushCount = 0;
        }
        // Instantiate underbrush
        if (underbrushCount > 0){
            ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Underbrush, underbrushPrefabsDirectory, underbrushCount);
            UnderbrushHandler underbrushHandler = Object.FindObjectOfType<UnderbrushHandler>();
            underbrushHandler.LoopInstantiateUnderbrushInBounds(underbrushCount, layout);
        }

        // Create the weed prefabs pool from the given WeedTypes parameter
        WeedHandler weedHandler = Object.FindObjectOfType<WeedHandler>();
        if (weedTypes != null && weedCount > 0){
            ObjectPooler.InitializePools(ObjectPooler.PoolType.Weeds, weedHandler.GetAvailableWeeds(weedTypes).ToArray(), weedCount);
            weedHandler.LoopInstantiateWeedsInBounds(weedCount);
        }

        sw.Stop();
        Debug.Log($"Time to load new domain: {sw.ElapsedMilliseconds} ms");
    }

    static private string GetPrefabDirectory(string prefabType, string fieldName){
        string relativePath = $"Assets/Resources/Prefabs/{prefabType}/{fieldName}";
        string fullPath = Path.GetFullPath(relativePath);
        if (Directory.Exists(fullPath))
            return fullPath;
        else {
            Debug.LogError("Could not find the domain path.");
            return null;
        }
    }
}


[Serializable]
public abstract class Event {
    public string name;
    public int frequency;
    public float timeToExecute;

    // Inputs
    public Event(string name, int frequency, float timeToExecute) 
    {
        this.name = name;
        this.frequency = frequency;
        this.timeToExecute = timeToExecute;
    }

    // Returns the time to execute if the event occurs on this iteration
    public float RunEventForIteration(int iteration){
        if (iteration % frequency == 0){
            RunEvent();
            Debug.Log("Ran event: " + name);
            return timeToExecute;
        }
        return 0f;
    }

    public abstract void RunEvent();
}

[Serializable]
public class SwapLightSourceEvent : Event {
    public List<LightSourceHandler.LightsourceType> lightsourceTypes;

    public SwapLightSourceEvent(string name, int frequency, float timeToExecute, List<LightSourceHandler.LightsourceType> lightsourceTypes)
    : base (name, frequency, timeToExecute)
    {
        this.lightsourceTypes = lightsourceTypes;
    }

    public override void RunEvent(){
        LightSourceHandler lightSourceHandler = Object.FindObjectOfType<LightSourceHandler>();
        lightSourceHandler.SwapLightSource(lightsourceTypes);
    }
}

[Serializable]
public class SwapGroundTextureEvent : Event {
    public List<TerrainHandler.TerrainType> terrainTypes;

    public SwapGroundTextureEvent(string name, int frequency, float timeToExecute, List<TerrainHandler.TerrainType> terrainTypes)
    : base (name, frequency, timeToExecute)
    {
        this.terrainTypes = terrainTypes;
    }

    public override void RunEvent(){
        TerrainHandler terrainHandler = Object.FindObjectOfType<TerrainHandler>();
        terrainHandler.SwapGroundTextures(terrainTypes);
    }
}

[Serializable]
public class MoveGroundEvent : Event {
    public int maxMoveDistance;

    public MoveGroundEvent(string name, int frequency, float timeToExecute, int maxMoveDistance)
    : base (name, frequency, timeToExecute)
    {
        this.maxMoveDistance = maxMoveDistance;
    }

    public override void RunEvent(){
        TerrainHandler terrainHandler = Object.FindObjectOfType<TerrainHandler>();
        terrainHandler.MoveTerrainPosition(maxMoveDistance);
    }
}

[Serializable]
public class Domain {
    public string name;
    public string fieldName;
    public List<string> eventNames;
    public int imagesLimit; // -1 represents no limit
    public int minutesLimit; // -1 represents no limit

    public Domain(string name, string fieldName, List<string> eventNames, int imagesLimit = -1, int minutesLimit = -1) 
    {
        this.name = name;
        this.fieldName = fieldName;
        this.eventNames = eventNames;
        this.imagesLimit = imagesLimit;
        this.minutesLimit = minutesLimit;
    }
}