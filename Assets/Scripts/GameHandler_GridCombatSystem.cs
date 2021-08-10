using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridPathfindingSystem;
using Cinemachine;

public class GameHandler_GridCombatSystem : MonoBehaviour
{
    public static GameHandlet_GridCombatSystem Instance { get; private set; }

    [SerializeField] private Transform cinemachineFollowTransform;
    [SerializeField] private MovementTilemapVisual movementTilemapVisual;
    [SerializeField] private CinemachineVirtualCamera CinemachineVirtualCamera;

    private Grid<GridCombatSystem.GridObject> grid;
    private MovementTilemap movementTilemap;
    public GridPathfinding gridPathfinding;

    private void Awake()
    {
        Instance = this;

        int mapWidth = 40;
        int mapHeight = 30;
        float cellSize = 1f;
        Vector3 origin = new Vector3(-mapWidth / 2f, -mapHeight / 2f);

        grid = new Grid<GridCombatSystem.GridObject>(mapWidth, mapHeight, cellSize, origin, (Grid<GridCombatSystem.GridObject> grid, x, y) => new Grid<GridCombatSystem.GridObject>(grid, x, y));
        gridPathfinding = new GridPathfinding();
        movementTilemap = new MovementTilemap(mapWidth, mapHeight, cellSize, origin);

    }
    private void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void HandleCameraMovement()
    {

    }

    public Grid<GridCombatSystem.GridObject> GetGrid()
    {
        return grid;
    }
    public MovementTilemap GetMovementTilemap()
    {

    }

    public void SetCameraFollow(Vector3 targetPosition)
    {

    }

    public void ScreenShake()
    {

    }

    public class EmptyGridObject
    {
        private Grid<EmptyGridObject> grid;
        private int x;
        private int y;
    }
}
