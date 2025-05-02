using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabsFromMeshes : MonoBehaviour
{
    static private string wheatMeshPath = "Assets/Resources/Meshes/Wheat Models";
    static private string wheatPrefabPath = "Assets/Resources/Prefabs/Wheat Models/";
    static private string gcMeshPath = "Assets/Resources/Meshes/Ground Cover Models";
    static private string gcPrefabPath = "Assets/Resources/Prefabs/Ground Cover Models/";

    private void Start()
    {
        DeleteAllFilesAndDirectories(wheatPrefabPath);
        DeleteAllFilesAndDirectories(gcPrefabPath);

        ConvertMeshesToPrefabs(Path.GetFullPath(wheatMeshPath), true, 0.2f);    
        ConvertMeshesToPrefabs(Path.GetFullPath(gcMeshPath), false, 20f);    
    }

    private void ConvertMeshesToPrefabs(string fullMeshPath, bool isWheat, float scaleModifier){
        string[] meshDirectories = Directory.GetDirectories(fullMeshPath);
        foreach (string meshDirectory in meshDirectories){
            string relativeMeshPath = meshDirectory.Replace("\\", "/").Split(new[] { "Assets/Resources/" }, System.StringSplitOptions.None)[1];
            GameObject[] meshes = Resources.LoadAll<GameObject>(relativeMeshPath);

            foreach (GameObject mesh in meshes){
                // Instantiate the model prefab
                GameObject instantiatedModel = Instantiate(mesh);

                if (isWheat){
                    PrepareWheatPrefab(instantiatedModel, scaleModifier);
                }
                else 
                {
                    PrepareNonWheatPrefab(instantiatedModel, scaleModifier);
                }

                // Save the modified prefab
                string newDirectory = meshDirectory.Replace("Meshes", "Prefabs");
                if (!Directory.Exists(newDirectory)){
                    Directory.CreateDirectory(newDirectory);
                }

                PrefabUtility.SaveAsPrefabAsset(instantiatedModel, $"{newDirectory}/{mesh.name}.prefab");

                // Destroy the instantiated model
                Destroy(instantiatedModel);
            }
        }
    }

    private void PrepareWheatPrefab(GameObject instantiatedModel, float scaleModifier){
        // Loop through each child object and add components
        foreach (Transform childTransform in instantiatedModel.GetComponentsInChildren<Transform>(true))
        {
            // Check if the child object is not the root object
            if (childTransform != instantiatedModel.transform)
            {
                // Add components to the child object
                childTransform.gameObject.AddComponent<WheatData>();
                MeshCollider mc = childTransform.gameObject.AddComponent<MeshCollider>();
                childTransform.gameObject.layer = Wheat.wheatLayer;

                // Assign it the suitable layer
                foreach (string partName in Wheat.nameToPartDict.Keys){
                    if (childTransform.gameObject.name.Contains(partName)){
                        Wheat.Part part = Wheat.nameToPartDict[partName];
                        int layer = Wheat.partToLayerDict[part];
                        childTransform.gameObject.layer = layer;
                    }
                }

                // Assign a suitable tag
                foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags){
                    if (childTransform.name.ToLower().Contains(tag.ToLower())){
                        childTransform.tag = tag;
                    }
                }
            }
        }

        // Set layer to "Wheat"
        instantiatedModel.layer = Wheat.wheatLayer;

        // Adjust scale
        instantiatedModel.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);
    }
    
    private void PrepareNonWheatPrefab(GameObject instantiatedModel, float scaleModifier){
        Transform transform = instantiatedModel.transform;

        // Adjust scale
        transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

        // If a tag is found 
        string name = transform.name;
        foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags){
            if (name.ToLower().Contains(tag.ToLower())){
                transform.tag = tag;
            }
        }
    }

    // Used to delete all the prefabs that do not have a corresponding mesh
    public static void DeleteAllFilesAndDirectories(string path)
    {
        if (Directory.Exists(path))
        {
            // Delete all files in the directory
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    // Debug.Log($"Deleted file: {file}");
                }
                catch (IOException ioExp)
                {
                    Debug.LogError($"Error deleting file: {file}, {ioExp.Message}");
                }
            }

            // Recursively delete all directories in the directory
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                try
                {
                    Directory.Delete(directory, true);
                    // Debug.Log($"Deleted directory: {directory}");
                }
                catch (IOException ioExp)
                {
                    Debug.LogError($"Error deleting directory: {directory}, {ioExp.Message}");
                }
            }

            AssetDatabase.Refresh();
            // Debug.Log("All files and directories in the directory have been deleted.");
        }
        else
        {
            // Debug.LogError("The specified directory does not exist.");
        }
    }
}
