using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int id;
    public int width;
    public int height;
    public Vector2Int start;
    public Vector2Int home;
    public int maxMoves;
    public List<Vector2Int> walls = new List<Vector2Int>();
    public List<WeightData> weights = new List<WeightData>();
    public List<Vector2Int> ice = new List<Vector2Int>();
}

[System.Serializable]
public class WeightData
{
    public Vector2Int pos;
    public int cost;
    public WeightData(int x, int y, int c) { pos = new Vector2Int(x, y); cost = c; }
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Prefabs")]
    public GameObject tileNormalPrefab;
    public GameObject tileHeavyPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject tileIcePrefab;
    public GameObject tileStartPrefab;
    public GameObject tileHomePrefab;
    public GameObject penguinPrefab;

    [Header("Level State")]
    public int currentLevelIndex = 0;
    public List<LevelData> levels = new List<LevelData>();

    [Header("Generation Settings")]
    public float spacing = 0.2f;
    public Transform levelParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        InitializeLevels();
    }

    private void Start()
    {
        GenerateLevel();
    }

    void InitializeLevels()
    {
        levels.Clear();

        // Level 1
        LevelData l1 = new LevelData { id = 1, width = 5, height = 5, maxMoves = 10, start = new Vector2Int(0, 0), home = new Vector2Int(4, 4) };
        l1.walls.Add(new Vector2Int(1, 1)); l1.walls.Add(new Vector2Int(1, 2)); l1.walls.Add(new Vector2Int(3, 3));
        l1.weights.Add(new WeightData(2, 2, 2)); l1.weights.Add(new WeightData(3, 0, 3));
        levels.Add(l1);

        // Level 2
        LevelData l2 = new LevelData { id = 2, width = 6, height = 6, maxMoves = 15, start = new Vector2Int(0, 0), home = new Vector2Int(5, 5) };
        l2.walls.AddRange(new[] { new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(2, 3), new Vector2Int(4, 4), new Vector2Int(4, 3) });
        l2.weights.Add(new WeightData(0, 3, 2)); l2.weights.Add(new WeightData(1, 3, 2)); l2.weights.Add(new WeightData(5, 4, 3));
        levels.Add(l2);

        // Level 3: The Fork
        LevelData l3 = new LevelData { id = 3, width = 5, height = 5, maxMoves = 8, start = new Vector2Int(2, 4), home = new Vector2Int(2, 0) };
        l3.walls.AddRange(new[] { new Vector2Int(1, 2), new Vector2Int(3, 2) });
        l3.weights.Add(new WeightData(2, 2, 3)); l3.weights.Add(new WeightData(0, 2, 1)); l3.weights.Add(new WeightData(4, 2, 1));
        levels.Add(l3);

        // Level 4: Spiral
        LevelData l4 = new LevelData { id = 4, width = 6, height = 6, maxMoves = 18, start = new Vector2Int(0, 0), home = new Vector2Int(2, 3) };
        l4.walls.Add(new Vector2Int(1, 0)); l4.walls.Add(new Vector2Int(2, 0)); l4.walls.Add(new Vector2Int(3, 0)); l4.walls.Add(new Vector2Int(4, 0)); l4.walls.Add(new Vector2Int(5, 0));
        l4.walls.Add(new Vector2Int(5, 1)); l4.walls.Add(new Vector2Int(5, 2)); l4.walls.Add(new Vector2Int(5, 3)); l4.walls.Add(new Vector2Int(5, 4)); l4.walls.Add(new Vector2Int(5, 5));
        l4.walls.Add(new Vector2Int(4, 5)); l4.walls.Add(new Vector2Int(3, 5)); l4.walls.Add(new Vector2Int(2, 5)); l4.walls.Add(new Vector2Int(1, 5)); l4.walls.Add(new Vector2Int(0, 5));
        l4.walls.Add(new Vector2Int(0, 4)); l4.walls.Add(new Vector2Int(0, 3)); l4.walls.Add(new Vector2Int(0, 2));
        l4.weights.Add(new WeightData(1, 1, 2)); l4.weights.Add(new WeightData(4, 4, 2));
        levels.Add(l4);

        // Level 5: Risk Reward
        LevelData l5 = new LevelData { id = 5, width = 5, height = 5, maxMoves = 10, start = new Vector2Int(0, 0), home = new Vector2Int(4, 4) };
        l5.walls.AddRange(new[] { new Vector2Int(1, 1), new Vector2Int(3, 3), new Vector2Int(1, 3), new Vector2Int(3, 1) });
        l5.weights.Add(new WeightData(2, 2, 5)); l5.weights.Add(new WeightData(2, 0, 1)); l5.weights.Add(new WeightData(4, 2, 1));
        levels.Add(l5);

        // Level 6: Ice Slides
        LevelData l6 = new LevelData { id = 6, width = 6, height = 6, maxMoves = 8, start = new Vector2Int(0, 0), home = new Vector2Int(5, 5) };
        l6.ice.AddRange(new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0) });
        l6.ice.AddRange(new[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3), new Vector2Int(0, 4) });
        l6.ice.Add(new Vector2Int(2, 2)); l6.ice.Add(new Vector2Int(3, 3));
        levels.Add(l6);
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        ClearLevel();

        if (levels.Count == 0) InitializeLevels();
        if (currentLevelIndex >= levels.Count) currentLevelIndex = 0;

        LevelData data = levels[currentLevelIndex];
        SpawnLevel(data);
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levels.Count) currentLevelIndex = 0; 
        GenerateLevel();
    }

    void SpawnLevel(LevelData data)
    {
        if (GameManager.Instance == null || GridManager.Instance == null)
        {
            Debug.LogError("No GridManager/GameManager context");
            return;
        }

        // Pass MaxMoves
        GameManager.Instance.maxMoves = data.maxMoves;

        // Clear Grid Data
        GridManager.Instance.ClearGrid();

        float width = data.width * spacing;
        float height = data.height * spacing;
        float startX = -(width / 2f) + (spacing / 2f);
        float startY = -(height / 2f) + (spacing / 2f);

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = new Vector3(startX + (x * spacing), startY + (y * spacing), 0);

                GameObject prefab = tileNormalPrefab;
                TileType type = TileType.Normal;
                int cost = 1;

                if (gridPos == data.home) 
                { 
                    prefab = tileHomePrefab; type = TileType.Home; 
                }
                else if (gridPos == data.start) 
                { 
                    prefab = tileStartPrefab; type = TileType.Start; 
                }
                else if (data.walls.Contains(gridPos)) 
                { 
                    prefab = tileObstaclePrefab; type = TileType.Obstacle; 
                }
                else if (data.ice.Contains(gridPos)) 
                { 
                    prefab = tileIcePrefab; type = TileType.Ice; 
                }
                else 
                {
                    WeightData w = data.weights.Find(wd => wd.pos == gridPos);
                    if (w != null)
                    {
                        cost = w.cost;
                        type = TileType.Heavy;
                        if (cost > 1) prefab = tileHeavyPrefab;
                        else prefab = tileNormalPrefab;
                    }
                }

                if (prefab != null)
                {
                    GameObject tObj = Instantiate(prefab, worldPos, Quaternion.identity);
                    if (levelParent) tObj.transform.parent = levelParent;
                    else tObj.transform.parent = transform;

                    tObj.name = $"Tile_{x}_{y}";
                    tObj.transform.localScale = Vector3.one * 0.15f;
                    
                    // Register directly to GridManager
                    GridManager.Instance.RegisterTile(gridPos, type, cost, tObj);
                }
            }
        }

        // Spawn Player
        if (penguinPrefab != null)
        {
            float px = startX + (data.start.x * spacing);
            float py = startY + (data.start.y * spacing);
            
            float vOffset = 0f;
            if (GameManager.Instance != null) vOffset = GameManager.Instance.verticalOffset;
            
            Vector3 pPos = new Vector3(px, py + vOffset, -0.5f);

            GameObject pObj = Instantiate(penguinPrefab, pPos, Quaternion.identity);
            
            if (GameManager.Instance != null)
                pObj.transform.localScale = Vector3.one * GameManager.Instance.penguinScale;
            else
                pObj.transform.localScale = Vector3.one * 0.4f;
            
            // Pass the player GameObject to GameManager directly
            // No PlayerController required anymore
            GameManager.Instance.InitializeLevel(pObj, data.start);
        }
    }

    public void ClearLevel()
    {
        if (levelParent == null) levelParent = transform;
        
        // Let GameManager know we are clearing
        if (GameManager.Instance != null) GameManager.Instance.ClearLevel();

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in levelParent) children.Add(child.gameObject);
        foreach (GameObject child in children)
        {
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }
}
