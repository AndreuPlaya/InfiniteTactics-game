using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }


    [SerializeField] private Transform cinemachineFollowTransform;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;


    
    public Grid<Coord> map;

    [SerializeField] private int mapWidth = 40;
    [SerializeField] private int mapHeight = 40;
    [SerializeField] private int fillPercent = 45;
    [SerializeField] private int obstaclePercent = 10;
    private float cellSize = 1f;
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GenerateMap();
       
    }
    private void GenerateMap()
    {
        MapGenerator.Instance.GenerateMap(mapWidth, mapHeight, cellSize, 0, fillPercent, obstaclePercent);
    }

 

}
