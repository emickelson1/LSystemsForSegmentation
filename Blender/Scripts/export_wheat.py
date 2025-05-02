import bpy
import os
import separate_wheat_by_part

def export_wheats(unity_project_path, field):
    base_export_path = f"{unity_project_path}/Assets/Resources/Meshes/Wheat Models/{field}/"
    
    # Ensure the export path exists
    if not os.path.exists(base_export_path):
        os.makedirs(base_export_path)

    # Name of the parent collection
    parent_collection_name = "Wheat Plant Objects"

    # Find the parent collection
    parent_collection = bpy.data.collections.get(parent_collection_name)

    # Collect the nodes from the material nodes to reset them after exporting baked materials
    previous_color_nodes = None

    if parent_collection is None:
        print(f"Collection '{parent_collection_name}' not found!")
    else:
        print("Separating parts...")
        separate_wheat_by_part.separate_wheat_parts()
        # need_to_separate_parts = False
        # for obj in parent_collection.children:
        #     if obj.type == 'MESH':
        #         need_to_separate_parts = True
        # if need_to_separate_parts:

        # Auto bake (calls AutoBakeAllMaterials which calls several fn's from AutoBake)
        previous_color_nodes = AutoBakeAllMaterials(unity_project_path, field)
        
        # Iterate through all child collections of the parent collection
        for collection in parent_collection.children:
            # Set the active collection
            bpy.context.view_layer.active_layer_collection = bpy.context.view_layer.layer_collection.children[parent_collection.name].children[collection.name]

            # Select all objects in the collection
            for obj in bpy.context.view_layer.objects:
                obj.select_set(obj.name in collection.objects)

            # Export the selected objects to the base export folder
            export_path = base_export_path + collection.name + ".fbx"
            bpy.ops.export_scene.fbx(filepath=export_path, use_selection=True, path_mode="COPY", embed_textures=True)

            # Deselect all objects for the next iteration
            bpy.ops.object.select_all(action='DESELECT')
        
        print("Export completed!")
        
        ResetNodeConnections(previous_color_nodes)

# Get a head object, then call on Autobake to use that object to bake the Head material texture.
# Returns all objects that need to have their nodes reset
def AutoBakeAllMaterials(unity_project_path, field) -> list:
    wheat_plant_objects = bpy.data.collections.get("Wheat Plant Objects")
    
    # Get the first wheat object in the scene
    first_wheat_parts = wheat_plant_objects.children[0].objects
    
    # Bake the head texture using the first wheat head found
    for obj in first_wheat_parts:
        if obj.name.startswith("Head"):
            previous_head_color_node = Bake(obj, unity_project_path, field + "_Head")
            break
        
    # Bake the awn texture using the first awn found
    for obj in first_wheat_parts:
        if obj.name.startswith("Awn"):
            previous_awn_color_node = Bake(obj, unity_project_path, field + "_Awn")
            break
    
    def BakeForGeneratedObjects(object_search_key):
        def simple_unwrap_uv(obj):
            if obj is None or obj.type != 'MESH':
                print("Invalid object. Must be a mesh.")
                return
            
            bpy.ops.object.select_all(action='DESELECT')
            
            bpy.context.view_layer.objects.active = obj
            obj.select_set(True)

            bpy.ops.object.mode_set(mode='EDIT')
            
            bpy.ops.mesh.select_all(action='SELECT')
            # for v in obj.data.vertices:
            #     v.select = True
            # selected_verts = [v for v in obj.data.vertices if v.select]
            # print(f"Selected vertices count before unwrapping: {len(selected_verts)}")
            
            bpy.ops.uv.smart_project()
            
            bpy.ops.object.mode_set(mode='OBJECT')
            obj.select_set(False)

        objects = [obj for obj in flatten([child.objects for child in wheat_plant_objects.children]) if object_search_key in obj.name]
        previous_color_nodes = list()
        
        for obj in objects:
            # Create a simple UV map for the object
            simple_unwrap_uv(obj)
            
            # Make the material single-user
            obj.data.materials[0] = obj.active_material.copy()
            
            # Bake the material
            texture_name = f"{field}_{obj.name}"
            previous_color_nodes.append(Bake(obj, unity_project_path, texture_name))
            
        return previous_color_nodes
    
    # Bake unique textures for each leaf and stem
    previous_color_nodes_list = list()
    previous_color_nodes_list.append(BakeForGeneratedObjects("Leaf"))
    previous_color_nodes_list.append(BakeForGeneratedObjects("Stem"))
    previous_color_nodes = flatten(previous_color_nodes_list)
    
    # export_wheats will call another function to reattach these nodes to the output after exporting
    # includes: one wheat head, one awns, one stem, and every leaf
    return flatten([[previous_head_color_node, previous_awn_color_node], previous_color_nodes])

