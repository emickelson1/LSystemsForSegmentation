bl_info = {
    "name" : "Wheat Generator",
    "author" : "Elijah Mickelson",
    "version" : (1, 4, 0),
    "blender" : (4, 1, 0),
    "location" : "View3d > Wheat",
    "warning" : "",
    "wiki_url" : "",
    "category" : ""
}

import bpy
import random
from mathutils import Vector

wheat_list = []
wheat_model = bpy.data.objects["WHEAT PLANT GEONODES"]
        
# Maps the name of each modifier to its actual identity in geo nodes modifier
modifier_inputs = {
    "Premise" : "Socket_6",
    "Rule 1 L" : "Socket_7",
    "Rule 1 R" : "Socket_8",
    "Rule 2 L" : "Socket_16",
    "Rule 2 R" : "Socket_17",
    "Rule 3 L" : "Socket_18",
    "Rule 3 R" : "Socket_19",
    
    "Set Time to Frame" : "Socket_45",
    "Time" : "Socket_11",
    "Growth Delay" : "Socket_38",
    "Growth Rate" : "Socket_39",
    "Final Growth Stage" : "Socket_40",
    "Angle" : "Socket_12",
    "Length" : "Socket_14",
    "Leaf Density" : "Socket_20",
    "Max Vertical Spikelet Count" : "Socket_53",
    "Spikelet Scale" : "Socket_54",
    "Awns Scale" : "Socket_55",
    
    "Leaf Models" : "Socket_49",
    "Head" : "Socket_31",
    "Leaf" : "Socket_46",
    "Stem" : "Socket_41",
    
    "Position" : "Socket_9",
    "Direction" : "Socket_10",
    "Up" : "Socket_15"
    }

iteration_limit = 6 # TODO: figure out what this does

# Add an object with a given name to the wheats list
def add_wheat_to_list(wheat):
    if type(wheat) == type("string"):
        wheat_list.append(bpy.data.objects[wheat])
    else:
        wheat_list.append(wheat)


# Using the dictionary and a given name,
# get the socket number in the wheat modifiers
def get_socket(name):
    return modifier_inputs[name]

# Reload object with updated modifiers (always call after changing modifiers)
def update(ob):
    modifier = ob.modifiers["GeometryNodes"]
    modifier.node_group.interface_update(bpy.context)

# Get the value of a specific socket
def get_modifier(obj, mod_name):
    mod_socket = get_socket(mod_name)
    return obj.modifiers["GeometryNodes"][mod_socket]

# Update the information for a given geometry nodes socket
def set_modifier(obj, mod_name, new):
    mod_socket = get_socket(mod_name)
    obj.modifiers["GeometryNodes"][mod_socket] = new
    update(obj)

# Use a preset dictionary to set several different geonodes sockets at the same time
def set_modifiers(obj, preset):
    for key in preset.keys():
        set_modifier(obj, key, preset[key])
# Generate an L-system that will determine how the wheat plant grows
def random_l_system():
    # What we start with. S does not represent geometry; it is a placeholder, so at growth step 0, it will be invisible
    premise = "S"
    
    # The first rule is only applied on growth step 1. S is replaced with FAFW: forward, [placeholder], forward, wheat head.
    rule1l = "S"
    rule1r = "FAFW"
    
    # The second rule will apply on every step after step 1. The wheat will grow in a random direction, as determined by rule2r.
    rule2l = "A"
    
    rule2r = ""
    random_turn = random.randint(0,2)
    if random_turn == 0:
        rule2r += "+"
    elif random_turn == 1:
        rule2r += "-"
        
    random_pitch = random.randint(0,2)
    if random_pitch == 0:
        rule2r += "^"
    elif random_pitch == 1:
        rule2r += "&"
        
    rule2r += "FLA"
    
    # The third rule is not in use
    rule3l = ""
    rule3r = ""
    
    # Return a dictionary that can be applied to a geo nodes modifier via set_modifiers(<wheat object>, random_l_system())
    return  {
        "Premise" : premise,
        "Rule 1 L" : rule1l,
        "Rule 1 R" : rule1r,
        "Rule 2 L" : rule2l,
        "Rule 2 R" : rule2r,
        "Rule 3 L" : rule3l,
        "Rule 3 R" : rule3r,
        }


