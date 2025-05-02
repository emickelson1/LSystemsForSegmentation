import json
import matplotlib.pyplot as plt
from PIL import Image
import numpy as np
import sys

# Load COCO JSON file
if sys.argv[1] is not None:
    coco_json_path = f"{sys.argv[1]}/coco_annotations_onlywheatheads.json"
    image_directory = f"{sys.argv[1]}/images"

    with open(coco_json_path) as f:
        coco_data = json.load(f)

    # Create a dictionary to map image IDs to file names and annotations
    image_dict = {img['id']: img for img in coco_data['images']}
    annotations_dict = {}
    for annotation in coco_data['annotations']:
        img_id = annotation['image_id']
        if img_id not in annotations_dict:
            annotations_dict[img_id] = []
        annotations_dict[img_id].append(annotation)

    # Function to draw polygons and bounding boxes on images
    def draw_polygons(image_path, annotations):
        print(len(annotations))
        image = Image.open(image_path)
        fig, ax = plt.subplots(1)
        ax.imshow(image)

        for annotation in annotations:
            print(annotation['id'])
            # Draw bounding box
            bbox = annotation['bbox']
            bbox_rect = plt.Rectangle(
                (bbox[0], bbox[1]), bbox[2], bbox[3],
                linewidth=1, edgecolor='yellow', facecolor='none'
            )
            ax.add_patch(bbox_rect)

            # Draw polygons
            for polygon in annotation['segmentation']:
                poly = np.array(polygon).reshape((len(polygon)//2, 2))
                category_id = annotation['category_id']
                if category_id == 1:
                    edge_color = "r"
                elif category_id == 2:
                    edge_color = "g"
                else:
                    edge_color = "b"

                polygon_patch = plt.Polygon(poly, linewidth=1, edgecolor=edge_color, facecolor='none')
                ax.add_patch(polygon_patch)

        plt.show()

    # Loop through images and their annotations
    for img_id, image_info in image_dict.items():
        image_path = f"{image_directory}/{image_info['file_name']}"
        if img_id in annotations_dict:
            draw_polygons(image_path, annotations_dict[img_id])
else:
    print("Error: Must provide the dataset location as an argument")
