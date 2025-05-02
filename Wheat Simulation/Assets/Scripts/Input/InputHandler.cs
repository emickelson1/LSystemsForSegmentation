using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Scripts
    [SerializeField] private ScreenShot screenShot;
    [SerializeField] private OrbitHandler autoOrbitScan;
    [SerializeField] private UnderbrushHandler underbrushHandler;
    [SerializeField] private InstantiateWheat instantiateWheat;
    [SerializeField] private ScheduleInterpreter scheduleInterpreter;
    [SerializeField] private ScheduleCreator scheduleCreator;

    // Map KeyCodes to Actions
    Dictionary<KeyCode, Action> keyMap = new Dictionary<KeyCode, Action>();
    
    void Start(){
        // Maps keybinds to functions in other scripts
        keyMap[KeyCode.T] = TakeScreenShot;

        keyMap[KeyCode.Z] = CreateSchedule;
        keyMap[KeyCode.X] = RunSchedule;
        keyMap[KeyCode.C] = InterruptSchedule;
    }

    // Update is called once per frame
    void Update()
    {
        // Checks each KeyCode in the keyMap dictionary. If that key is pressed down, activate the corresponding function
        foreach (KeyCode keyCode in keyMap.Keys){
            if (Input.GetKeyDown(keyCode)){
                keyMap[keyCode]();
            }
        }
    }

    // Take a screen shot
    private void TakeScreenShot(){
        screenShot.TakeScreenShot();
    }

    // Run the build schedule command from ScheduleCreator
    private void CreateSchedule(){
        scheduleCreator.BuildSchedule();
    }

    // Run the schedule given in the editor input
    private void RunSchedule(){
        string scheduleName = FindObjectOfType<DirectoryManager>().currentScheduleName;
        Schedule schedule = scheduleInterpreter.LoadScheduleByName(scheduleName);
        StartCoroutine(scheduleInterpreter.InterpretSchedule(schedule));
    }

    // Try to interrupt the schedule processing
    private void InterruptSchedule(){
        scheduleInterpreter.Interrupt();
    }
}
