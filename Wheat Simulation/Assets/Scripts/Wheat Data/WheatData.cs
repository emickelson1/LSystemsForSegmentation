using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WheatData : MonoBehaviour
{
    // Variables are determined whenever they are requested, rather than being set on start, to prevent issues with model-->prefab conversion.

    // The part can be Stem, Head, Leaf, or generic Wheat
    public Wheat.Part part;

    public Material originalMaterial
    {
        get { return GetSlightlyRecoloredMaterial(gameObject.transform.GetComponent<Renderer>().material); }
    }

    public String materialName {
        get { return originalMaterial.name; }
    }
    
    private void Start(){
        // Update the material to a new material that is slightly darker or lighter
        gameObject.transform.GetComponent<Renderer>().material = originalMaterial;

        foreach (Wheat.Part enumPart in Enum.GetValues(typeof(Wheat.Part))){
            if (materialName.Contains(enumPart.ToString())){
                part = enumPart;
            }
        }
    }

    private Material GetSlightlyRecoloredMaterial(Material material){
        float red = material.color.r;
        float green = material.color.g;
        float blue = material.color.b;

        float delta = 0.03f;
        red *= UnityEngine.Random.Range(1f-delta, 1f+delta);
        green *= UnityEngine.Random.Range(1f-delta, 1f+delta);
        blue *= UnityEngine.Random.Range(1f-delta, 1f+delta);

        Material newMaterial = new Material(material);
        newMaterial.color = new Color(red, green, blue);
        return newMaterial;
    }

    // Collision handling (to prevent overlap)
    public bool IsOverlappingWheat(){
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Vector3 center = meshCollider.bounds.center;
        Vector3 halfExtents = meshCollider.bounds.extents;
        Collider[] colliders = Physics.OverlapBox(center, halfExtents);

        foreach (Collider collider in colliders){
            GameObject obj = collider.gameObject;
            if (obj != this.gameObject && obj.transform.parent != gameObject.transform.parent){
                if (Wheat.IsWheat(obj, Wheat.Part.Head)){
                    return true;
                }
            }
        }
        return false;
    }
}
