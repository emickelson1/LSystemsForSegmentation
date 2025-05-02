using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DatasetJSON
{
    public List<Image> images = new List<Image>();
    public List<AnnotationMap> annotationMaps = new List<AnnotationMap>();
    public List<Annotation> annotations = new List<Annotation>();
    public List<Category> categories = new List<Category>();
}

[Serializable]
public class Image{
    public int id;
    public int width;
    public int height;
    public string file_name;
    public string date_captured;

    // constructor
    public Image(int id, int width, int height, string file_name, string date_captured){
        this.file_name = file_name;
        this.width = width;
        this.height = height;
        this.id = id;
        this.date_captured = date_captured;
    }
}

[Serializable]
public class AnnotationMap {
    public int image_id;
    public int width;
    public int height;
    public string file_name;

    public AnnotationMap(int image_id, int width, int height, string file_name){
        this.image_id = image_id;
        this.width = width;
        this.height = height;
        this.file_name = file_name;
    }
}

// Meant to represent leaf, wheat head, etc. as ints. Basically a dictionary.
[Serializable]
public class Category {
    public int id;
    public string name;

    // constructor
    public Category(int id, string name){
        this.id = id;
        this.name = name;
    }
}

[Serializable]
public class Annotation {
    public int id;
    public int image_id; // image id
    public int category_id; // part id
    public int area; // number of segmented pixels
    public int[] bbox; // xmin, ymin, xmax, ymax
    public int iscrowd;
    
    // constructor
    public Annotation(int id, int image_id, int category_id, int area, int[] bbox){
        this.id = id;
        this.image_id = image_id;
        this.category_id = category_id;
        this.area = area;
        this.bbox = bbox;
        this.iscrowd = 0;
    }
}
