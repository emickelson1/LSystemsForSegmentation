using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Unity.VisualScripting;

public class DirectoryManager : MonoBehaviour {
    // Inspector variables
    public profileChoice profileChoiceDropdown = profileChoice.Laptop;
    public string currentDatasetName;
    public string currentScheduleName;

    // Public, not visible in inspector
    public Profile currentProfile => getCurrentProfile();
    public bool useInstanceSegmentation =>  modeChoiceDropdown == modeChoice.Instance ? true : false;
    public string currentDatasetDirectory => currentProfile.datasetDirectory;
    public string currentPythonPath => currentProfile.pythonPath;


    private List<Profile> profiles = new List<Profile>();
    public enum profileChoice
    {
        PC, 
        Laptop
    };

    // Determines whether image labels should be saved in instance or semantic segmentation format.
    public modeChoice modeChoiceDropdown = modeChoice.Instance;
    public enum modeChoice
    {
        Semantic, 
        Instance
    };
    
    private Profile getCurrentProfile(){
        foreach (Profile profile in profiles){
            if (profile.key == profileChoiceDropdown){
                return profile;
            }
        }
        Debug.LogError("Error: Could not find profile.");
        return null;
    }

    // Define profiles
    void Start(){
        profiles.Add(
            new Profile(profileChoice.Laptop, 
            @"C:\Users\xSkul\OneDrive\Documents\Projects\Wheat\Datasets", 
            @"C:\Users\xSkul\AppData\Local\Programs\Python\Python311\python.exe"));
        profiles.Add(
            new Profile(profileChoice.PC, 
            @"K:\Users\Skull\Downloads\tempDatasets", 
            @"C:\Users\Skull\AppData\Local\Programs\Python\Python311\python.exe"));
    }
}

// Each Profile contains several directories that need to be referenced. This is mostly useful when working from multiple devices.
public class Profile {
    public Profile(DirectoryManager.profileChoice key, string datasetDirectory, string pythonPath){
        this.key = key;
        this.datasetDirectory = datasetDirectory;
        this.pythonPath = pythonPath;
    }

    public DirectoryManager.profileChoice key;
    public string datasetDirectory;
    public string pythonPath;
}