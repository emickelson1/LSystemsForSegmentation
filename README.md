# L Systems for Segmentation

## Description
This application is designed for the purpose of generating synthetic datasets for wheat head segmentation.

## Usage
### Generating models via Blender
In Blender, to generate a wheat model, follow these steps:
1. Go to the scripting section, and activate all scripts. These should open custom menus that control the wheat model generation. If you can't see the menus, mouse over a 3D viewport view panel, then press N on your keyboard and navigate to the "Wheat" section on the right side of this panel.
2. Adjust the parameters listed under Instantiate Wheat as desired, then click "Create Wheat". If you plan to export the wheat to Unity, leave the position and position variance at (0,0,0).
3. Click on the "Separate Wheat Parts" button to apply all modifiers to all wheat models, then separate them into folders that contain their parts. This makes the process of baking textures and exporting to Unity much easier.
4. Enter the Unity project directory into the field above the "Export Wheat Models" button, then click it to export the wheat models to Unity.
5. (optional) If desired, select a number of leaf objects and click "Export As Underbrush" to place additional leaves on the ground later

### Creating dataset via Unity
In Unity, to generate the dataset, follow these steps:
1. Adjust settings in the Unity inspector as desired. These include whether to use semantic or instance segmentation, the location and name of the dataset, and which objects to include in the segmentation.
2. Using the ScheduleCreator script, create a Schedule object with the desired time limit, image limit, fields, events, and domains. In future versions, an interface will be created to simplify this process.
3. Enter Play Mode.
5. Save the Schedule object by pressing Z.
5. Once you are ready to generate the dataset, run the Schedule object by pressing X.
6. To stop generation early, press C. Otherwise, wait until generation is complete.
7. Exit play mode.

For semantic segmentation, the dataset will consist of images and corresponding labels in the same folder. They have identical names of the format "YYYY-MM-DD_HH-mm-SS_image.png" for images and "YYYY-MM-DD_HH-mm-SS_label.png" for labels.

For instance segmentation, the same applies, but the labels consist of integer values (created by combining the color value of into a 32-bit integer for each pixel) and an additional JSON file is created that tracks the ID numbers of each object. A Python script is also provided that can adapt these segmentations for the COCO format by representing these labels as polygon segmentations. 

## Support
### Why is there no git history?
This project was initially hosted on GitLab. Unfortunately, due to a lack of foresight in handling large binary files (primarily Blender project versions and a growing number of resources for the Unity project), it was not possible to directly mirror this project to the GitHub.

### Contact me
For help, contact Elijah Mickelson at the following email:

elijah.mickelson@ucalgary.ca

## Roadmap
Future goals:
- Create a new UI, because the previous UI was mostly scrapped after implementing schedule and instance segmentation features
- Implement post-processing of generated segmentation masks to make generated annotations more similar to human annotations
- Replicate the existing Unity project in Unreal Engine 5, potentially improving realism

## Authors and acknowledgment
Developer: Elijah Mickelson

Special thanks to Hosein Beheshtifard and Dr. Farhad Maleki for the helpful advice in improving this program.