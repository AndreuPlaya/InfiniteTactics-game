using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisual : MonoBehaviour
{
	public Tilemap backgroundMap;
	public Tilemap foregroundMap;
	public TileBase floorTile;
	public TileBase wallTile;
   
    public void GenerateTilemap()
	{
		GenerateBackground(floorTile);
		GenerateForeground(wallTile);
	}

	private void GenerateBackground(TileBase floorTile)
	{
		backgroundMap.ClearAllTiles();

		int mapSizeX = MapGenerator.Instance.GetGrid().GetWidth();
		int mapSizeY = MapGenerator.Instance.GetGrid().GetHeight();
		for (int x = 0; x < mapSizeX; x++)
		{
			for (int y = 0; y < mapSizeY; y++)
			{
				if (MapGenerator.Instance.GetGrid().GetGridObject(x, y).GetCellType() == Coord.CellType.floor)
				{
					backgroundMap.SetTile(new Vector3Int(Mathf.FloorToInt(x - mapSizeX / 2), Mathf.FloorToInt(y - mapSizeY / 2), 0), floorTile);
				}
			}
		}
	}

	private void GenerateForeground(TileBase wallTile)
	{
		foregroundMap.ClearAllTiles();
		int borderSize = 10;
		int mapSizeX = MapGenerator.Instance.GetGrid().GetWidth() + 2 * borderSize;
		int mapSizeY = MapGenerator.Instance.GetGrid().GetHeight() + 2 * borderSize;

		for (int x = 0; x < mapSizeX; x++)
		{
			for (int y = 0; y < mapSizeY; y++)
			{
				if (x >= borderSize && y >= borderSize && x < mapSizeX - borderSize && y < mapSizeY - borderSize)
				{
					if (MapGenerator.Instance.GetGrid().GetGridObject(x - borderSize, y - borderSize).GetCellType() == Coord.CellType.wall)
					{
						foregroundMap.SetTile(new Vector3Int(Mathf.FloorToInt(x - mapSizeX / 2), Mathf.FloorToInt(y - mapSizeY / 2), 0), wallTile);
					}
				}
				else
				{
					foregroundMap.SetTile(new Vector3Int(Mathf.FloorToInt(x - mapSizeX / 2), Mathf.FloorToInt(y - mapSizeY / 2), 0), wallTile);
				}

			}
		}
	}
}