# from Autobake script
def Bake(obj, unity_project_path, baked_texture_name, width=512, height=512):
    bpy.context.scene.render.engine = 'CYCLES'
    bpy.context.scene.cycles.device = 'GPU'
    # Get base path for images to be saved to
    image_path = f"{unity_project_path}/Assets/Resources/Textures/Blender Export Wheat/"
    
    # Name the image that will be saved
    baked_image_name = baked_texture_name + ".png"

    # Select the obj
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)

    # Get the material
    material = obj.active_material

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
        baked_image_node.name = baked_texture_name  # Name the node
    baked_image_node.image = baked_image

    # Set the image texture node as active for baking
    material.node_tree.nodes.active = baked_image_node  # Set the active node for baking

    # Set bake settings
    bpy.context.scene.render.bake.use_selected_to_active = False
    bpy.context.scene.render.bake.use_pass_direct = False
    bpy.context.scene.render.bake.use_pass_indirect = False
    bpy.context.scene.render.bake.use_pass_color = True
    bpy.context.scene.render.bake.use_clear = True
    
    # Perform the bake
    bpy.ops.object.bake(type='DIFFUSE')

    # Save the baked image to specified path
    baked_image.filepath_raw = image_path + baked_image_name
    baked_image.file_format = 'PNG'
    baked_image.save()

    # Find the existing Principled BSDF node
    principled_bsdf = GetPBSDF(nodes)

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
    
    return material, previous_color_node # Used for returning the matnodes to their original state after export


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
def ResetNodeConnections(old_nodes):
    for material_node_tuple in old_nodes:
        # Get information from input
        material = material_node_tuple[0]
        color_node = material_node_tuple[1]
        
        # Get the principled_BSDF to connect to
        nodes = material.node_tree.nodes
        principled_bsdf = GetPBSDF(nodes)
        
        # Connect the node and principled_bsdf
        material.node_tree.links.new(color_node.outputs['Color'], principled_bsdf.inputs['Base Color'])
        
def flatten(xss):
    return [x for xs in xss for x in xs]

    
# UI

# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Export Wheat Objects"
    bl_idname = "WHEAT_PT_export_wheat_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"
 
    def draw(self, context):
        layout = self.layout
        mytool = context.scene.my_tool
        
        box = layout.box()
        row = box.row()
        row.prop(mytool, "unity_project_path")
        row = box.row()
        row.prop(mytool, "field")
        
        row = box.row()
        row.operator("wheat.export_wheat_objects")
 
 
# Operator for the separate wheat parts button in UI
class WHEAT_OT_export_wheat_objects(bpy.types.Operator):
    bl_label = "Export Wheat Objects"
    bl_idname = "wheat.export_wheat_objects"
    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        export_wheats(mytool.unity_project_path, mytool.field)
        
        return {'FINISHED'}
 
 
# Handle registration of UI classes

classes = [WHEAT_PT_main_panel, WHEAT_OT_export_wheat_objects]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
 
def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)

if __name__ == "__main__":
    register()
