using System.Collections.Generic;
using UnityEngine;

public static class MaskToPolygons
{
    public static List<List<int[]>> GeneratePolygons(List<int[]> positions)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<List<int[]>> polygons = new List<List<int[]>>();

        foreach (var pos in positions)
        {
            Vector2Int pixel = new Vector2Int(pos[0], pos[1]);
            if (!visited.Contains(pixel))
            {
                List<Vector2Int> polygon = new List<Vector2Int>();
                CreatePolygon(pixel, positions, visited, polygon);
                polygons.Add(ConvertToIntArrayList(polygon));
            }
        }

        return polygons;
    }

    private static void CreatePolygon(Vector2Int start, List<int[]> positions, HashSet<Vector2Int> visited, List<Vector2Int> polygon)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        List<Vector2Int> cluster = new List<Vector2Int>();
        cluster.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (positions.Exists(pos => pos[0] == neighbor.x && pos[1] == neighbor.y) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    cluster.Add(neighbor);
                }
            }
        }

        // Generate minimal polygon from the cluster
        if (cluster.Count > 0)
        {
            GenerateMinimalPolygon(cluster, polygon);
        }
    }

    private static void GenerateMinimalPolygon(List<Vector2Int> cluster, List<Vector2Int> polygon)
    {
        // Simple approach to create a convex polygon (could be improved)
        HashSet<Vector2Int> uniquePoints = new HashSet<Vector2Int>(cluster);
        polygon.AddRange(uniquePoints);
    }

    private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
        yield return new Vector2Int(pos.x, pos.y - 1);
    }

    private static List<int[]> ConvertToIntArrayList(List<Vector2Int> polygon)
    {
        List<int[]> result = new List<int[]>();
        foreach (var point in polygon)
        {
            result.Add(new int[] { point.x, point.y });
        }
        return result;
    }
}