# Create a new wheat instance by duplicating the first one
def generate_wheat_plant(position, modifiers):
    # Make a name
    global wheat_list
    name = "Wheat[" + str(len(wheat_list)) + "]"
    
    # Instantiate object
    new_wheat = bpy.data.objects.new(name=name, object_data = wheat_model.data)
    
    # Apply the geometry nodes modifier from the original wheat
    source = wheat_model
    target = new_wheat
    modifier_name = "GeometryNodes"
    with bpy.context.temp_override(object=source, selected_objects=(source, target)):
        bpy.ops.object.modifier_copy_to_selected(modifier=modifier_name) 
    
    # Update location
    new_wheat.location = position
    
    # Link to wheats collection
    bpy.data.collections["Wheat Plant Objects"].objects.link(new_wheat)
    
    # Add to wheat list
    wheat_list.append(new_wheat)
    
    # Apply modifiers
    set_modifiers(new_wheat, modifiers)
    
    # Generate a simple L-system for this wheat
    set_modifiers(new_wheat, random_l_system())

    

# Set the time variable in each wheat's geo nodes to the given Time
def update_all_wheat_times(time):
    for wheat in wheat_list:
        set_modifier(wheat, "Time", time)
        print(time)


def delete_all_wheat():
    col = bpy.data.collections["Wheat Plant Objects"]
    for child_collection in col.children:
        for obj in child_collection.objects:
            bpy.data.objects.remove(obj, do_unlink = True)
        col.children.unlink(child_collection)
        bpy.data.collections.remove(child_collection)
    for obj in col.objects:
        bpy.data.objects.remove(obj, do_unlink = True)
    wheat_list.clear()


# UI

# Properties in UI panel
class MyProperties(bpy.types.PropertyGroup):
    ## Generation Settings
    
    # Range: (1-length variance) to (1+length variance)
    length_min : bpy.props.FloatProperty(name = "Min", soft_min = 0.5, soft_max = 1.5, default = 0.9)
    length_max : bpy.props.FloatProperty(name = "Max", soft_min = 0.5, soft_max = 1.5, default = 1.1) 
    
    # Range: -(angle variation) to (angle variation)
    angle_min : bpy.props.FloatProperty(name = "Min", soft_min = 0, soft_max = 30, default = 0)
    angle_max : bpy.props.FloatProperty(name = "Max", soft_min = 0, soft_max = 30, default = 5) 
    
    # Leaf density, constant, represents the percentage of bends in the stem that have leaves
    leaf_density : bpy.props.FloatProperty(name = "Value", soft_min = 0, soft_max = 1)
    
    # Range: 0 to (variation)
    growth_delay_min : bpy.props.FloatProperty(name = "Min", soft_min = 0, soft_max = 30, default = 0)
    growth_delay_max : bpy.props.FloatProperty(name = "Max", soft_min = 0, soft_max = 30, default = 3) 
    
    # Range: (1 - growth rate variation) to (1 + growth rate variation)
    growth_rate_min : bpy.props.FloatProperty(name = "Min", soft_min = 0.5, soft_max = 1.5, default = 0.9)
    growth_rate_max : bpy.props.FloatProperty(name = "Max", soft_min = 0.5, soft_max = 1.5, default = 1.1) 
    
    final_growth_stage : bpy.props.IntProperty(name = "Value", soft_min = 1, soft_max = 14, default = 9)
    
    # Range: min-max
    vertical_spikelet_count_min : bpy.props.IntProperty(name = "Min", soft_min = 1, soft_max = 15, default = 13)
    vertical_spikelet_count_max : bpy.props.IntProperty(name = "Max", soft_min = 1, soft_max = 15, default = 15)
    
    # Range: min-max
    spikelet_scale_min : bpy.props.FloatProperty(name = "Min", soft_min = 0.5, soft_max = 1.5, default = 0.9)
    spikelet_scale_max : bpy.props.FloatProperty(name = "Max", soft_min = 0.5, soft_max = 1.5, default = 1.1) 
    
    # Range: min-max
    awns_scale_min : bpy.props.FloatProperty(name = "Min", soft_min = 0, soft_max = 3, default = 0.9)
    awns_scale_max : bpy.props.FloatProperty(name = "Max", soft_min = 0, soft_max = 3, default = 1.1) 
    
    
    ## Quantity, position, time
    
    # Number of wheats to place
    wheat_quantity : bpy.props.IntProperty(name = "Quantity", soft_min = 1, soft_max = 100)
    
    # Whether to use time input or animation frames to determine current growth
    set_time_to_frame : bpy.props.BoolProperty(name = "Set Time to Frame")
    
    # Center to place wheat on
    position : bpy.props.FloatVectorProperty(name = "Position")
    
    # Maximum distance at which the wheat can be placed from the center in each direction (randomized)
    position_variance : bpy.props.FloatVectorProperty(name = "Variance")
    
    # Global time
    time : bpy.props.FloatProperty(name = "New Time", soft_min = 0, soft_max = 10000)
    
    ## Export related variables
    # Base path of the Unity project directory
    unity_project_path : bpy.props.StringProperty(name = "Unity Project Path")
    
    # Field for each model
    # Accessed by the Export script
    field : bpy.props.StringProperty(name = "Field Name")
    

# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Place Wheat"
    bl_idname = "WHEAT_PT_main_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"
 
    def draw(self, context):
        layout = self.layout
        scene = context.scene
        mytool = scene.my_tool
        
        
        # Wheat plant
        row = layout.row()
        row.label(text="Generation Settings")
        
        box = layout.box()
        
        row = box.row()
        row.label(text="Length Scale")
        row.prop(mytool, "length_min")
        row.prop(mytool, "length_max")
        
        row = box.row()
        row.label(text="Bend Angle")
        row.prop(mytool, "angle_min")
        row.prop(mytool, "angle_max")
        
        row = box.row()
        row.label(text="Leaf Density")
        row.prop(mytool, "leaf_density")
        
        row = box.row()
        row.label(text="Growth Rate")
        row.prop(mytool, "growth_rate_min")
        row.prop(mytool, "growth_rate_max")
        
        row = box.row()
        row.label(text="Growth Delay")
        row.prop(mytool, "growth_delay_min")
        row.prop(mytool, "growth_delay_max")
        
        row = box.row()
        row.label(text="Final Growth Stage")
        row.prop(mytool, "final_growth_stage")
        
        row = box.row()
        row.label(text="Max Vertical Spikelet Count")
        row.prop(mytool, "vertical_spikelet_count_min")
        row.prop(mytool, "vertical_spikelet_count_max")
        
        row = box.row()
        row.label(text= "Spikelet Scale")
        row.prop(mytool, "spikelet_scale_min")
        row.prop(mytool, "spikelet_scale_max")
        
        row = box.row()
        row.label(text= "Awns Scale")
        row.prop(mytool, "awns_scale_min")
        row.prop(mytool, "awns_scale_max")
        
        # Position
        row = layout.row()
        row.label(text="Position")
        
        box = layout.box()
        
        row = box.row()
        row.prop(mytool, "position")
        row = box.row()
        row.prop(mytool, "position_variance")
        
        # Placing and destroying wheat
        row = layout.row()
        row.label(text="Place/Destroy Wheat")
        
        row = layout.box().row()
        row.prop(mytool, "wheat_quantity")
        row.operator("wheat.place_wheat")
        
        row = layout.box().row()
        row.operator("wheat.delete_all_wheat")
 
 
