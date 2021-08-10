using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator: MonoBehaviour
{
	public static MapGenerator Instance { get; private set; }
	private  Grid<Coord> map;
	public Transform mapHolder;
	public bool mapIsReady = false;
	public event System.Action OnMapFinished;
	void Awake()
	{
		Instance = this;
	}
	public void GenerateMap(int mapWidth, int mapHeight, float cellSize , int seed, int fillPercent, int obstaclePercent)
	{
		mapIsReady = false;
		string holderName = "Generated Map";
		if (transform.Find(holderName))
		{
			DestroyImmediate(transform.Find(holderName).gameObject);
		}
		mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = this.transform;

		Vector3 originPosition = new Vector3(-mapWidth / 2 * cellSize, -mapHeight / 2 * cellSize);
		map = new Grid<Coord>(mapWidth,mapHeight , cellSize, originPosition, (Grid<Coord> g, int x, int y) => new Coord(g, x, y));

		RandomFillMap(seed, fillPercent);

		for (int i = 0; i < 5; i++)
		{
			SmoothMap();
		}
		
		ProcessMap();
		ClearIlegalTiles();
		ObjectSpawner.Instance.SetupObstacles(seed, obstaclePercent);
		TilemapVisual.Instance.GenerateTilemap();

		mapIsReady = true;
		OnMapFinished();


	}
	public Grid<Coord> GetGrid()
    {
		return map;
    }


	List<List<Coord>> GetRegions(Coord.CellType tileType)
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		bool[,] mapFlags = new bool[map.GetWidth(), map.GetHeight()];
		for (int x = 0; x < map.GetWidth(); x++)
		{
			for (int y = 0; y < map.GetHeight(); y++)
			{
				if (!mapFlags[x,y] && map.GetGridObject(x, y).GetCellType() == tileType)
                {
					List<Coord> newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);

					foreach(Coord tile in newRegion)
                    {
						mapFlags[tile.x, tile.y] = true;
                    }
				}
			}
		}
		return regions;
	}

	void ProcessMap()
    {
		
		List<List<Coord>>  wallRegions = GetRegions(Coord.CellType.wall);
		int wallThresholdSize = 50;
		foreach (List<Coord> wallRegion in wallRegions)
        {
			if (wallRegion.Count < wallThresholdSize)
            {
				foreach(Coord tile in wallRegion)
                {
					map.GetGridObject(tile.x, tile.y).SetCellType(Coord.CellType.floor);
                }
            }	
        }

		List<List<Coord>> roomRegions = GetRegions(Coord.CellType.floor);
		int roomThresholdSize = 50;
		List<Room> survivingRooms = new List<Room>();
		foreach (List<Coord> roomRegion in roomRegions)
		{
			if (roomRegion.Count < roomThresholdSize)
			{
				foreach (Coord tile in roomRegion)
				{
					map.GetGridObject(tile.x, tile.y).SetCellType(Coord.CellType.wall);
				}
            }
            else
            {
				survivingRooms.Add(new Room(roomRegion, map));
            }
		}
		survivingRooms.Sort();
		survivingRooms[0].isMainRoom = true;
		survivingRooms[0].isAccessibleFromMainRoom = true;

		ConnectClosestRooms(survivingRooms);
	}
	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) 
	{
		List<Room> roomListA = new List<Room>();
		List<Room> roomListB = new List<Room>();

		if (forceAccessibilityFromMainRoom)
        {
			foreach(Room room in allRooms)
            {
				if (room.isAccessibleFromMainRoom)
                {
					roomListB.Add(room);
                }
                else
                {
					roomListA.Add(room);
				}
            }
        }
        else
        {
			roomListA = allRooms;
			roomListB = allRooms;
        }
		int bestDistance = 0;
		Coord bestTileA = new Coord();
		Coord bestTileB = new Coord();
		Room bestRoomA = new Room();
		Room bestRoomB = new Room();
		bool possibleConnectionFound = false;
		foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
				possibleConnectionFound = false;
				if (roomA.connectedRooms.Count > 0)
                {
					continue;
                }
			}
			foreach (Room roomB in roomListB)
            {
				if(roomA == roomB || roomA.IsConnected(roomB))
                {
					continue;
                }
				
				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
					{
						Coord TileA = roomA.edgeTiles[tileIndexA];
						Coord TileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow((TileA.x - TileB.x), 2f) + Mathf.Pow((TileA.y - TileB.y), 2f));
						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) 
                        {
							possibleConnectionFound = true;
							bestTileA = TileA;
							bestTileB = TileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
							bestDistance = distanceBetweenRooms;

						}
					}
				}
            }
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}
		if (possibleConnectionFound && forceAccessibilityFromMainRoom)
		{
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(allRooms, true);
		}
		if (!forceAccessibilityFromMainRoom)
        {
			ConnectClosestRooms(allRooms, true);
        }
	}

	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
		Room.ConnectRooms(roomA, roomB);
		List<Coord> line = GetLine(tileA, tileB);
		int radious = 1;
		foreach(Coord c in line)
        {
			DrawCircle(c, radious);
        }
    }
	void DrawCircle(Coord c, int r)
    {
		for (int x = -r; x <= r; x++)
        {
			for (int y = -r; y <= r; y++)
			{
				if (x*x + y*y <= r * r)
                {
					int realX = c.x + x;
					int realY = c.y + y;
					if (IsInMapRange(realX, realY))
					{
						map.GetGridObject(realX, realY).SetCellType(Coord.CellType.floor);
                    }
                }
			}
		}
    }
	List<Coord> GetLine(Coord from, Coord to)
    {
		List < Coord > line = new List<Coord>();
		int x = from.x;
		int y = from.y;
		int dx = to.x - from.x;
		int dy = to.y - from.y;

		bool inverted = false;

		int step = (int) Mathf.Sign(dx);
		int gradientSep = (int)Mathf.Sign(dy);

		int longest = Mathf.Abs(dx);
		int shortest = Mathf.Abs(dy);
		if (longest < shortest)
        {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);
			step = (int) Mathf.Sign(dy);
			gradientSep = (int) Mathf.Sign(dx);
		}
		int gradientAccumulation = longest / 2;
		for(int i = 0; i<longest; i++)
        {
			line.Add(new Coord(x, y));
			if (inverted)
            {
				y += step;
            }
            else
            {
				x += step;
            }
			gradientAccumulation += shortest;
			if(gradientAccumulation>= longest)
            {
				if (inverted)
                {
					x += gradientSep;
                }
                else
                {
					y += gradientSep;
				}
				gradientAccumulation -= longest;
            }

        }
		return line;
	}


	List<Coord> GetRegionTiles(int startX, int startY)
    {
		List<Coord> tiles = new List<Coord>();
		bool[,] mapFlags = new bool[map.GetWidth(), map.GetHeight()];
		Coord.CellType tileType = map.GetGridObject(startX, startY).GetCellType(); 

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		mapFlags[startX, startY] = true;

        while (queue.Count > 0)
        {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);
			for( int x= tile.x - 1; x <= tile.x + 1; x++)
            {
				for (int y = tile.y - 1; y <= tile.y + 1; y++)
				{
					if (IsInMapRange(x,y) && (y == tile.y || x == tile.x))
                    {
						if (!mapFlags[x,y]  && map.GetGridObject(x, y).GetCellType() == tileType)
                        {
							mapFlags[x, y] = true;
							queue.Enqueue(new Coord(x, y));
                        }
                    }
				}
			}
        }
		return tiles;

	}

	bool IsInMapRange (int x, int y)
    {
		return x >= 0 && x < map.GetWidth() && y >= 0 && y < map.GetHeight();

	}
	void RandomFillMap(int seed, int randomFillPercent)
	{
		System.Random pseudoRandom;
		if (seed == 0)
		{
			pseudoRandom = new System.Random(DateTime.Now.ToString().GetHashCode());
		}
		else
		{
			pseudoRandom = new System.Random(seed.GetHashCode());
		}

		for (int x = 0; x < map.GetWidth(); x++)
		{
			for (int y = 0; y < map.GetHeight(); y++)
			{
				if (x == 0 || x == map.GetWidth() - 1 || y == 0 || y == map.GetHeight() - 1)
				{
					map.GetGridObject(x, y).SetCellType(Coord.CellType.wall);
				}
				else
				{
					map.GetGridObject(x, y).SetCellType((pseudoRandom.Next(0, 100) < randomFillPercent) ? Coord.CellType.wall : Coord.CellType.floor);
				}
			}
		}
	}

	void SmoothMap()
	{
		for (int x = 0; x < map.GetWidth(); x++)
		{
			for (int y = 0; y < map.GetHeight(); y++)
			{
				int neighbourWallTiles = GetSurroundingWallCount(x, y);

				if (neighbourWallTiles > 4)
					map.GetGridObject(x, y).SetCellType(Coord.CellType.wall);
				else if (neighbourWallTiles < 4)
					map.GetGridObject(x, y).SetCellType(Coord.CellType.floor);

			}
		}
	}

	void ClearIlegalTiles()
	{
		for (int x = 0; x < map.GetWidth(); x++)
		{
			for (int y = 0; y < map.GetHeight(); y++)
			{
				if (IsTileIlegal(x, y))
				{
					map.GetGridObject(x, y).SetCellType(Coord.CellType.floor);
				}
			}
		}
	}

	bool IsTileIlegal(int gridX, int gridY)
    {
			bool isIlegal = true;
			int tileCount = 0;
			Coord.CellType[] ilegalDef;
			ilegalDef = new Coord.CellType[9] 
				{Coord.CellType.undefined , Coord.CellType.floor, Coord.CellType.undefined,
				Coord.CellType.undefined , Coord.CellType.wall, Coord.CellType.undefined,
				Coord.CellType.undefined , Coord.CellType.floor, Coord.CellType.undefined};
			Coord.CellType[] tileDef;
			tileDef = new Coord.CellType[9];
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{ 
				for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
				{
					if (IsInMapRange(neighbourX, neighbourY))
					{
						tileDef[tileCount] = map.GetGridObject(neighbourX, neighbourY).GetCellType();
						tileCount++;
					}
				}
			}
			for (int i = 0; i < ilegalDef.Length; i++)
            {
				if (ilegalDef[i] != Coord.CellType.undefined && tileDef[i] != ilegalDef[i])
                {
					isIlegal = false;
					
				}
					
            }
			return isIlegal;
	}

	int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (IsInMapRange(neighbourX ,neighbourY))
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						if(map.GetGridObject(neighbourX, neighbourY).GetCellType() == Coord.CellType.wall)
                        {
							wallCount++;
						}
					}
				}
				else
				{
					wallCount++;
				}
			}
		}

		return wallCount;
	}


	class Room : IComparable<Room>
	{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;

		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;
		public Room()
        {

        }
		public Room(List<Coord> roomTiles, Grid<Coord> map)
		{
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();
			edgeTiles = new List<Coord>();

			foreach (Coord tile in tiles)
			{
				for (int x = tile.x - 1; x <= tile.x + 1; x++)
				{
					for (int y = tile.y - 1; y <= tile.y + 1; y++)
					{
						if (x == tile.x || y == tile.y)
						{
							if (map.GetGridObject(x,y).GetCellType() == Coord.CellType.wall)
							{
								edgeTiles.Add(tile);
							}
						}
					}

				}
			}
		}

		public void SetAccessileFromMainRoom()
        {
			if (!isAccessibleFromMainRoom)
            {
				isAccessibleFromMainRoom = true;
				foreach(Room connectedRoom in connectedRooms)
                {
					connectedRoom.SetAccessileFromMainRoom();
                }
            }
        }
		public static void ConnectRooms(Room roomA, Room roomB)
		{ 
			if (roomA.isAccessibleFromMainRoom)
            {
				roomB.SetAccessileFromMainRoom();
			}
			else if (roomB.isAccessibleFromMainRoom)
            {
				roomA.SetAccessileFromMainRoom();
			}
			roomA.connectedRooms.Add(roomB);
			roomB.connectedRooms.Add(roomA);
		}
		public bool IsConnected(Room otherRoom)
        {
			return connectedRooms.Contains(otherRoom);
        }

		public int CompareTo(Room otherRoom)
        {
			return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
	
	


}