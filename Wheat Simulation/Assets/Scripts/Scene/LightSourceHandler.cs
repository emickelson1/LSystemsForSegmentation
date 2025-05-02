using System.Collections.Generic;
using UnityEngine;

public class LightSourceHandler : MonoBehaviour {
    [SerializeField] private GameObject defaultSun;
    private GameObject activeLightSource;

    [SerializeField] private List<GameObject> darkLightSources = new List<GameObject>();
    [SerializeField] private List<GameObject> dimLightSources = new List<GameObject>();
    [SerializeField] private List<GameObject> brightLightSources = new List<GameObject>();

    public enum LightsourceType {
        dark = 0,
        dim = 1,
        bright = 2
    }

    public static List<LightsourceType> GetAllLightsourceTypes(){
        return new List<LightsourceType>(){LightsourceType.dark, LightsourceType.dim, LightsourceType.bright};
    }

    private List<GameObject> GetAvailableLightsources(List<LightsourceType> availableLightsourceTypes){
        List<GameObject> availableLightsources = new List<GameObject>();
        if (availableLightsourceTypes.Contains(LightsourceType.dark)){
            availableLightsources.AddRange(darkLightSources);
        }
        if (availableLightsourceTypes.Contains(LightsourceType.dim)){
            availableLightsources.AddRange(dimLightSources);
        }
        if (availableLightsourceTypes.Contains(LightsourceType.bright)){
            availableLightsources.AddRange(brightLightSources);
        }
        return availableLightsources;
    }

    private List<GameObject> GetAllLightsources(){
        List<GameObject> availableLightsources = new List<GameObject>();

        availableLightsources.AddRange(darkLightSources);
        availableLightsources.AddRange(dimLightSources);
        availableLightsources.AddRange(brightLightSources);

        return availableLightsources;
    }

    public void SwapLightSource(List<LightsourceType> availableLightsourceTypes){
        List<GameObject> lightSources = GetAvailableLightsources(availableLightsourceTypes);
        activeLightSource = lightSources[Random.Range(0, lightSources.Count - 1)];

        // Disable all other light sources.
        DisableAllOtherLightSources(activeLightSource);

        // Set the active light source to the new light source.
        activeLightSource.SetActive(true);
    }

    public void ResetLightSource(){
        activeLightSource = defaultSun;
        DisableAllOtherLightSources(activeLightSource);
    }

    private void DisableAllOtherLightSources(GameObject exception){
        // Disable default sun
        if (exception != defaultSun){
            defaultSun.SetActive(false);
        }

        // Disable everything else
        foreach (GameObject lightSource in GetAllLightsources()){
            if (lightSource != exception){
                lightSource.SetActive(false);
            }
        }
    }
}