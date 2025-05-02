import bpy

# based on https://blender.stackexchange.com/a/177364

def separate_wheat_parts():
    # Get plants collection
    wheats_collection = None
    for collection in bpy.data.collections:
        if collection.name.startswith("Wheat Plant Objects"):
            wheats_collection = collection
    
    # Apply modifiers to all wheats
    for obj in wheats_collection.objects:
        # Ensure the object is a mesh and has modifiers
        if obj.type == 'MESH' and obj.modifiers:
            # Select and make the object active
            bpy.context.view_layer.objects.active = obj
            obj.select_set(True)
            
            # Make the object a single user
            bpy.ops.object.make_single_user(type='SELECTED_OBJECTS', object=True, obdata=True)
            
            # Apply all modifiers
            bpy.ops.object.mode_set(mode='OBJECT')
            for modifier in obj.modifiers:
                bpy.ops.object.modifier_apply(modifier=modifier.name)
            
            # Deselect the object
            obj.select_set(False)
        
    # Split each wheat by material
    for object in wheats_collection.objects:
        object.select_set(True)
        
        # Create a new collection for the object
        new_collection = bpy.data.collections.new(object.name)
        wheats_collection.children.link(new_collection)
        
        # Put the wheat into the new collection
        wheats_collection.objects.unlink(object)
        new_collection.objects.link(object)
        
        # Split by material
        if object.type == 'MESH':
            bpy.ops.mesh.separate(type = 'MATERIAL')
        
        object.select_set(False)
        
    # Loop through each object in each wheat collection, splitting leaves
    for collection in wheats_collection.children:
        for obj in collection.objects:
            if obj.data.materials and obj.data.materials[0].name.startswith("Leaf"):
                bpy.ops.object.select_all(action='DESELECT')
                obj.select_set(True)
                # Split by loose parts
                if obj.type == 'MESH':
                    bpy.ops.mesh.separate(type = 'LOOSE')
                obj.select_set(False)
                break;
    
    # Rename each object, in the form "Part[123]"
    # Part is substituted for Leaf, Stem, or Head
    # 123 is the ID of the part; can be more than three digits
    for collection in bpy.data.collections:
        if collection.name.startswith("Wheat["):
            for object in collection.objects:
                if object.data.materials:
                    material = object.data.materials[0]
                    old_name = object.data.name
                    name_numbers = old_name.replace("wheat ", "").replace("Cube.", "")
                    object.name = material.name[:4] + "[" + name_numbers + "]"
    smooth_all_leaves()

def smooth_all_leaves():
    for obj in bpy.data.objects:
        if "Leaf[" in obj.name:
            bpy.context.view_layer.objects.active = obj

            # Add and configure the Smooth modifier
            smooth_modifier = obj.modifiers.new(name="Smooth", type='SMOOTH')
            smooth_modifier.factor = 0.5
            smooth_modifier.iterations = 2

            # Apply the Smooth modifier
            bpy.ops.object.modifier_apply(modifier=smooth_modifier.name)

            # Add and configure the Solidify modifier
            solidify_modifier = obj.modifiers.new(name="Solidify", type='SOLIDIFY')
            solidify_modifier.thickness = 0.005
            solidify_modifier.offset = -1

            # Apply the Solidify modifier
            bpy.ops.object.modifier_apply(modifier=solidify_modifier.name)

            # Add and configure the Decimate modifier
            decimate_modifier = obj.modifiers.new(name="Decimate", type='DECIMATE')
            decimate_modifier.decimate_type = 'DISSOLVE'  # Use 'DISSOLVE' for planar decimation
            decimate_modifier.angle_limit = 1/29  # Convert degrees to radians

            # Apply the Decimate modifier
            bpy.ops.object.modifier_apply(modifier=decimate_modifier.name)
    
# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Separate Wheat Parts"
    bl_idname = "WHEAT_PT_separate_wheat_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"
 
    def draw(self, context):
        layout = self.layout
        scene = context.scene
        
        row = layout.box().row()
        row.operator("wheat.separate_wheat_parts")
 
 
# Operator for the separate wheat parts button in UI
class WHEAT_OT_separate_wheat_parts(bpy.types.Operator):
    bl_label = "Separate Wheat Parts"
    bl_idname = "wheat.separate_wheat_parts"
    
    def execute(self, context):
        
        separate_wheat_parts()
        
        return {'FINISHED'}
 
 
# Handle registration of UI classes

classes = [WHEAT_PT_main_panel, WHEAT_OT_separate_wheat_parts]

def register():
    #bpy.app.handlers.frame_change_post.append(frame_change_handler)
    for cls in classes:
        bpy.utils.register_class(cls)
 
def unregister():
    #bpy.app.handlers.frame_change_post.remove(frame_change_handler)
    for cls in classes:
        bpy.utils.unregister_class(cls)

if __name__ == "__main__":
    register()
