import glob
import json
import shutil
import cv2
import numpy as np
from collections import defaultdict
import os
import sys
from tqdm import tqdm
from imantics import Mask


# Copy a JSON file to a new location. Categories can be excluded from the final result by passing a list of ints.
def json_copy(input_json_path, output_json_path, only_wheat=False):
    # Load the existing JSON file
    with open(input_json_path, 'r') as f:
        data = json.load(f)

    # Filter annotations
    if not only_wheat:
        filtered_annotations = [anno for anno in data['annotations']]
    else:
        filtered_annotations = [anno for anno in data['annotations'] if anno['category_id'] == 1]

    # Update the data dictionary
    data['annotations'] = filtered_annotations

    # Write the new JSON file
    with open(output_json_path, 'w') as f:
        json.dump(data, f, indent=2)


def reorganize_dataset(domain_path):
    # Check if the dataset path exists
    if os.path.exists(domain_path):
        # Make dir for the release dataset
        domain_path = domain_path.rstrip('/\\')
        new_directory_path = f"{domain_path}_seg"
        if not os.path.exists(new_directory_path):
            os.mkdir(new_directory_path)

        # Copy the JSON into the new directory
        json_copy(f"{domain_path}/coco_annotations.json",
                  f"{new_directory_path}/coco_annotations.json")
        json_copy(f"{domain_path}/coco_annotations.json",
                  f"{new_directory_path}/coco_annotations_onlywheatheads.json", True)

        # Copy the images into the new directory images sub-folder
        new_images_path = f"{new_directory_path}/images"
        os.mkdir(new_images_path)
        images = [image for image in glob.glob(f"{domain_path}/*_image.png")]
        for image in images:
            shutil.copy(image, new_images_path)

    # If the domain does not exist, throw an error.
    else:
        print("The domain directory does not exist.")


# Main code
def get_polygon_segmentation_from_masks(domain_path):
    if os.path.exists(domain_path):
        # Make dir for the release dataset
        domain_path = domain_path.rstrip('/\\')
        # Load the existing JSON file
        json_file_path = os.path.join(domain_path, 'annotations.json')
        with open(json_file_path, 'r') as f:
            coco_data = json.load(f)

        # Remove the "annotationMaps" section
        annotation_maps = coco_data.pop('annotationMaps', [])

        print(f"Total annotation maps: {len(annotation_maps)}")

        # Dictionary to hold segmentation data
        segmentations = defaultdict(list)

        # Process each annotation map with a progress bar
        for annotation_map in tqdm(annotation_maps, desc="Processing annotation maps"):
            annotation_file = annotation_map['file_name']

            # Construct the full path to the annotation PNG file
            annotation_file_path = os.path.join(domain_path, annotation_file)

            # Read the annotation PNG file, collapse the four channels to a 32-bit integer
            annotation_img = cv2.imread(annotation_file_path, cv2.IMREAD_UNCHANGED)
            if annotation_img.shape[2] == 4:
                # Extract each channel
                r_channel = annotation_img[:, :, 2]  # Red channel
                g_channel = annotation_img[:, :, 1]  # Green channel
                b_channel = annotation_img[:, :, 0]  # Blue channel
                a_channel = annotation_img[:, :, 3]  # Alpha channel

                # Combine channels into a single 32-bit integer
                annotation_img = (r_channel.astype(np.uint32) << 24) | \
                                 (g_channel.astype(np.uint32) << 16) | \
                                 (b_channel.astype(np.uint32) << 8) | \
                                 (a_channel.astype(np.uint32))

            if annotation_img is None:
                print(f"Failed to load annotation image: {annotation_file_path}")
                continue

            # Convert the annotation image to unique object IDs
            unique_ids = np.unique(annotation_img)

            for obj_id in unique_ids:
                if obj_id == 0:
                    continue  # Skip the background
                # print(obj_id)

                # Create a binary mask for the current object ID
                binary_mask = annotation_img == obj_id

                polygons = Mask(binary_mask).polygons()

                for polygon in polygons:
                    segmentations[obj_id].append(polygon.tolist())

        # Update the annotations in the COCO data with the segmentation data
        for annotation in tqdm(coco_data['annotations'], desc="Updating Annotations"):
            annotation_id = annotation['id']
            if annotation_id in segmentations:
                annotation['segmentation'] = segmentations[annotation_id]

        # Test with standard JSON module
        standard_json_file_path = os.path.join(domain_path, 'coco_annotations.json')
        try:
            with open(standard_json_file_path, 'w') as f:
                json.dump(coco_data, f, indent=2)
            print(f"Standard JSON serialized and saved to {standard_json_file_path}")
        except Exception as e:
            print(f"Error saving JSON file with standard json module: {e}")

