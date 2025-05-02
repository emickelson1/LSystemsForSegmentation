using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.UIElements;

public class PlaceAtCursor : MonoBehaviour
{
    // // Variables to handle the position of objects that are being placed
    // private Vector3 mouseScreenPosition;
    // private Vector3 mouseWorldPosition;

    // //Scripts
    // [SerializeField] private UnderbrushHandler ubHandler;


    // // Camera that is in use
    // [SerializeField] private Camera cam;

    // // Variables relating to the objects and number of objects that are being placed
    // [SerializeField] private int amountOfWheatToPlace;
    // [SerializeField] private int amountOfUnderbrushToPlace;
    // [SerializeField] float acceptableDistance;

    // public void PlaceObjectsAtCursor(){
    //     // https://forum.unity.com/threads/help-on-object-attached-to-mouse.167316/
    //     // Do a raycast to the position of the mouse in the 3D space
    //     mouseScreenPosition = Input.mousePosition;
    //     mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, cam.nearClipPlane+1));
    //     Ray ray = new Ray(origin: cam.transform.position, direction: (mouseWorldPosition - cam.transform.position).normalized);
    //     RaycastHit hit;

    //     // If the raycast hits ground, place an object at the mouse space, or place objects in random positions within the acceptable distance around the mouse.
    //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, Wheat.groundLayerMask)){
    //         if (amountOfWheatToPlace > 0){
    //             for (int i = 0; i < amountOfWheatToPlace; i++){
    //                 InstantiateWheat.IW.GenerateWheat(getPositionNearMousePosition(hit.point), true);
    //             }
    //         }
            
    //         if (amountOfUnderbrushToPlace > 0){
    //             for (int i = 0; i < amountOfUnderbrushToPlace; i++){
    //                 Vector3 approximatePosition = getPositionNearMousePosition(hit.point);
    //                 Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);
    //                 ubHandler.InstantiateUnderbrush(approximatePosition, rotation);
    //             }
                
    //         }
    //     }
    // }

    // public Vector3 getPositionNearMousePosition(Vector3 mousePosition){
    //     if (amountOfWheatToPlace <= 1 && amountOfUnderbrushToPlace <= 1){
    //         return mousePosition;
    //     }

    //     // If the user is trying to place >1 object, the objects will be placed within a certain distance of the mouse, rather than on top of it
    //     Vector3 positionDeviation = new Vector3(0f,0f,0f);
    //     if (amountOfWheatToPlace > 1 || amountOfUnderbrushToPlace > 1){
    //         positionDeviation = new Vector3(
    //             UnityEngine.Random.Range(-acceptableDistance, acceptableDistance),
    //             0f,
    //             UnityEngine.Random.Range(-acceptableDistance, acceptableDistance)
    //         );
    //     }

    //     return mousePosition + positionDeviation;
    // }
}
