using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    // All parts of a wheat object are considered to have the Wheat type, as well as their own (i.e. Head, Stem, or Leaf) type.
    public enum Part {
        Wheat = 0,
        Head = 1,
        Stem = 2,
        Leaf = 3,
        Awns = 4
    }

    // Track whether wheat objects are currently annotated (materials are simplified colors) or not
    public static bool wheatIsAnnotated = false;

    // Layers (useful for instantiation purposes)
    public static int groundLayer = 3;
    public static int wheatLayer = 8;
    public static int headLayer = 9;
    public static int stemLayer = 10;
    public static int leafLayer = 11;
    public static int awnsLayer = 12;

    // Layermasks
    public static int groundLayerMask = 1 << groundLayer; // If passed: only hit ground
    public static int awnsLayerMask = ~(1 << awnsLayer); // If passed: ignore awns

    public static Dictionary<Part, int> partToLayerDict = new Dictionary<Part, int> {
        {Part.Wheat, wheatLayer},
        {Part.Head, headLayer},
        {Part.Stem, stemLayer},
        {Part.Leaf, leafLayer},
        {Part.Awns, awnsLayer}
    };

    public static Dictionary<string, Part> nameToPartDict = new Dictionary<string, Part> {
        {"Wheat", Part.Wheat},
        {"Head", Part.Head},
        {"Stem", Part.Stem},
        {"Leaf", Part.Leaf},
        {"Awns", Part.Awns}
    };

    public static Dictionary<Part, string> partToNameDict = new Dictionary<Part, string>(){
        {Part.Wheat, "Wheat"},
        {Part.Head, "Head"},
        {Part.Leaf, "Leaf"},
        {Part.Awns, "Awns"},
        {Part.Stem, "Stem"}
    };

    public static Dictionary<Part, int> partToIDDict = new Dictionary<Part, int>(){
        {Part.Head, 1},
        {Part.Leaf, 2},
        {Part.Stem, 3},
        {Part.Awns, 4}
    };

    public static Part[] GetPartTypesToLabel(){
        return new Part[]{Part.Head, Part.Leaf, Part.Awns, Part.Stem};
    }

    public static string GetPartName(Wheat.Part part){
        return partToNameDict[part];
    }

    // Check if an obj is a wheat. If there is a second argument, also check if the obj is of the same part as the parameter.
    public static bool IsWheat(GameObject obj, Part part = Part.Wheat){
        WheatData wheatData = obj.GetComponent<WheatData>();
        if (wheatData){
            // If the user doesn't specify the part, but the object has a wheatData script, it is a wheat
            if (part == Part.Wheat){
                return true;
            }

            // If the user does specify the part, the wheatData.part must be the same, or return false
            if (part.Equals(wheatData.part)){
                return true;
            }
        } 
        return false;
    }


    // Returns all Wheat objects in the scene
    public static HashSet<GameObject> getAllWheats(){
        HashSet<GameObject> wheats = new HashSet<GameObject>();
        foreach (GameObject obj in FindObjectsOfType<GameObject>()){
            if (IsWheat(obj)){
                wheats.Add(obj);
            }
        }
        return wheats;
    }
}
