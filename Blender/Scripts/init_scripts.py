import os
import sys
import bpy


def setup_paths():
    base_path = bpy.path.abspath("//")
    scripts_dir = os.path.join(base_path, "Scripts")

    if scripts_dir not in sys.path:
        sys.path.append(scripts_dir)


setup_paths()
