using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Coord
{
	public int x;
	public int y;
	private Grid<Coord> grid;
	public enum CellType { wall, floor, undefined, occupied}
	private CellType cellType;
	private bool isWalkable;
	public Coord()
	{

	}
	public Coord(int x, int y)
	{
		this.x = x;
		this.y = y;
		this.cellType = CellType.undefined;
		this.isWalkable = false;
	}
	public Coord(int x, int y, CellType cellType)
	{
		this.x = x;
		this.y = y;
		this.cellType = cellType;
		this.isWalkable = (CellType.floor == cellType);
	}
	public Coord(Grid<Coord> grid, int x, int y)
	{
		this.grid = grid;
		this.x = x;
		this.y = y;
		this.cellType = CellType.undefined;
		this.isWalkable = false;
	}

	public Vector3 GetWorldPos()
    {
		return grid.GetCenteredWorldPosition(x, y);
    }
	public CellType GetCellType()
    {
		return cellType;
    }

	public void SetIsWalkable(bool isWalkable)
    { 
		this.isWalkable = isWalkable;
		grid.TriggerGridObjectChanged(x, y);
	}

	public bool GetIsWalkable()
    {
		return isWalkable;
    }

	public void SetCellType(CellType cellType)
    {
		this.cellType = cellType;
		this.isWalkable = (cellType == CellType.floor);
		grid.TriggerGridObjectChanged(x, y);
    }
	public int ManhattanDistance( Coord c)
    {
		return Mathf.Abs(this.x-c.x + this.y -c.y);
    }
	public static bool operator == (Coord c1, Coord c2)
	{
		return (c1.x == c2.x) && (c1.y == c2.y);

	}
	public static bool operator != (Coord c1, Coord c2)
	{
		return (c1.x != c2.x) || (c1.y != c2.y);
	}
	public static Coord operator + (Coord c1, Coord c2)
	{
		return new Coord((c1.x + c2.x) , (c1.y + c2.y));

	}
	public static Coord operator -(Coord c1, Coord c2)
	{
		return new Coord((c1.x - c2.x), (c1.y - c2.y));

	}
	public override string ToString()
    {
		return  cellType.ToString();
    }
}
