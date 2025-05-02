using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitHandler : MonoBehaviour
{
    // Scripts that have functions to be called
    public PositionFinder positionFinder; // needed to reference bounding points

    // Main camera
    public Camera cam;

    // Variables to save and reuse when ending orbit scan mode
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    
    // Local variables for camera movement
    private Vector3 focus => GetCameraOrbitFocus();
    private Vector3[] bounds => GetCameraOrbitBounds();
    private Vector3 bound0 => bounds[0];
    private Vector3 bound1 => bounds[1];

    // Adjustable variables
    [SerializeField] private float minCameraHeight;
    [SerializeField] private float maxCameraHeight;

    void Start(){
        initialCameraPosition = cam.transform.position;
        initialCameraRotation = cam.transform.rotation;
    }

    private Vector3 GetCameraOrbitFocus(){
        Vector3 pos1 = positionFinder.bound0;
        Vector3 pos2 = positionFinder.bound1;

        float x = (pos1.x + pos2.x)/2.0f;
        float y = -Mathf.Infinity; // redefined later by raycast
        float z = (pos1.z + pos2.z)/2.0f;

        // Define the y-component of the camera's focal point as the average wheat height above the ground in the center of the generation bounds
        Vector3 rayCastHitPoint = Vector3.down;
        Ray ray = new Ray(origin: new Vector3(x, Mathf.Max(pos1.y, pos2.y), z), direction: Vector3.down);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Abs(pos1.y - pos2.y), Wheat.groundLayerMask)){
            y = hit.point.y;
        }
        y += 0.5f; // Wheat is 6 units high, but if the camera aims at the top of the wheat, it will see the background

        return new Vector3(x,y,z);
    }

    private Vector3[] GetCameraOrbitBounds(){
        Vector3 pos0 = positionFinder.bound0;
        Vector3 pos1 = positionFinder.bound1;

        Vector3[] positions = new Vector3[2]; 
        positions[0] = pos0;
        positions[1] = pos1;

        return positions;
    }

    public void ResetPosition(){
        cam.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
    }

    public void MoveCameraRandomly(){
        float x_diff = bound0.x - bound1.x;
        float z_diff = bound0.z - bound1.z;
        float pos_variance = 0.4f; // How much the camera can move around within the orbit bounds

        float x_pos = focus.x + Random.Range(-pos_variance * x_diff, pos_variance * x_diff);
        float y_pos = focus.y + Random.Range(minCameraHeight, minCameraHeight + maxCameraHeight); // Wheat is 6 units tall
        float z_pos = focus.z + Random.Range(-pos_variance * z_diff, pos_variance * z_diff);

        Vector3 cam_pos = new Vector3(x_pos, y_pos, z_pos);

        cam.transform.SetPositionAndRotation(cam_pos, cam.transform.rotation);
        cam.transform.LookAt(focus);
        
        // Lower camera angle if its angle is too high and might see background
        if (cam.transform.rotation.x < 40f){
            cam.transform.Rotate(40f - cam.transform.rotation.x, 0f, 0f);
        }
    }
}
