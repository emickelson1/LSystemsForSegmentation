import bpy
import os

def export_selected(unity_project_path, field):
    base_export_path = f"{unity_project_path}/Assets/Resources/Meshes/Ground Cover Models/{field}/"
    
    # Ensure the export path exists
    if not os.path.exists(base_export_path):
        os.makedirs(base_export_path)
    
    # Bake the materials of the selected objects into textures before exporting them
    object_names = [obj.name for obj in bpy.context.selected_objects]
    # Deselect other objects
    bpy.ops.object.select_all(action='DESELECT')
    for object_name in object_names:
        # Select the current object
        obj = bpy.data.objects[object_name]
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        
        # Bake the texture of the object into the Unity project
        baked_texture_name = object_name + "_texture"
        bake_results = Bake(object_name, unity_project_path, baked_texture_name)
        material = bake_results[0]
        color_node = bake_results[1]
        
        # Export the selected objects to the base export folder
        export_path = base_export_path + object_name + ".fbx"
        bpy.ops.export_scene.fbx(filepath=export_path, use_selection=True, path_mode="COPY", embed_textures=True)
        
        # Re-connect the Principled BSDF and the color (instead of the baked texture)
        ResetNodeConnections(material, color_node)
        
        # Deselect all objects for the next iteration
        bpy.ops.object.select_all(action='DESELECT')

    print("Export completed!")

def Bake(object_name, unity_project_path, baked_texture_name, width=512, height=512):
    bpy.context.scene.render.engine = 'CYCLES'
    bpy.context.scene.cycles.device = 'GPU'
    # Get base path for images to be saved to
    image_path = f"{unity_project_path}/Assets/Resources/Textures/Blender Export Selected/"
    
    # Name the image that will be saved
    baked_image_name = baked_texture_name + ".png"

    # Select the first object in the object list
    obj = bpy.data.objects[object_name]
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)

    # Get the material
    material = obj.active_material
#    if material.users > 1:
#        material = material.copy()
#        obj.data.materials[0] = material

    # Create a new image to bake to
    baked_image = bpy.data.images.new(baked_texture_name, width, height)

    # Find the existing Image Texture node from the material or create a new one if not found
    baked_image_node = None
    nodes = material.node_tree.nodes
    for node in nodes:
        if node.type == 'TEX_IMAGE':
            baked_image_node = node
            break
    if not baked_image_node:
        baked_image_node = nodes.new(type='ShaderNodeTexImage')
        baked_image_node.name = baked_texture_name  # Name the node "BakedTexture"
    baked_image_node.image = baked_image

    # Set the image texture node as active for baking
    material.node_tree.nodes.active = baked_image_node  # Set the active node for baking

    # Set bake settings
    bpy.context.scene.render.bake.use_selected_to_active = False
    bpy.context.scene.render.bake.use_pass_direct = False
    bpy.context.scene.render.bake.use_pass_indirect = False
    bpy.context.scene.render.bake.use_pass_color = True
    bpy.ops.object.bake(type='DIFFUSE')

    # Perform the bake
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    
    bpy.context.scene.render.bake.use_clear = True

    # Save the baked image to specified path
    baked_image.filepath_raw = image_path + baked_image_name
    baked_image.file_format = 'PNG'
    baked_image.save()

    # Find the existing Principled BSDF node
    principled_bsdf = None
    for node in nodes:
        if node.type == 'BSDF_PRINCIPLED':
            principled_bsdf = node
            break

    # If Principled BSDF node is not found, create a new one
    if not principled_bsdf:
        principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    
    # Save the node that is connected to PBSDF to re-attach it after exporting
    previous_color_node = GetNodeAttachedToPBSDF(principled_bsdf)

    # Connect the baked texture to the Base Color of the Principled BSDF node
    material.node_tree.links.new(baked_image_node.outputs['Color'], principled_bsdf.inputs['Base Color'])

    # Find the output node
    output_node = None
    for node in nodes:
        if node.type == 'OUTPUT_MATERIAL':
            output_node = node
            break

    # Connect the Principled BSDF node to the material output
    material.node_tree.links.new(principled_bsdf.outputs['BSDF'], output_node.inputs['Surface'])

    print("Baking complete and material updated with baked texture")
    
    return material, previous_color_node



# In a Material Nodes, get the node that outputs to the input of the given PBSDF
def GetNodeAttachedToPBSDF(principled_bsdf):
    node = None
    if principled_bsdf:
        # assuming principled BSDF only has one link
        for input_socket in principled_bsdf.inputs:
            if input_socket.is_linked:
                link = input_socket.links[0]  # Assuming only one link
                node = link.from_node
                break
    return node

def GetPBSDF(nodes):
    # Find the existing Principled BSDF node
    for node in nodes:
        if node.type == 'BSDF_PRINCIPLED':
            return node

# Input: (material, color node)
# Reconnects the color node to the principled BSDF of the material
# Call after the process of baking and exporting
def ResetNodeConnections(material, color_node):
    # Get the principled_BSDF to connect to
    nodes = material.node_tree.nodes
    principled_bsdf = GetPBSDF(nodes)
    
    # Connect the node and principled_bsdf
    material.node_tree.links.new(color_node.outputs['Color'], principled_bsdf.inputs['Base Color'])


# UI

# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Export Selected Objects as Undergrowth"
    bl_idname = "WHEAT_PT_export_selected_objects_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"
 
    def draw(self, context):
        layout = self.layout
        mytool = context.scene.my_tool
        
        box = layout.box()
        
        row = box.row()
        row.operator("wheat.export_selected_objects")
 
 
# Operator for the separate wheat parts button in UI
class WHEAT_OT_export_selected_objects(bpy.types.Operator):
    bl_label = "Export Selected Objects as Undergrowth"
    bl_idname = "wheat.export_selected_objects"
    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        export_selected(mytool.unity_project_path, mytool.field)
        
        return {'FINISHED'}
 
 
# Handle registration of UI classes

classes = [WHEAT_PT_main_panel, WHEAT_OT_export_selected_objects]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
 
def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)

if __name__ == "__main__":
    register()
