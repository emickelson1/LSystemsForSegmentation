import os
import sys
import bpy
import random

import export_wheat
import export_selected

def quick_export_field(unity_project_path, field):
    export_wheat.export_wheats(unity_project_path, field)

    # Name of the parent collection
    parent_collection_name = "Wheat Plant Objects"

    # Find the parent collection
    parent_collection = bpy.data.collections.get(parent_collection_name)

    # Iterate through all child collections of the parent collection
    leaf_objects = list()
    for collection in parent_collection.children:
        for obj in collection.objects:
            if obj.type == 'Mesh' and obj.name.contains("Leaf["):
                leaf_objects.append(obj)

    # Choose up to 15 leaves to export
    num_leaves_to_export = min(len(leaf_objects), 15)
    leaf_objects_to_export = random.shuffle(leaf_objects)[:num_leaves_to_export]

    # Set the active collection
    bpy.context.view_layer.active_layer_collection = bpy.context.view_layer.layer_collection.children[parent_collection.name].children[collection.name]

    # Select all objects in the collection
    for obj in leaf_objects_to_export:
        obj.select_set(obj.name in collection.objects)
        obj.select_set(True)

    # Export the leaves
    export_selected.export_selected(unity_project_path, field)

# UI

# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Quick Export Field"
    bl_idname = "WHEAT_PT_quick_export_field_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"

    def draw(self, context):
        layout = self.layout
        mytool = context.scene.my_tool

        box = layout.box()
        row = box.row()
        row.operator("wheat.quick_export_field")


# Operator for the separate wheat parts button in UI
class WHEAT_OT_quick_export_wheat(bpy.types.Operator):
    bl_label = "Quick Export Field"
    bl_idname = "wheat.quick_export_field"

    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool

        quick_export_field(mytool.unity_project_path, mytool.field)

        return {'FINISHED'}


# Handle registration of UI classes

classes = [WHEAT_PT_main_panel, WHEAT_OT_quick_export_wheat]


def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