def get_rle_segmentation_from_masks(domain_path):
    if os.path.exists(domain_path):
        # Make dir for the release dataset
        domain_path = domain_path.rstrip('/\\')
        # Load the existing JSON file
        json_file_path = os.path.join(domain_path, 'annotations.json')
        with open(json_file_path, 'r') as f:
            coco_data = json.load(f)

        # Remove the "annotationMaps" section
        annotation_maps = coco_data.pop('annotationMaps', [])

        print(f"Total annotation maps: {len(annotation_maps)}")

        # Dictionary to hold segmentation data
        segmentations = defaultdict(list)

        # Process each annotation map with a progress bar
        for annotation_map in tqdm(annotation_maps, desc="Processing annotation maps"):
            annotation_file = annotation_map['file_name']

            # Construct the full path to the annotation PNG file
            annotation_file_path = os.path.join(domain_path, annotation_file)

            # Read the annotation PNG file, collapse the four channels to a 32-bit integer
            annotation_img = cv2.imread(annotation_file_path, cv2.IMREAD_UNCHANGED)
            if annotation_img.shape[2] == 4:
                # Extract each channel
                r_channel = annotation_img[:, :, 2]  # Red channel
                g_channel = annotation_img[:, :, 1]  # Green channel
                b_channel = annotation_img[:, :, 0]  # Blue channel
                a_channel = annotation_img[:, :, 3]  # Alpha channel

                # Combine channels into a single 32-bit integer
                annotation_img = (r_channel.astype(np.uint32) << 24) | \
                                 (g_channel.astype(np.uint32) << 16) | \
                                 (b_channel.astype(np.uint32) << 8) | \
                                 (a_channel.astype(np.uint32))

            if annotation_img is None:
                print(f"Failed to load annotation image: {annotation_file_path}")
                continue

            # Convert the annotation image to unique object IDs
            unique_ids = np.unique(annotation_img)

            for obj_id in unique_ids:
                if obj_id == 0:
                    continue  # Skip the background
                # print(obj_id)

                # Create a binary mask for the current object ID
                binary_mask = annotation_img == obj_id

                polygons = Mask(binary_mask).polygons() # add the new seg method here

                for polygon in polygons:
                    segmentations[obj_id].append(polygon.tolist())

        # Update the annotations in the COCO data with the segmentation data
        for annotation in tqdm(coco_data['annotations'], desc="Updating Annotations"):
            annotation_id = annotation['id']
            if annotation_id in segmentations:
                annotation['segmentation'] = segmentations[annotation_id]

        # Test with standard JSON module
        standard_json_file_path = os.path.join(domain_path, 'coco_annotations.json')
        try:
            with open(standard_json_file_path, 'w') as f:
                json.dump(coco_data, f, indent=2)
            print(f"Standard JSON serialized and saved to {standard_json_file_path}")
        except Exception as e:
            print(f"Error saving JSON file with standard json module: {e}")


# Copy a JSON file to a new location. Categories can be excluded from the final result by passing a list of ints.
def add_is_crowd_and_fix_bbox(domain_path):
    json_path = f"{domain_path}coco_annotations_onlywheatheads.json"

    # Load the existing JSON file
    with open(json_path, 'r') as f:
        data = json.load(f)

    # Remove unused categories
    filtered_categories = [cat for cat in data['categories'] if (cat['name'] == "Ground" or cat['name'] == "Head")]
    data['categories'] = filtered_categories

    resolution = 1024
    # Fix the bounding box format and add iscrowd
    for annotation in data['annotations']:
        bounding_box = annotation['bbox']
        bl_x = bounding_box[0]
        bl_y = bounding_box[1]
        tr_x = bounding_box[2]
        tr_y = bounding_box[3]

        width = tr_x - bl_x
        height = tr_y - bl_y

        annotation['bbox'] = [bl_x, resolution - tr_y, width, height]
        annotation['iscrowd'] = 0

    # Write the new JSON file
    with open(json_path, 'w') as f:
        json.dump(data, f, indent=2)


if sys.argv[1] is not None and sys.argv[2] is not None:
    dataset_path = sys.argv[1]
    domain_paths = glob.glob(f"{dataset_path}/*/")
    print(f"Domain paths found: {domain_paths}")

    if len(domain_paths) > 0:
        for domain_path in domain_paths:
            domain_path = domain_path.rstrip("/\\")

            # polygon encoding segmentation
            get_polygon_segmentation_from_masks(domain_path)

            # Move the files to another directory without anything unnecessary
            reorganize_dataset(domain_path)

            # Fix some issues with the annotation
            add_is_crowd_and_fix_bbox(domain_path)
    else:
        print("Error: could not find any domain paths in dataset path")
else:
    print("arg 1: dataset path (not domain path!)")