# Operator for the place wheat button in UI
class WHEAT_OT_place_wheat(bpy.types.Operator):
    bl_label = "Place Wheat"
    bl_idname = "wheat.place_wheat"

    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        for i in range(mytool.wheat_quantity):
            # If the final growth stage is >7, decrease the angle and length accordingly
            final_growth_stage = mytool.final_growth_stage
            fgmulti = 1
            if final_growth_stage > 7:
                fgmulti = 7.0/final_growth_stage
            
            # Plant modifiers
            length = fgmulti*(1 + random.uniform(mytool.length_min, mytool.length_max))
            angle = fgmulti*random.uniform(mytool.angle_min, mytool.angle_max)
            leaf_density = mytool.leaf_density
            growth_rate = random.uniform(mytool.growth_rate_min, mytool.growth_rate_max)
            growth_delay = random.uniform(mytool.growth_delay_min, mytool.growth_delay_max)
            vertical_spikelet_count = random.randint(mytool.vertical_spikelet_count_min, mytool.vertical_spikelet_count_max)
            spikelet_scale = random.uniform(mytool.spikelet_scale_min, mytool.spikelet_scale_max)
            awns_scale = random.uniform(mytool.awns_scale_min, mytool.awns_scale_max)
            
            # Time modifiers
            set_time_to_frame = mytool.set_time_to_frame
            time = mytool.time
            
            # Position modifiers
            position_variance = Vector(mytool.position_variance);
            delta_x = random.uniform(-position_variance.x, position_variance.x)
            delta_y = random.uniform(-position_variance.y, position_variance.y)
            delta_z = random.uniform(-position_variance.z, position_variance.z)
            delta = Vector((delta_x, delta_y, delta_z))
            position = Vector(mytool.position) + delta
            
            plant_modifiers = {
#               "Set Time to Frame" : set_time_to_frame,
                "Set Time to Frame" : True,
                "Time" : time,
                "Growth Delay" : growth_delay,
                "Growth Rate" : growth_rate,
                "Final Growth Stage" : final_growth_stage,
                "Angle" : angle,
                "Length" : length,
                "Leaf Density" : leaf_density,
                "Max Vertical Spikelet Count" : vertical_spikelet_count,
                "Spikelet Scale" : spikelet_scale,
                "Awns Scale" : awns_scale,
            }
            
            # Generate the wheat plant
            generate_wheat_plant(position, plant_modifiers)
        
        return {'FINISHED'}
 
 # Operator for time update
class WHEAT_OT_update_time(bpy.types.Operator):
    bl_label = "Update Time"
    bl_idname = "wheat.update_time"

    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        update_all_wheat_times(mytool.time)
        
        return {'FINISHED'}


# Operator for clearing wheat
class WHEAT_OT_delete_wheat(bpy.types.Operator):
    bl_label = "Delete All Wheat"
    bl_idname = "wheat.delete_all_wheat"

    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        delete_all_wheat()
        update_all_wheat_times(mytool.time)
        
        return {'FINISHED'}




 
# Handle registration of UI classes

classes = [MyProperties, WHEAT_PT_main_panel, WHEAT_OT_place_wheat, WHEAT_OT_update_time, WHEAT_OT_delete_wheat]

def register():
    #bpy.app.handlers.frame_change_post.append(frame_change_handler)
    for cls in classes:
        bpy.utils.register_class(cls)
        
        bpy.types.Scene.my_tool = bpy.props.PointerProperty(type = MyProperties)
 
def unregister():
    #bpy.app.handlers.frame_change_post.remove(frame_change_handler)
    for cls in classes:
        bpy.utils.unregister_class(cls)
        
        del bpy.types.Scene.my_tool

if __name__ == "__main__":
    register()
