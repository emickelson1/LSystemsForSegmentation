using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ScheduleInterpreter : MonoBehaviour {
    [HideInInspector] public Domain currentDomain;
    [SerializeField] private bool disablePythonProcessing;
    private bool interrupt = false; // Used to stop the schedule via a key input.

    // JSON deserialize settings
    private JsonSerializerSettings settings = new JsonSerializerSettings{
        TypeNameHandling = TypeNameHandling.All
    };

    // Deserialize and return the JSON with the given name
    public Schedule LoadScheduleByName(string name){
        string fullPath = Path.GetFullPath($"Assets/Schedules/{name}.json");
        if (File.Exists(fullPath)){
            string jsonText = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<Schedule>(jsonText, settings:settings);
        } else {
            Debug.LogError("JSON not found at path: " + fullPath);
            return null;
        }
    }

    public IEnumerator InterpretSchedule(Schedule schedule){
        Debug.Log("Interpreting schedule...");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        List<Field> fields = schedule.fields;
        List<Domain> domains = schedule.domains;
        List<Event> events = schedule.events;

        Field GetFieldForDomain(Domain domain){
            foreach (Field field in fields){
                if (field.name == domain.fieldName) { return field; }
            }
            Debug.LogError("Error: No field found for domain " + domain.name);
            return null;
        }

        List<Event> GetEventsForDomain(Domain domain){
            List<Event> eventsForDomain = new List<Event>();
            foreach (Event ev in events){
                if (domain.eventNames.Contains(ev.name)){
                    eventsForDomain.Add(ev);
                }
            }
            return eventsForDomain;
        }

        foreach (Domain domain in domains){
            Field fieldForDomain = GetFieldForDomain(domain);
            List<Event> eventsForDomain = GetEventsForDomain(domain);
            Debug.Log("Starting coroutine for domain " + domain.name);
            yield return StartCoroutine(InterpretDomain(domain, fieldForDomain, eventsForDomain));
        }

        sw.Stop();
        Debug.Log($"Finished generating dataset in {sw.ElapsedMilliseconds/1000f/60f} minutes. Now working on the python script.");

        sw.Reset();
        sw.Start();

        // Run the Python script to fix coco annotations (add polygon segmentations and a few other things)
        if (!disablePythonProcessing){
            PythonRunner pythonRunner = FindObjectOfType<PythonRunner>();
            ScreenShot screenShot = FindObjectOfType<ScreenShot>();
            pythonRunner.RunPythonScript(screenShot.datasetDirectory);
        } else {
            Debug.Log("Skipped running python script");
        }
        
        sw.Stop();
        Debug.Log($"Finished running python script {sw.ElapsedMilliseconds/1000f/60f} minutes.");
    }

    public IEnumerator InterpretDomain(Domain domain, Field field, List<Event> events){
        Debug.Log("Step0");
        LoadNewDomain(domain, field);
        Debug.Log("Step1");
        yield return new WaitForSeconds(2.0f);

        DateTime initialTime = DateTime.Now;
        int minutesLimit = domain.minutesLimit;

        int currentIteration = 0;
        int imagesLimit = domain.imagesLimit;

        bool timeIsValid(){
            // Check if there is a time limit
            if (minutesLimit == -1){
                return true;
            }
            // If there is a time limit, check if the time has passed
            if ((DateTime.Now - initialTime).TotalMinutes < minutesLimit){
                return true;
            }
            return false;
        }

        bool imageCountIsValid(){
            // Check if there is a image count limit
            if (imagesLimit == -1){
                return true;
            }
            // Return true if there are less images than the image limit
            if (currentIteration < imagesLimit){
                return true;
            }
            return false;
        }
        
        Debug.Log("Step2");
        while (!interrupt && timeIsValid() && imageCountIsValid()){
            yield return null; // Wait a frame to allow the user to see changes

            // Run iteration: Check events, move camera, take picture.
            yield return StartCoroutine(InterpretEvents(currentIteration, events));
            
            // Print a progress message
            StringBuilder progress = new StringBuilder();
            progress.Append($"Field: {field.name}\n");
            StringBuilder evSB = new StringBuilder();
            if (events.Count > 0){
                foreach (Event ev in events){
                    evSB.Append($"'{ev.name}', ");
                }
                evSB.Length = evSB.Length - 2; // remove trailing ", "
            }
            progress.Append($"Events: {evSB}\n");
            if (imageCountIsValid()){
                progress.Append($"{currentIteration + 1}/{imagesLimit} images created.\n");
            } if (timeIsValid()){
                progress.Append($"Up to {minutesLimit - (DateTime.Now - initialTime).TotalMinutes} minutes remaining");
            }
            Debug.Log(progress.ToString());
            currentIteration += 1;
        }
    }

    private IEnumerator InterpretEvents(int currentIteration, List<Event> events){
        MoveCamera();

        float waitTime = 1f; // Base wait time
        foreach (Event ev in events){
            waitTime += ev.RunEventForIteration(currentIteration);
        }
        
        yield return new WaitForSeconds(waitTime);

        // Take the screenshots
        yield return StartCoroutine(TakePicture());
    }

    //  Load the next Domain in the domains list.
    private void LoadNewDomain(Domain newDomain, Field newField){
        // Build the field if it is not the same as the previous field
        Debug.Log("Loading new domain");
        if (newField.name != currentDomain.fieldName){
            newField.Build();
        }

        // Update the domain
        currentDomain = newDomain;
    }

    //  Move camera using OrbitHandler
    private void MoveCamera(){
        OrbitHandler orbitHandler = FindObjectOfType<OrbitHandler>();
        orbitHandler.MoveCameraRandomly();
    }

    private IEnumerator TakePicture(){
        ScreenShot screenShot = FindObjectOfType<ScreenShot>();
        float timeToWait = 0.1f;
        yield return StartCoroutine(screenShot.ScreenshotSequenceEnum(timeToWait));
        yield return null;
    }

    public void Interrupt(){
        interrupt = true;
    }
}