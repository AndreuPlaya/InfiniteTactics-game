using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectSpawner : MonoBehaviour
{
	public static ObjectSpawner Instance { get; private set; }



	public GameObject obstaclePrefab;

	private bool[,] obstacleMap;
	private List<Coord> allTileCoords;
	private Queue<Coord> shuffledTileCoords;
	private Queue<Coord> shuffledOpenTileCoords;
	private Coord startCoord;
	private Coord endCoord;
    private void Awake()
    {
		Instance = this;
    }
  
	public void SetupObstacles(int seed, int obstaclePercent)
    {
		ShuffleAllValidCoords(seed);
		startCoord = GetRandomCoord();
		SpawnObstacles(seed, obstaclePercent);

	}
	

	private void ShuffleAllValidCoords(int seed)
    {
		allTileCoords = new List<Coord>();
		int mapWidth = MapGenerator.Instance.GetGrid().GetWidth();
		int mapHeight = MapGenerator.Instance.GetGrid().GetHeight();
		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				if (MapGenerator.Instance.GetGrid().GetGridObject(x,y).GetCellType() == Coord.CellType.floor)
                {
					allTileCoords.Add(MapGenerator.Instance.GetGrid().GetGridObject(x, y));
				}
			}
		}
		if (seed == 0)
		{
			seed= Time.time.ToString().GetHashCode();
		}
		shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed.GetHashCode()));
		
	}
	public Coord GetStartCoord()
	{
		return startCoord;
	}
	public Coord GetEndCoord()
	{
		return endCoord;
	}

	
	private Coord GetRandomCoord()
    {
		Coord randomCoord = shuffledTileCoords.Dequeue();
		shuffledTileCoords.Enqueue(randomCoord);
		return randomCoord;
    }

	public void GetRandomOpenXY(out int x, out int y)
	{
		Coord randomCoord = shuffledOpenTileCoords.Dequeue();
		shuffledOpenTileCoords.Enqueue(randomCoord);
		x = randomCoord.x;
		y = randomCoord.y;
	}
	private void SpawnObstacles(int seed, int obstaclePercent)
    {
		int mapWidth = MapGenerator.Instance.GetGrid().GetWidth();
		int mapHeight = MapGenerator.Instance.GetGrid().GetHeight();
		obstacleMap = new bool[mapWidth, mapHeight];
		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				obstacleMap[x, y] = !MapGenerator.Instance.GetGrid().GetGridObject(x, y).GetIsWalkable();
			}
		}
		List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
		int obstacleCount = (int)((float)allTileCoords.Count * obstaclePercent / 100.0);

		int currentObstacleCount = 0;
		for (int i=0; i < obstacleCount; i++)
        { 
			Coord randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;
			if (MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				Vector3 obstaclePosition = MapGenerator.Instance.GetGrid().GetCenteredWorldPosition(randomCoord.x,randomCoord.y);
				GameObject newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
				newObstacle.transform.parent = MapGenerator.Instance.mapHolder.transform;
				MapGenerator.Instance.GetGrid().GetGridObject(randomCoord.x, randomCoord.y).SetIsWalkable(false);
				newObstacle.name = obstaclePrefab.name;
				allOpenCoords.Remove(randomCoord);
            }
            else
            {
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
        }
		if (seed == 0)
		{
			shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), DateTime.Now.ToString().GetHashCode()));
		}
		else
		{
			shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), seed.GetHashCode()));
		}
		int longestDistance = 0;
		Coord bestCoord = new Coord();
		foreach(Coord coord in shuffledOpenTileCoords)
        {
			if (startCoord.ManhattanDistance(coord)> longestDistance)
            {
				longestDistance = startCoord.ManhattanDistance(coord);
				bestCoord = coord;
			}
        }
		endCoord = bestCoord;
	}
	
	private bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {

		bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		for (int x = 0; x < mapFlags.GetLength(0); x++)
		{
			for (int y = 0; y < mapFlags.GetLength(1); y++)
			{
				mapFlags[x, y] = !MapGenerator.Instance.GetGrid().GetGridObject(x, y).GetIsWalkable();
			}
		}

		Queue<Coord> queue = new Queue<Coord>();
		
		int accessibleTileCount = 1;
		
		int targetAccesibleTileCount = allTileCoords.Count;
		queue.Enqueue(startCoord);
		mapFlags[startCoord.x, startCoord.y] = true;
		

		while (queue.Count > 0)
        {
			Coord tile = queue.Dequeue();
			for (int x = -1; x <= 1; x++)
            {
				for (int y = -1; y <= 1; y++)
				{
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x==0 ^ y == 0)
                    {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
							if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
								mapFlags[neighbourX, neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX, neighbourY));
								accessibleTileCount++;

							}
                        }

					}
				}
			}
        }
		return (targetAccesibleTileCount - currentObstacleCount) == accessibleTileCount;
	}
}