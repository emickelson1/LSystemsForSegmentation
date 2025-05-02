using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ScheduleCreator : MonoBehaviour {
    // This script is used to define a list of domains to create and how many images to take for each.
    // In the future, this should be made possible through an interface
    
    // public void BuildSchedule(){
    //     // Schedule object
    //     Schedule sc = new Schedule();

    //     // Control variables
    //     int domainCount = 5;
    //     int maxDatasetSize = 3500; // -1 is no limit
    //     int maxTimeInHours = 6; // -1 is no limit

    //     int maxImagesPerDomain = maxDatasetSize / domainCount;
    //     int minutesAvailablePerDomain = maxTimeInHours * 60 / domainCount;

    //     // Events
    //     sc.add(new SwapGroundTextureEvent("SwapToAnyGround", 15, 1.0f, TerrainHandler.GetAllTerrainTypes()));
    //     sc.add(new SwapLightSourceEvent("SwapToAnyLightsource", 25, 4f, LightSourceHandler.GetAllLightsourceTypes()));
    //     sc.add(new SwapLightSourceEvent("SwapToNonDarkLightsource", 25, 4f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim}));
    //     sc.add(new SwapLightSourceEvent("SwapToDarkLightSource", 25, 4f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark}));
    //     // ...

    //     // Fields
    //     sc.add(new Field("0a68ff4e05", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1200, 10000, 1350));
    //     sc.add(new Field("0a671b00f0", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.wildGrass}, 1200, 10000, 1350));
    //     sc.add(new Field("0de114d8b1", PositionFinder.FieldLayout.EightRows, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1000, 1000, 1000));
    //     sc.add(new Field("2a0c2644bf", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1000, 5000, 1000));
    //     // ...

    //     // Domains
    //     sc.add(new Domain("0a68ff4e05", "0a68ff4e05", new List<string>(){"SwapToAnyGround", "SwapToNonDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain));
    //     sc.add(new Domain("0a671b00f0", "0a671b00f0", new List<string>(){"SwapToAnyGround", "SwapToNonDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain));
    //     sc.add(new Domain("0de114d8b1", "0de114d8b1", new List<string>(){"SwapToAnyGround", "SwapToNonDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain));
    //     sc.add(new Domain("2a0c2644bf_Light", "2a0c2644bf", new List<string>(){"SwapToAnyGround", "SwapToNonDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain));
    //     sc.add(new Domain("2a0c2644bf_Dark", "2a0c2644bf", new List<string>(){"SwapToAnyGround", "SwapToDarkLightSource"}, maxImagesPerDomain, minutesAvailablePerDomain));
    //     // ...
        
    //     string scheduleName = FindObjectOfType<DirectoryManager>().currentScheduleName;
    //     SaveScheduleWithName(sc, scheduleName);
    // }

    public void BuildSchedule(){
        // Schedule object
        Schedule sc = new Schedule();

        // Control variables
        int domainCount = 1;
        int maxDatasetSize = 15; // -1 is no limit
        int maxTimeInHours = 1; // -1 is no limit

        int maxImagesPerDomain = maxDatasetSize / domainCount;
        int minutesAvailablePerDomain = maxTimeInHours * 60 / domainCount;

        // Events
        sc.add(new SwapGroundTextureEvent("SwapToAnyGround", 15, 1.0f, TerrainHandler.GetAllTerrainTypes()));
        sc.add(new SwapLightSourceEvent("SwapToAnyLightsource", 25, 4f, LightSourceHandler.GetAllLightsourceTypes()));
        sc.add(new SwapLightSourceEvent("SwapToNonDarkLightsource", 25, 4f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.bright, LightSourceHandler.LightsourceType.dim}));
        sc.add(new SwapLightSourceEvent("SwapToDarkLightSource", 25, 4f, new List<LightSourceHandler.LightsourceType>(){LightSourceHandler.LightsourceType.dark}));
        // ...

        // Fields
        sc.add(new Field("test", PositionFinder.FieldLayout.Uniform, new List<WeedHandler.WeedType>(){WeedHandler.WeedType.dryGrass, WeedHandler.WeedType.wildGrass}, 1200, 10000, 1350));
        // ...

        // Domains
        sc.add(new Domain("FieldTestDomain", "test", new List<string>(){"SwapToAnyGround", "SwapToNonDarkLightsource"}, maxImagesPerDomain, minutesAvailablePerDomain));
        // ...
        
        string scheduleName = FindObjectOfType<DirectoryManager>().currentScheduleName;
        SaveScheduleWithName(sc, scheduleName);
    }

    private JsonSerializerSettings settings = new JsonSerializerSettings{
        TypeNameHandling = TypeNameHandling.All
    };

    public void SaveScheduleWithName(Schedule schedule, string name){
        string jsonString = JsonConvert.SerializeObject(schedule, Formatting.Indented, settings);
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        File.WriteAllTextAsync(fullPath, jsonString);
    }
}