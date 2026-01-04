using UnityEngine;
using System.Collections.Generic;

public enum TileType
{
    Normal,     // Cost: 1
    Heavy,      // Cost: 2
    Obstacle,   // Cost: 999 (Impassable)
    Start,      // Starting Point
    Home,       // Goal
    Ice         // Slides
}

[System.Serializable]
public class TileData
{
    public int x, y;
    public TileType type;
    public int cost;
    public GameObject visualObject;
    
    public int GetMoveCost()
    {
        if (type == TileType.Obstacle) return 999;
        return cost;
    }
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Dictionary<Vector2Int, TileData> grid = new Dictionary<Vector2Int, TileData>();
    
    [Header("Grid Settings")]
    public float tileSize = 0.15f; 
    
    // Limits
    public int minX, maxX, minY, maxY;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
            return; 
        }
        Instance = this;
    }

    public void RegisterTile(Vector2Int pos, TileType type, int cost, GameObject obj)
    {
        TileData data = new TileData
        {
            x = pos.x,
            y = pos.y,
            type = type,
            cost = cost,
            visualObject = obj
        };

        if (grid.ContainsKey(pos))
        {
            grid[pos] = data; // Overwrite
        }
        else
        {
            grid.Add(pos, data);
        }

        // Update bounds
        if (pos.x < minX) minX = pos.x;
        if (pos.x > maxX) maxX = pos.x;
        if (pos.y < minY) minY = pos.y;
        if (pos.y > maxY) maxY = pos.y;
    }

    public void ClearGrid()
    {
        grid.Clear();
        minX = int.MaxValue; maxX = int.MinValue;
        minY = int.MaxValue; maxY = int.MinValue;
    }

    public TileData GetTileAt(Vector2Int pos)
    {
        if (grid.TryGetValue(pos, out TileData tile))
        {
            return tile;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;
        foreach (var kvp in grid)
        {
            Vector3 worldPos = new Vector3(kvp.Key.x * tileSize, kvp.Key.y * tileSize, 0); // Approx
            if (kvp.Value.visualObject != null) worldPos = kvp.Value.visualObject.transform.position;

            switch(kvp.Value.type)
            {
                case TileType.Obstacle: Gizmos.color = Color.red; break;
                case TileType.Ice: Gizmos.color = Color.cyan; break;
                case TileType.Start: Gizmos.color = Color.green; break;
                case TileType.Home: Gizmos.color = Color.yellow; break;
                default: Gizmos.color = Color.white; break;
            }
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
        }
    }
}
