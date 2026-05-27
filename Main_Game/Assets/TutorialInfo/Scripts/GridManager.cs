using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Bounds")]
    public Vector2Int gridMin = new Vector2Int(-50, -50);
    public Vector2Int gridMax = new Vector2Int(50, 50);
    public LayerMask wallLayer;

    [Header("Floor Y-Levels")]
    // Enter the exact Y heights of your floors in the inspector
    public float[] floorHeights = { 0f, 5f, 10f }; 

    [Header("Stair Connections")]
    // Assign specific empty GameObjects as stair entry points
    public Transform[] floor1To2Stairs; 

    // The Memory Map: Dictionary<FloorIndex, bool[x, z]>
    private Dictionary<int, bool[,]> levelGrids = new Dictionary<int, bool[,]>();

    void Awake()
    {
        Instance = this;
        BakeGrids();
    }

    void BakeGrids()
    {
        int width = gridMax.x - gridMin.x + 1;
        int height = gridMax.y - gridMin.y + 1;

        for (int floor = 0; floor < floorHeights.Length; floor++)
        {
            bool[,] grid = new bool[width, height];
            float yHeight = floorHeights[floor];

            for (int x = gridMin.x; x <= gridMax.x; x++)
            {
                for (int z = gridMin.y; z <= gridMax.y; z++)
                {
                    Vector3 checkPos = new Vector3(x, yHeight, z);
                    
                    // If NO wall is hit, the space is walkable (true)
                    bool isWalkable = !Physics.CheckSphere(checkPos, 0.45f, wallLayer);
                    
                    // Array indices must start at 0, so we offset by gridMin
                    grid[x - gridMin.x, z - gridMin.y] = isWalkable;
                }
            }
            levelGrids[floor] = grid;
        }
    }

    // O(1) Instant lookup for your A* algorithm
    public bool IsWalkable(int floor, int x, int z)
    {
        if (!levelGrids.ContainsKey(floor)) return false;

        int arrayX = x - gridMin.x;
        int arrayZ = z - gridMin.y;

        int width = gridMax.x - gridMin.x + 1;
        int height = gridMax.y - gridMin.y + 1;

        if (arrayX < 0 || arrayX >= width || arrayZ < 0 || arrayZ >= height) return false;

        return levelGrids[floor][arrayX, arrayZ];
    }
}